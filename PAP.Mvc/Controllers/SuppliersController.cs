using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using PAP.DataAccess;
using System.Net.Mail;

namespace PAP.Mvc.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly SupplierManager _manager = new SupplierManager();

        // Muestra la tabla y la operación seleccionada dentro de Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            // Los proveedores nuevos estarán activos inicialmente.
            var selected = new Supplier { IsActive = true };

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

        // Recibe únicamente los campos editables del proveedor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,Name,ContactName,Email,Phone,Address,IsActive")]
            Supplier supplier)
        {
            string mode = supplier.Id == 0 ? "create" : "edit";

            // Limpia espacios y normaliza los campos opcionales.
            supplier.Name = Clean(supplier.Name);
            supplier.ContactName = Clean(supplier.ContactName);
            supplier.Email = Clean(supplier.Email);
            supplier.Phone = Clean(supplier.Phone);
            supplier.Address = Clean(supplier.Address);

            // Valida los tamaños definidos en la base de datos.
            if (supplier.Name == null)
                ModelState.AddModelError("Name", "El nombre es obligatorio.");
            else if (supplier.Name.Length > 200)
                ModelState.AddModelError("Name", "Máximo 200 caracteres.");

            if (supplier.ContactName != null &&
                supplier.ContactName.Length > 150)
            {
                ModelState.AddModelError(
                    "ContactName", "Máximo 150 caracteres.");
            }

            if (supplier.Phone != null && supplier.Phone.Length > 30)
                ModelState.AddModelError("Phone", "Máximo 30 caracteres.");

            if (supplier.Address != null && supplier.Address.Length > 300)
                ModelState.AddModelError("Address", "Máximo 300 caracteres.");

            // El correo es opcional; si se indica, debe tener formato válido.
            if (supplier.Email != null)
            {
                if (supplier.Email.Length > 150)
                {
                    ModelState.AddModelError("Email", "Máximo 150 caracteres.");
                }
                else
                {
                    try
                    {
                        var email = new MailAddress(supplier.Email);

                        if (email.Address != supplier.Email)
                            ModelState.AddModelError("Email", "Correo inválido.");
                    }
                    catch (FormatException)
                    {
                        ModelState.AddModelError("Email", "Correo inválido.");
                    }
                }
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, supplier);

            try
            {
                if (supplier.Id == 0)
                {
                    _manager.Add(supplier);
                }
                else
                {
                    // Actualiza únicamente los campos permitidos.
                    var existing = _manager.GetById(supplier.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.Name = supplier.Name;
                    existing.ContactName = supplier.ContactName;
                    existing.Email = supplier.Email;
                    existing.Phone = supplier.Phone;
                    existing.Address = supplier.Address;
                    existing.IsActive = supplier.IsActive;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Proveedor guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // Conserva el formulario para corregir los datos.
                ModelState.AddModelError(
                    "", "No se pudo guardar el proveedor. Revisá los datos.");

                return ShowIndex(mode, supplier);
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
                TempData["Success"] = "Proveedor eliminado correctamente.";
            }
            catch (Exception)
            {
                // Las relaciones protegen los proveedores con órdenes de compra.
                TempData["Error"] =
                    "No se pudo eliminar. Verificá si tiene órdenes de compra. " +
                    "Podés desactivarlo desde Editar.";
            }

            return RedirectToAction("Index");
        }

        // Prepara la única vista utilizada por el módulo.
        private ActionResult ShowIndex(string mode, Supplier selected)
        {
            ViewBag.Mode = mode;
            ViewBag.SelectedSupplier = selected;

            return View("Index", _manager.GetAll());
        }

        // Convierte los textos vacíos en null y elimina espacios exteriores.
        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}