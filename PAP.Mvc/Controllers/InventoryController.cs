using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using PAP.DataAccess;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryManager _manager = new InventoryManager();

        // Reutiliza el manager de libros que ya existe en el proyecto.
        private readonly BookManager _books = new BookManager();

        // Muestra el listado y la operación seleccionada en Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Inventory { ReorderLevel = 5 };

            if (mode == "edit" || mode == "details" || mode == "delete")
            {
                if (!id.HasValue)
                    return HttpNotFound();

                selected = _manager.GetById(id.Value);

                if (selected == null)
                    return HttpNotFound();
            }
            else if (mode != "create")
            {
                mode = "";
            }

            return ShowIndex(mode, selected);
        }

        // Recibe los campos editables del inventario.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include =
                "Id,BookId,QuantityInStock,ReorderLevel,LastRestockedAt")]
            Inventory inventory)
        {
            string mode = inventory.Id == 0 ? "create" : "edit";

            // Comprueba que el libro exista.
            bool validBook = _books.GetAllBooksFromDataModel()
                .Any(book => book.Id == inventory.BookId);

            if (!validBook)
            {
                ModelState.AddModelError(
                    "BookId", "Seleccioná un libro válido.");
            }
            else if (_manager.BookExists(inventory.BookId, inventory.Id))
            {
                ModelState.AddModelError(
                    "BookId", "Ese libro ya tiene un registro de inventario.");
            }

            // No permite cantidades ni niveles negativos.
            if (inventory.QuantityInStock < 0)
            {
                ModelState.AddModelError(
                    "QuantityInStock", "Las existencias no pueden ser negativas.");
            }

            if (inventory.ReorderLevel < 0)
            {
                ModelState.AddModelError(
                    "ReorderLevel", "El nivel de reposición no puede ser negativo.");
            }

            // Este formulario registra la fecha de reposición sin hora.
            if (inventory.LastRestockedAt.HasValue)
            {
                inventory.LastRestockedAt =
                    inventory.LastRestockedAt.Value.Date;

                if (inventory.LastRestockedAt.Value > DateTime.Today)
                {
                    ModelState.AddModelError(
                        "LastRestockedAt", "La fecha no puede ser futura.");
                }
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, inventory);

            try
            {
                if (inventory.Id == 0)
                {
                    _manager.Add(inventory);
                }
                else
                {
                    var existing = _manager.GetById(inventory.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.BookId = inventory.BookId;
                    existing.QuantityInStock = inventory.QuantityInStock;
                    existing.ReorderLevel = inventory.ReorderLevel;
                    existing.LastRestockedAt = inventory.LastRestockedAt;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Inventario guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // La base de datos también protege el libro único.
                ModelState.AddModelError(
                    "", "No se pudo guardar. Revisá los datos y el libro.");

                return ShowIndex(mode, inventory);
            }
        }

        // Elimina únicamente después de confirmar mediante POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (_manager.GetById(id) == null)
                return HttpNotFound();

            try
            {
                _manager.Delete(id);
                TempData["Success"] = "Registro de inventario eliminado.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudo eliminar el registro.";
            }

            return RedirectToAction("Index");
        }

        // Prepara el selector y los nombres de libros para la única vista.
        private ActionResult ShowIndex(string mode, Inventory selected)
        {
            var books = _books.GetAllBooksFromDataModel().ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedInventory = selected;

            ViewBag.BookOptions = new SelectList(
                books.Select(book => new
                {
                    book.Id,
                    Label = "#" + book.Id + " — " +
                        (string.IsNullOrWhiteSpace(book.Title)
                            ? book.Name
                            : book.Title)
                }),
                "Id",
                "Label",
                selected.BookId);

            ViewBag.BookNames = books.ToDictionary(
                book => book.Id,
                book => string.IsNullOrWhiteSpace(book.Title)
                    ? book.Name
                    : book.Title);

            return View("Index", _manager.GetAll());
        }
    }
}