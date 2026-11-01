using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using System;
using System.Linq;
using System.Net.Mail;
using System.Web.Helpers;
using System.Web.Mvc;

using Employee = PAP.DataAccess.Employee;

namespace PAP.Mvc.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly EmployeeManager _manager = new EmployeeManager();
        private readonly RoleManager _roles = new RoleManager();

        // Muestra el listado y la operación seleccionada dentro de Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Employee { IsActive = true };

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

        // No recibe PasswordHash ni CreatedAt desde el navegador.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,RoleId,FirstName,LastName,Email,IsActive")]
            Employee employee,
            string password)
        {
            string mode = employee.Id == 0 ? "create" : "edit";

            // Normaliza los campos de texto.
            employee.FirstName = Clean(employee.FirstName);
            employee.LastName = Clean(employee.LastName);
            employee.Email = Clean(employee.Email);

            ValidateText("FirstName", employee.FirstName, 100);
            ValidateText("LastName", employee.LastName, 100);
            ValidateText("Email", employee.Email, 150);

            // Verifica que el rol exista antes de guardar la llave foránea.
            if (employee.RoleId <= 0 ||
                _roles.GetById(employee.RoleId) == null)
            {
                ModelState.AddModelError(
                    "RoleId", "Seleccioná un rol válido.");
            }

            // Comprueba el formato y la unicidad del correo.
            if (employee.Email != null && employee.Email.Length <= 150)
            {
                try
                {
                    var email = new MailAddress(employee.Email);

                    if (email.Address != employee.Email)
                    {
                        ModelState.AddModelError("Email", "Correo inválido.");
                    }
                    else if (_manager.EmailExists(employee.Email, employee.Id))
                    {
                        ModelState.AddModelError(
                            "Email", "Ya existe un empleado con ese correo.");
                    }
                }
                catch (FormatException)
                {
                    ModelState.AddModelError("Email", "Correo inválido.");
                }
            }

            // La contraseña es obligatoria al crear y opcional al editar.
            if (employee.Id == 0 && string.IsNullOrWhiteSpace(password))
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

            if (!ModelState.IsValid)
                return ShowIndex(mode, employee);

            try
            {
                if (employee.Id == 0)
                {
                    // Guarda un hash con sal, nunca la contraseña original.
                    employee.PasswordHash = Crypto.HashPassword(password);
                    employee.CreatedAt = DateTime.UtcNow;

                    _manager.Add(employee);
                }
                else
                {
                    // Conserva CreatedAt y la contraseña si no se cambia.
                    var existing = _manager.GetById(employee.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.RoleId = employee.RoleId;
                    existing.FirstName = employee.FirstName;
                    existing.LastName = employee.LastName;
                    existing.Email = employee.Email;
                    existing.IsActive = employee.IsActive;

                    if (!string.IsNullOrEmpty(password))
                        existing.PasswordHash = Crypto.HashPassword(password);

                    _manager.Update(existing);
                }

                TempData["Success"] = "Empleado guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar el empleado. Revisá los datos.");

                return ShowIndex(mode, employee);
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
                TempData["Success"] = "Empleado eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "No se pudo eliminar el empleado. " +
                    "Podés desactivarlo desde Editar.";
            }

            return RedirectToAction("Index");
        }

        // Prepara el selector y los nombres de roles para la vista.
        private ActionResult ShowIndex(string mode, Employee selected)
        {
            var roles = _roles.GetAll().ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedEmployee = selected;

            ViewBag.RoleOptions = new SelectList(
                roles, "Id", "Name", selected.RoleId);

            ViewBag.RoleNames = roles.ToDictionary(
                role => role.Id,
                role => role.Name);

            return View("Index", _manager.GetAll());
        }

        // Valida los textos obligatorios y su tamaño.
        private void ValidateText(string field, string value, int maximum)
        {
            if (value == null)
                ModelState.AddModelError(field, "Este campo es obligatorio.");
            else if (value.Length > maximum)
                ModelState.AddModelError(
                    field, "Máximo " + maximum + " caracteres.");
        }

        // Elimina espacios exteriores y normaliza los textos vacíos.
        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}