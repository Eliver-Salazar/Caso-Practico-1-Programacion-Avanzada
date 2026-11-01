using System;
using System.Web.Mvc;

using PAP.Business;
using PAP.DataAccess;

namespace PAP.Mvc.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly CategoryManager _manager = new CategoryManager();

        // Muestra la tabla y la operación seleccionada dentro de Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Category();

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

        // Recibe únicamente los campos permitidos del formulario.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,Name,Description")] Category category)
        {
            string mode = category.Id == 0 ? "create" : "edit";

            // Valida el nombre obligatorio y su longitud.
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError("Name", "El nombre es obligatorio.");
            }
            else
            {
                category.Name = category.Name.Trim();

                if (category.Name.Length > 100)
                {
                    ModelState.AddModelError(
                        "Name", "El nombre admite máximo 100 caracteres.");
                }
                else if (_manager.NameExists(category.Name, category.Id))
                {
                    ModelState.AddModelError(
                        "Name", "Ya existe una categoría con ese nombre.");
                }
            }

            // La descripción es opcional y admite hasta 500 caracteres.
            if (category.Description != null &&
                category.Description.Length > 500)
            {
                ModelState.AddModelError(
                    "Description", "La descripción admite máximo 500 caracteres.");
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, category);

            try
            {
                if (category.Id == 0)
                {
                    _manager.Add(category);
                }
                else
                {
                    // Modifica el registro existente sin cambiar su ID.
                    var existing = _manager.GetById(category.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.Name = category.Name;
                    existing.Description = category.Description;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Categoría guardada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // La base de datos también protege los nombres únicos.
                ModelState.AddModelError(
                    "", "No se pudo guardar. Revisá los datos y el nombre.");

                return ShowIndex(mode, category);
            }
        }

        // La eliminación requiere confirmación y una solicitud POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (_manager.GetById(id) == null)
                return HttpNotFound();

            try
            {
                _manager.Delete(id);
                TempData["Success"] = "Categoría eliminada correctamente.";
            }
            catch (Exception)
            {
                // Una categoría asociada a libros no puede eliminarse.
                TempData["Error"] =
                    "No se pudo eliminar. Verificá si está asociada a libros.";
            }

            return RedirectToAction("Index");
        }

        // Todas las operaciones utilizan la misma vista.
        private ActionResult ShowIndex(string mode, Category selected)
        {
            ViewBag.Mode = mode;
            ViewBag.SelectedCategory = selected;

            return View("Index", _manager.GetAll());
        }
    }
}