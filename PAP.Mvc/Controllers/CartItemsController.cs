using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using PAP.DataAccess;

namespace PAP.Mvc.Controllers
{
    public class CartItemsController : Controller
    {
        private readonly CartItemManager _manager = new CartItemManager();
        private readonly ShoppingCartManager _carts = new ShoppingCartManager();
        private readonly CustomerManager _customers = new CustomerManager();
        private readonly BookManager _books = new BookManager();

        // Muestra el listado y la operación seleccionada en Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new CartItem { Quantity = 1 };

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

        // La fecha de incorporación se controla desde el servidor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,CartId,BookId,Quantity")] CartItem item)
        {
            string mode = item.Id == 0 ? "create" : "edit";

            // Verifica ambas llaves foráneas.
            bool validCart = _carts.GetById(item.CartId) != null;
            bool validBook = _books.GetAllBooksFromDataModel()
                .Any(book => book.Id == item.BookId);

            if (!validCart)
                ModelState.AddModelError("CartId", "Seleccioná un carrito válido.");

            if (!validBook)
                ModelState.AddModelError("BookId", "Seleccioná un libro válido.");

            // La cantidad debe ser un entero mayor que cero.
            if (item.Quantity <= 0)
                ModelState.AddModelError("Quantity", "La cantidad debe ser mayor que cero.");

            if (validCart && validBook &&
                _manager.ItemExists(item.CartId, item.BookId, item.Id))
            {
                ModelState.AddModelError(
                    "BookId",
                    "Ese libro ya está en el carrito. Editá su cantidad.");
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, item);

            try
            {
                if (item.Id == 0)
                {
                    item.AddedAt = DateTime.UtcNow;
                    _manager.Add(item);
                }
                else
                {
                    // Conserva la fecha original de incorporación.
                    var existing = _manager.GetById(item.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.CartId = item.CartId;
                    existing.BookId = item.BookId;
                    existing.Quantity = item.Quantity;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Artículo guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // La base también protege la combinación carrito-libro.
                ModelState.AddModelError(
                    "", "No se pudo guardar. Revisá el carrito, libro y cantidad.");

                return ShowIndex(mode, item);
            }
        }

        // Elimina después de confirmar mediante POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (_manager.GetById(id) == null)
                return HttpNotFound();

            try
            {
                _manager.Delete(id);
                TempData["Success"] = "Artículo eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudo eliminar el artículo.";
            }

            return RedirectToAction("Index");
        }

        // Prepara los selectores y sus etiquetas para la única vista.
        private ActionResult ShowIndex(string mode, CartItem selected)
        {
            var customers = _customers.GetAll().ToDictionary(
                customer => customer.Id,
                customer => customer.FirstName + " " + customer.LastName);

            var carts = _carts.GetAll().Select(cart => new
            {
                cart.Id,
                Label = "#" + cart.Id + " — " +
                    (customers.ContainsKey(cart.CustomerId)
                        ? customers[cart.CustomerId]
                        : "Cliente #" + cart.CustomerId)
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

            ViewBag.CartOptions = new SelectList(
                carts, "Id", "Label", selected.CartId);

            ViewBag.BookOptions = new SelectList(
                books, "Id", "Label", selected.BookId);

            ViewBag.CartNames = carts.ToDictionary(cart => cart.Id, cart => cart.Label);
            ViewBag.BookNames = books.ToDictionary(book => book.Id, book => book.Label);

            return View("Index", _manager.GetAll());
        }
    }
}