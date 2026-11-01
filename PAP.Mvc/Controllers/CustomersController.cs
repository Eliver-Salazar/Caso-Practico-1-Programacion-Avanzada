using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using PAP.DataAccess;
using System.Net.Mail;
using System.Web.Helpers;

namespace PAP.Mvc.Controllers
{
    public class CustomersController : Controller
    {
        private readonly CustomerManager _manager = new CustomerManager();

        // Muestra el listado y la operación seleccionada dentro de Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Customer { IsActive = true };

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

        // PasswordHash y las fechas no se reciben desde el navegador.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include =
                "Id,FirstName,LastName,Email,Phone,BirthDate,IsActive")]
            Customer customer,
            string password)
        {
            string mode = customer.Id == 0 ? "create" : "edit";

            // Normaliza los textos antes de validarlos.
            customer.FirstName = Clean(customer.FirstName);
            customer.LastName = Clean(customer.LastName);
            customer.Email = Clean(customer.Email);
            customer.Phone = Clean(customer.Phone);

            ValidateText("FirstName", customer.FirstName, 100, true);
            ValidateText("LastName", customer.LastName, 100, true);
            ValidateText("Email", customer.Email, 150, true);
            ValidateText("Phone", customer.Phone, 30, false);

            // Valida formato y unicidad del correo.
            if (customer.Email != null && customer.Email.Length <= 150)
            {
                try
                {
                    var email = new MailAddress(customer.Email);

                    if (email.Address != customer.Email)
                    {
                        ModelState.AddModelError("Email", "Correo inválido.");
                    }
                    else if (_manager.EmailExists(customer.Email, customer.Id))
                    {
                        ModelState.AddModelError(
                            "Email", "Ya existe un cliente con ese correo.");
                    }
                }
                catch (FormatException)
                {
                    ModelState.AddModelError("Email", "Correo inválido.");
                }
            }

            if (customer.BirthDate.HasValue &&
                customer.BirthDate.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    "BirthDate", "La fecha de nacimiento no puede ser futura.");
            }

            // La contraseña es obligatoria al crear; opcional al editar.
            if (customer.Id == 0 && string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "password", "La contraseña es obligatoria.");
            }
            else if (!string.IsNullOrEmpty(password) &&
                     (string.IsNullOrWhiteSpace(password) ||
                      password.Length < 8 || password.Length > 128))
            {
                ModelState.AddModelError(
                    "password", "Usá entre 8 y 128 caracteres, no solo espacios.");
            }

            // No conserva la contraseña dentro del estado del formulario.
            ModelState.Remove("customer.PasswordHash");

            if (!ModelState.IsValid)
                return ShowIndex(mode, customer);

            try
            {
                if (customer.Id == 0)
                {
                    // Genera un hash con sal; nunca guarda la contraseña original.
                    customer.PasswordHash = Crypto.HashPassword(password);
                    customer.CreatedAt = DateTime.UtcNow;
                    customer.UpdatedAt = customer.CreatedAt;

                    _manager.Add(customer);
                }
                else
                {
                    var existing = _manager.GetById(customer.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.FirstName = customer.FirstName;
                    existing.LastName = customer.LastName;
                    existing.Email = customer.Email;
                    existing.Phone = customer.Phone;
                    existing.BirthDate = customer.BirthDate;
                    existing.IsActive = customer.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;

                    // Conserva el hash anterior si no se escribe una contraseña.
                    if (!string.IsNullOrEmpty(password))
                        existing.PasswordHash = Crypto.HashPassword(password);

                    _manager.Update(existing);
                }

                TempData["Success"] = "Cliente guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar el cliente. Revisá los datos.");

                return ShowIndex(mode, customer);
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
                TempData["Success"] = "Cliente eliminado correctamente.";
            }
            catch (Exception)
            {
                // Las relaciones protegen clientes con registros asociados.
                TempData["Error"] =
                    "No se pudo eliminar. Puede tener direcciones, pedidos, " +
                    "carritos o reseñas asociados. Podés desactivarlo desde Editar.";
            }

            return RedirectToAction("Index");
        }

        // Prepara la única vista del módulo.
        private ActionResult ShowIndex(string mode, Customer selected)
        {
            ViewBag.Mode = mode;
            ViewBag.SelectedCustomer = selected;

            return View("Index", _manager.GetAll());
        }

        // Reutiliza la validación de campos de texto.
        private void ValidateText(
            string field, string value, int maximum, bool required)
        {
            if (required && value == null)
                ModelState.AddModelError(field, "Este campo es obligatorio.");
            else if (value != null && value.Length > maximum)
                ModelState.AddModelError(
                    field, "Máximo " + maximum + " caracteres.");
        }

        // Limpia espacios exteriores y convierte textos vacíos en null.
        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}