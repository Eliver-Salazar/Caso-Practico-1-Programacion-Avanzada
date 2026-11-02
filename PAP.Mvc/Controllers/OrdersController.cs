using System;
using System.Linq;

using System.Web.Mvc;

using PAP.Business;
using PAP.DataAccess;

namespace PAP.Mvc.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderManager _manager = new OrderManager();
        private readonly CustomerManager _customers = new CustomerManager();
        private readonly AddressManager _addresses = new AddressManager();

        // Muestra todas las operaciones dentro de Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Order { Status = "Pending" };

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

        // No recibe Total ni las fechas: se establecen en el servidor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include =
                "Id,CustomerId,ShippingAddressId,Status,Subtotal,TaxAmount,ShippingCost,Notes")]
            Order order)
        {
            string mode = order.Id == 0 ? "create" : "edit";

            // Verifica que exista el cliente seleccionado.
            if (_customers.GetById(order.CustomerId) == null)
            {
                ModelState.AddModelError(
                    "CustomerId", "Seleccioná un cliente válido.");
            }

            // La dirección es opcional, pero debe pertenecer al cliente.
            if (order.ShippingAddressId.HasValue)
            {
                var address = _addresses.GetById(order.ShippingAddressId.Value);

                if (address == null || address.CustomerId != order.CustomerId)
                {
                    ModelState.AddModelError(
                        "ShippingAddressId",
                        "La dirección debe pertenecer al cliente seleccionado.");
                }
            }

            // Valida los estados disponibles en este módulo.
            string[] statuses =
            {
                "Pending", "Processing", "Shipped", "Delivered", "Cancelled"
            };

            if (!statuses.Contains(order.Status))
                ModelState.AddModelError("Status", "Seleccioná un estado válido.");

            // Valida importes positivos o cero y máximo dos decimales.
            ValidateAmount("Subtotal", order.Subtotal);
            ValidateAmount("TaxAmount", order.TaxAmount);
            ValidateAmount("ShippingCost", order.ShippingCost);

            order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                ? null
                : order.Notes.Trim();

            if (order.Notes != null && order.Notes.Length > 500)
                ModelState.AddModelError("Notes", "Máximo 500 caracteres.");

            if (!ModelState.IsValid)
                return ShowIndex(mode, order);

            // Calcula el total sin aceptar un valor enviado por el navegador.
            order.Total = order.Subtotal + order.TaxAmount + order.ShippingCost;

            if (order.Total > 9999999999999999.99m)
            {
                ModelState.AddModelError(
                    "", "El total supera el importe permitido.");

                return ShowIndex(mode, order);
            }

            try
            {
                if (order.Id == 0)
                {
                    order.OrderDate = DateTime.UtcNow;
                    order.UpdatedAt = order.OrderDate;

                    _manager.Add(order);
                }
                else
                {
                    // Conserva la fecha original del pedido.
                    var existing = _manager.GetById(order.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.CustomerId = order.CustomerId;
                    existing.ShippingAddressId = order.ShippingAddressId;
                    existing.Status = order.Status;
                    existing.Subtotal = order.Subtotal;
                    existing.TaxAmount = order.TaxAmount;
                    existing.ShippingCost = order.ShippingCost;
                    existing.Total = order.Total;
                    existing.Notes = order.Notes;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Pedido guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar el pedido. Revisá los datos.");

                return ShowIndex(mode, order);
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
                TempData["Success"] = "Pedido eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "No se pudo eliminar. Verificá si tiene artículos o pagos.";
            }

            return RedirectToAction("Index");
        }

        // Prepara los selectores y las etiquetas relacionadas.
        private ActionResult ShowIndex(string mode, Order selected)
        {
            var customers = _customers.GetAll().ToList();

            var customerNames = customers.ToDictionary(
                customer => customer.Id,
                customer => customer.FirstName + " " + customer.LastName);

            var addresses = _addresses.GetAll().Select(address => new
            {
                address.Id,
                Label = "#" + address.Id + " — " +
                    (customerNames.ContainsKey(address.CustomerId)
                        ? customerNames[address.CustomerId]
                        : "Cliente #" + address.CustomerId) +
                    " — " + address.Street + ", " + address.City
            }).ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedOrder = selected;

            ViewBag.CustomerOptions = new SelectList(
                customers.Select(customer => new
                {
                    customer.Id,
                    Label = "#" + customer.Id + " — " +
                        customer.FirstName + " " + customer.LastName +
                        " — " + customer.Email
                }),
                "Id", "Label", selected.CustomerId);

            // Muestra el cliente de cada dirección; Save comprueba su pertenencia.
            ViewBag.AddressOptions = new SelectList(
                addresses, "Id", "Label", selected.ShippingAddressId);

            ViewBag.CustomerNames = customerNames;

            ViewBag.AddressNames = addresses.ToDictionary(
                address => address.Id, address => address.Label);

            return View("Index", _manager.GetAll());
        }

        // Respeta el tamaño DECIMAL(18,2) de los importes.
        private void ValidateAmount(string field, decimal value)
        {
            if (value < 0 || value > 9999999999999999.99m)
                ModelState.AddModelError(field, "Ingresá un importe válido, no negativo.");
            else if (decimal.Round(value, 2) != value)
                ModelState.AddModelError(field, "Usá máximo dos decimales.");
        }
    }
}