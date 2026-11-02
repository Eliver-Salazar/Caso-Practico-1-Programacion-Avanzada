using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using PAP.DataAccess;

namespace PAP.Mvc.Controllers
{
    public class PurchaseOrdersController : Controller
    {
        private readonly PurchaseOrderManager _manager =
            new PurchaseOrderManager();

        private readonly SupplierManager _suppliers = new SupplierManager();

        // Muestra el listado y la operación seleccionada en Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new PurchaseOrder
            {
                Status = "Pending",
                OrderDate = DateTime.UtcNow
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

        // La fecha de creación y el total se controlan desde el servidor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,SupplierId,ExpectedDate,Status")]
            PurchaseOrder order)
        {
            string mode = order.Id == 0 ? "create" : "edit";

            // Recupera la orden original cuando se está editando.
            var existing = order.Id == 0
                ? null
                : _manager.GetById(order.Id);

            if (order.Id != 0 && existing == null)
                return HttpNotFound();

            // Conserva las fechas y el total aunque falle la validación.
            order.OrderDate = existing == null
                ? DateTime.UtcNow
                : existing.OrderDate;

            order.TotalCost = existing == null ? 0m : existing.TotalCost;

            // Verifica que exista el proveedor seleccionado.
            if (order.SupplierId <= 0 ||
                _suppliers.GetById(order.SupplierId) == null)
            {
                ModelState.AddModelError(
                    "SupplierId", "Seleccioná un proveedor válido.");
            }

            // Valida los estados ofrecidos por este módulo.
            string[] allowedStatuses =
            {
                "Pending", "Ordered", "Received", "Cancelled"
            };

            if (!allowedStatuses.Contains(order.Status))
            {
                ModelState.AddModelError(
                    "Status", "Seleccioná un estado válido.");
            }

            // La fecha esperada es opcional y no debe preceder a la orden.
            if (order.ExpectedDate.HasValue)
            {
                order.ExpectedDate = order.ExpectedDate.Value.Date;

                if (order.ExpectedDate.Value < order.OrderDate.Date)
                {
                    ModelState.AddModelError(
                        "ExpectedDate",
                        "La fecha esperada no puede ser anterior a la orden.");
                }
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, order);

            try
            {
                if (existing == null)
                {
                    // Inicia en cero; el total vendrá de sus artículos.
                    _manager.Add(order);
                }
                else
                {
                    existing.SupplierId = order.SupplierId;
                    existing.ExpectedDate = order.ExpectedDate;
                    existing.Status = order.Status;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Orden de compra guardada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar la orden. Revisá los datos.");

                return ShowIndex(mode, order);
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
                TempData["Success"] = "Orden de compra eliminada.";
            }
            catch (Exception)
            {
                // La base protege las órdenes que tienen artículos asociados.
                TempData["Error"] =
                    "No se pudo eliminar. Verificá si contiene artículos.";
            }

            return RedirectToAction("Index");
        }

        // Prepara los proveedores para el selector y el listado.
        private ActionResult ShowIndex(string mode, PurchaseOrder selected)
        {
            var suppliers = _suppliers.GetAll().ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedOrder = selected;

            ViewBag.SupplierOptions = new SelectList(
                suppliers.Select(supplier => new
                {
                    supplier.Id,
                    Label = "#" + supplier.Id + " — " + supplier.Name
                }),
                "Id", "Label", selected.SupplierId);

            ViewBag.SupplierNames = suppliers.ToDictionary(
                supplier => supplier.Id,
                supplier => supplier.Name);

            return View("Index", _manager.GetAll());
        }
    }
}