using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using System;
using System.Web.Mvc;

// Define exactamente cuál entidad Role utiliza este controlador.
using Role = PAP.DataAccess.Role;

namespace PAP.Mvc.Controllers
{
    public class RolesController : Controller
    {
        private readonly RoleManager _manager = new RoleManager();

        // Todas las operaciones se muestran dentro de Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Role();

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

        // Recibe únicamente el ID y el nombre del formulario.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save([Bind(Include = "Id,Name")] Role role)
        {
            string mode = role.Id == 0 ? "create" : "edit";

            // El nombre es obligatorio y admite hasta 50 caracteres.
            if (string.IsNullOrWhiteSpace(role.Name))
            {
                ModelState.AddModelError("Name", "El nombre es obligatorio.");
            }
            else
            {
                role.Name = role.Name.Trim();

                if (role.Name.Length > 50)
                {
                    ModelState.AddModelError(
                        "Name", "El nombre admite máximo 50 caracteres.");
                }
                else if (_manager.NameExists(role.Name, role.Id))
                {
                    ModelState.AddModelError(
                        "Name", "Ya existe un rol con ese nombre.");
                }
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, role);

            try
            {
                if (role.Id == 0)
                {
                    _manager.Add(role);
                }
                else
                {
                    // Actualiza únicamente el nombre del registro existente.
                    var existing = _manager.GetById(role.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.Name = role.Name;
                    _manager.Update(existing);
                }

                TempData["Success"] = "Rol guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // Conserva el formulario para corregir los datos.
                ModelState.AddModelError(
                    "", "No se pudo guardar el rol. Revisá el nombre.");

                return ShowIndex(mode, role);
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
                TempData["Success"] = "Rol eliminado correctamente.";
            }
            catch (Exception)
            {
                // La llave foránea protege los roles utilizados por empleados.
                TempData["Error"] =
                    "No se pudo eliminar. Verificá si está asociado a empleados.";
            }

            return RedirectToAction("Index");
        }

        // Prepara el listado y el rol seleccionado para la única vista.
        private ActionResult ShowIndex(string mode, Role selected)
        {
            ViewBag.Mode = mode;
            ViewBag.SelectedRole = selected;

            return View("Index", _manager.GetAll());
        }
    }
}