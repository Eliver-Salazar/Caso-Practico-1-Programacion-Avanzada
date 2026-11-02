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
    public class PaymentsController : Controller
    {
        private readonly PaymentManager _manager = new PaymentManager();
        private readonly OrderManager _orders = new OrderManager();
        private readonly CustomerManager _customers = new CustomerManager();

        // Muestra todas las operaciones dentro de Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Payment
            {
                PaymentMethod = "Cash",
                Status = "Completed"
            };

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

        // La fecha del pago se establece en el servidor al crear.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include =
                "Id,OrderId,PaymentMethod,Amount,Status,TransactionRef")]
            Payment payment)
        {
            string mode = payment.Id == 0 ? "create" : "edit";

            // Comprueba la llave foránea del pedido.
            if (_orders.GetById(payment.OrderId) == null)
            {
                ModelState.AddModelError(
                    "OrderId", "Seleccioná un pedido válido.");
            }

            // Métodos disponibles para este módulo académico.
            string[] methods =
            {
                "Cash", "Card", "Transfer", "SINPE"
            };

            if (!methods.Contains(payment.PaymentMethod))
            {
                ModelState.AddModelError(
                    "PaymentMethod", "Seleccioná un método válido.");
            }

            // Estados disponibles para el registro del pago.
            string[] statuses =
            {
                "Pending", "Completed", "Failed", "Refunded"
            };

            if (!statuses.Contains(payment.Status))
            {
                ModelState.AddModelError(
                    "Status", "Seleccioná un estado válido.");
            }

            // Valida un monto positivo compatible con DECIMAL(18,2).
            if (payment.Amount <= 0 ||
                payment.Amount > 9999999999999999.99m)
            {
                ModelState.AddModelError(
                    "Amount", "Ingresá un monto válido, mayor que cero.");
            }
            else if (decimal.Round(payment.Amount, 2) != payment.Amount)
            {
                ModelState.AddModelError(
                    "Amount", "Usá máximo dos decimales.");
            }

            // La referencia es opcional y admite hasta 100 caracteres.
            payment.TransactionRef =
                string.IsNullOrWhiteSpace(payment.TransactionRef)
                    ? null
                    : payment.TransactionRef.Trim();

            if (payment.TransactionRef != null &&
                payment.TransactionRef.Length > 100)
            {
                ModelState.AddModelError(
                    "TransactionRef", "Máximo 100 caracteres.");
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, payment);

            try
            {
                if (payment.Id == 0)
                {
                    payment.PaymentDate = DateTime.UtcNow;
                    _manager.Add(payment);
                }
                else
                {
                    // Conserva la fecha original del pago.
                    var existing = _manager.GetById(payment.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.OrderId = payment.OrderId;
                    existing.PaymentMethod = payment.PaymentMethod;
                    existing.Amount = payment.Amount;
                    existing.Status = payment.Status;
                    existing.TransactionRef = payment.TransactionRef;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Pago guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar el pago. Revisá los datos.");

                return ShowIndex(mode, payment);
            }
        }

        // Requiere confirmación mediante POST para eliminar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (_manager.GetById(id) == null)
                return HttpNotFound();

            try
            {
                _manager.Delete(id);
                TempData["Success"] = "Pago eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudo eliminar el pago.";
            }

            return RedirectToAction("Index");
        }

        // Prepara el selector con número de pedido, cliente y total.
        private ActionResult ShowIndex(string mode, Payment selected)
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
                        : "Cliente #" + order.CustomerId) +
                    " — Total: " + order.Total.ToString("N2")
            }).ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedPayment = selected;

            ViewBag.OrderOptions = new SelectList(
                orders, "Id", "Label", selected.OrderId);

            ViewBag.OrderNames = orders.ToDictionary(
                order => order.Id, order => order.Label);

            return View("Index", _manager.GetAll());
        }
    }
}