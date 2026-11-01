using System;

using PAP.Business;
using PAP.DataAccess;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class GenresController : Controller
    {
        private readonly GenreManager _manager = new GenreManager();

        // Muestra el listado y la operación seleccionada en la misma vista.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Genre();

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

        // Recibe únicamente los campos permitidos para crear o editar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,Name,Description")] Genre genre)
        {
            string mode = genre.Id == 0 ? "create" : "edit";

            // Valida que el nombre exista, respete el tamaño y sea único.
            if (string.IsNullOrWhiteSpace(genre.Name))
            {
                ModelState.AddModelError("Name", "El nombre es obligatorio.");
            }
            else
            {
                genre.Name = genre.Name.Trim();

                if (genre.Name.Length > 100)
                {
                    ModelState.AddModelError(
                        "Name", "El nombre admite máximo 100 caracteres.");
                }
                else if (_manager.NameExists(genre.Name, genre.Id))
                {
                    ModelState.AddModelError(
                        "Name", "Ya existe un género con ese nombre.");
                }
            }

            // La descripción es opcional, con máximo 500 caracteres.
            if (genre.Description != null &&
                genre.Description.Length > 500)
            {
                ModelState.AddModelError(
                    "Description",
                    "La descripción admite máximo 500 caracteres.");
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, genre);

            try
            {
                if (genre.Id == 0)
                {
                    _manager.Add(genre);
                }
                else
                {
                    // Recupera el registro y actualiza sus campos editables.
                    var existing = _manager.GetById(genre.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.Name = genre.Name;
                    existing.Description = genre.Description;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Género guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // Conserva el formulario si falla el guardado.
                ModelState.AddModelError(
                    "", "No se pudo guardar. Revisá los datos y el nombre.");

                return ShowIndex(mode, genre);
            }
        }

        // Elimina después de confirmar mediante un formulario POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (_manager.GetById(id) == null)
                return HttpNotFound();

            try
            {
                _manager.Delete(id);
                TempData["Success"] = "Género eliminado correctamente.";
            }
            catch (Exception)
            {
                // La base de datos protege los géneros asociados a libros.
                TempData["Error"] =
                    "No se pudo eliminar. Verificá si está asociado a libros.";
            }

            return RedirectToAction("Index");
        }

        // Prepara los datos y reutiliza Index para todas las operaciones.
        private ActionResult ShowIndex(string mode, Genre selected)
        {
            ViewBag.Mode = mode;
            ViewBag.SelectedGenre = selected;

            return View("Index", _manager.GetAll());
        }
    }
}