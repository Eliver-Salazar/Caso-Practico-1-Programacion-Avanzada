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
    public class OrderItemsController : Controller
    {
        private readonly OrderItemManager _manager = new OrderItemManager();
        private readonly OrderManager _orders = new OrderManager();
        private readonly CustomerManager _customers = new CustomerManager();
        private readonly BookManager _books = new BookManager();

        // Muestra las operaciones dentro de la única vista Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new OrderItem { Quantity = 1 };

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

        // Recibe únicamente los campos editables del artículo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,OrderId,BookId,Quantity,UnitPrice,Discount")]
            OrderItem item)
        {
            string mode = item.Id == 0 ? "create" : "edit";

            // Valida la existencia del pedido y del libro.
            if (_orders.GetById(item.OrderId) == null)
            {
                ModelState.AddModelError(
                    "OrderId", "Seleccioná un pedido válido.");
            }

            if (!_books.GetAllBooksFromDataModel()
                .Any(book => book.Id == item.BookId))
            {
                ModelState.AddModelError(
                    "BookId", "Seleccioná un libro válido.");
            }

            if (item.Quantity <= 0)
            {
                ModelState.AddModelError(
                    "Quantity", "La cantidad debe ser mayor que cero.");
            }

            // Respeta el tamaño y los decimales de los campos monetarios.
            ValidateAmount("UnitPrice", item.UnitPrice);
            ValidateAmount("Discount", item.Discount);

            // El descuento es un monto aplicado a toda la línea.
            if (item.Quantity > 0 &&
                item.UnitPrice >= 0 &&
                item.UnitPrice <= 9999999999999999.99m)
            {
                decimal grossAmount = item.Quantity * item.UnitPrice;

                if (item.Discount > grossAmount)
                {
                    ModelState.AddModelError(
                        "Discount",
                        "El descuento no puede superar cantidad × precio.");
                }

                if (grossAmount - item.Discount > 9999999999999999.99m)
                {
                    ModelState.AddModelError(
                        "", "El importe de la línea supera el máximo permitido.");
                }
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, item);

            try
            {
                if (item.Id == 0)
                {
                    _manager.Add(item);
                }
                else
                {
                    // Recupera el registro para modificar sus campos.
                    var existing = _manager.GetById(item.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.OrderId = item.OrderId;
                    existing.BookId = item.BookId;
                    existing.Quantity = item.Quantity;
                    existing.UnitPrice = item.UnitPrice;
                    existing.Discount = item.Discount;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Artículo del pedido guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar el artículo. Revisá los datos.");

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
                TempData["Success"] = "Artículo del pedido eliminado.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudo eliminar el artículo.";
            }

            return RedirectToAction("Index");
        }

        // Prepara los selectores y las etiquetas del listado.
        private ActionResult ShowIndex(string mode, OrderItem selected)
        {
            var customers = _customers.GetAll().ToDictionary(
                customer => customer.Id,
                customer => customer.FirstName + " " + customer.LastName);

            var orders = _orders.GetAll().Select(order => new
            {
                order.Id,
                Label = "#" + order.Id + " — " +
                    (customers.ContainsKey(order.CustomerId)
                        ? customers[order.CustomerId]
                        : "Cliente #" + order.CustomerId)
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
                orders, "Id", "Label", selected.OrderId);

            ViewBag.BookOptions = new SelectList(
                books, "Id", "Label", selected.BookId);

            ViewBag.OrderNames = orders.ToDictionary(
                order => order.Id, order => order.Label);

            ViewBag.BookNames = books.ToDictionary(
                book => book.Id, book => book.Label);

            return View("Index", _manager.GetAll());
        }

        // Valida importes no negativos y con máximo dos decimales.
        private void ValidateAmount(string field, decimal value)
        {
            if (value < 0 || value > 9999999999999999.99m)
            {
                ModelState.AddModelError(
                    field, "Ingresá un importe válido, no negativo.");
            }
            else if (decimal.Round(value, 2) != value)
            {
                ModelState.AddModelError(
                    field, "Usá máximo dos decimales.");
            }
        }
    }
}