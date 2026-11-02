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
    public class PurchaseOrderItemsController : Controller
    {
        private readonly PurchaseOrderItemManager _manager =
            new PurchaseOrderItemManager();

        private readonly PurchaseOrderManager _orders =
            new PurchaseOrderManager();

        private readonly SupplierManager _suppliers = new SupplierManager();
        private readonly BookManager _books = new BookManager();

        // Muestra el listado y la operación seleccionada en la única vista.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new PurchaseOrderItem { Quantity = 1 };

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

        // Recibe únicamente los campos editables.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,PurchaseOrderId,BookId,Quantity,UnitCost")]
            PurchaseOrderItem item)
        {
            string mode = item.Id == 0 ? "create" : "edit";

            // Verifica que la orden seleccionada exista.
            if (_orders.GetById(item.PurchaseOrderId) == null)
            {
                ModelState.AddModelError(
                    "PurchaseOrderId", "Seleccioná una orden válida.");
            }

            // Verifica que el libro seleccionado exista.
            if (!_books.GetAllBooksFromDataModel()
                .Any(book => book.Id == item.BookId))
            {
                ModelState.AddModelError(
                    "BookId", "Seleccioná un libro válido.");
            }

            // La cantidad debe ser positiva.
            if (item.Quantity <= 0)
            {
                ModelState.AddModelError(
                    "Quantity", "La cantidad debe ser mayor que cero.");
            }

            // Respeta el tamaño DECIMAL(18,2) del costo unitario.
            const decimal maximumAmount = 9999999999999999.99m;

            if (item.UnitCost < 0 || item.UnitCost > maximumAmount)
            {
                ModelState.AddModelError(
                    "UnitCost", "Ingresá un costo válido, mayor o igual a cero.");
            }
            else if (decimal.Round(item.UnitCost, 2) != item.UnitCost)
            {
                ModelState.AddModelError(
                    "UnitCost", "Usá máximo dos decimales.");
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, item);

            try
            {
                if (item.Id == 0)
                {
                    // Agrega mediante el manager y el repository base.
                    _manager.Add(item);
                }
                else
                {
                    // Recupera el registro para modificar sus campos.
                    var existing = _manager.GetById(item.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.PurchaseOrderId = item.PurchaseOrderId;
                    existing.BookId = item.BookId;
                    existing.Quantity = item.Quantity;
                    existing.UnitCost = item.UnitCost;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Artículo de compra guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar el artículo. Revisá los datos.");

                return ShowIndex(mode, item);
            }
        }

        // La eliminación requiere confirmación mediante POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (_manager.GetById(id) == null)
                return HttpNotFound();

            try
            {
                _manager.Delete(id);
                TempData["Success"] = "Artículo de compra eliminado.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudo eliminar el artículo.";
            }

            return RedirectToAction("Index");
        }

        // Mantiene los mismos datos que necesita la vista enviada.
        private ActionResult ShowIndex(
            string mode, PurchaseOrderItem selected)
        {
            var suppliers = _suppliers.GetAll().ToDictionary(
                supplier => supplier.Id,
                supplier => supplier.Name);

            var orders = _orders.GetAll().Select(order => new
            {
                order.Id,
                Label = "#" + order.Id + " — " +
                    (suppliers.ContainsKey(order.SupplierId)
                        ? suppliers[order.SupplierId]
                        : "Proveedor #" + order.SupplierId)
            }).ToList();

            var books = _books.GetAllBooksFromDataModel().Select(book => new
            {
                book.Id,
                Label = "#" + book.Id + " — " +
                    (string.IsNullOrWhiteSpace(book.Title)
                        ? book.Name
                        : book.Title)
            }).ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedItem = selected;

            ViewBag.OrderOptions = new SelectList(
                orders, "Id", "Label", selected.PurchaseOrderId);

            ViewBag.BookOptions = new SelectList(
                books, "Id", "Label", selected.BookId);

            ViewBag.OrderNames = orders.ToDictionary(
                order => order.Id, order => order.Label);

            ViewBag.BookNames = books.ToDictionary(
                book => book.Id, book => book.Label);

            return View("Index", _manager.GetAll());
        }
    }
}