using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using PAP.Business;
using PAP.DataAccess;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class AuthorsController : Controller
    {
        // El manager comunica el controlador con el repository.
        private readonly AuthorManager _manager = new AuthorManager();

        // Muestra la tabla y, cuando corresponde, el formulario o detalle.
        public ActionResult Index(string mode = "", int? id = null)
        {
            Author selected = new Author();

            // Estas acciones requieren un autor existente.
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

        // Recibe el formulario para crear o editar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,FirstName,LastName,Biography,BirthDate,Country")]
            Author author)
        {
            string mode = author.Id == 0 ? "create" : "edit";

            // Validaciones de los campos obligatorios y sus tamaños.
            if (string.IsNullOrWhiteSpace(author.FirstName))
                ModelState.AddModelError("FirstName", "El nombre es obligatorio.");
            else if (author.FirstName.Length > 100)
                ModelState.AddModelError("FirstName", "Máximo 100 caracteres.");

            if (string.IsNullOrWhiteSpace(author.LastName))
                ModelState.AddModelError("LastName", "El apellido es obligatorio.");
            else if (author.LastName.Length > 100)
                ModelState.AddModelError("LastName", "Máximo 100 caracteres.");

            if (author.Country != null && author.Country.Length > 100)
                ModelState.AddModelError("Country", "Máximo 100 caracteres.");

            if (author.BirthDate.HasValue &&
                author.BirthDate.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    "BirthDate", "La fecha no puede ser futura.");
            }

            // Conserva el formulario y muestra los errores.
            if (!ModelState.IsValid)
                return ShowIndex(mode, author);

            try
            {
                if (author.Id == 0)
                {
                    // La fecha de creación se establece en el servidor.
                    author.CreatedAt = DateTime.UtcNow;
                    _manager.Add(author);
                }
                else
                {
                    // Recupera el registro para conservar CreatedAt
                    // y modificar únicamente los campos permitidos.
                    var existing = _manager.GetById(author.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.FirstName = author.FirstName;
                    existing.LastName = author.LastName;
                    existing.Biography = author.Biography;
                    existing.BirthDate = author.BirthDate;
                    existing.Country = author.Country;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Autor guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                // No muestra detalles internos de la base de datos.
                ModelState.AddModelError(
                    "", "No se pudo guardar el autor. Revisá los datos.");

                return ShowIndex(mode, author);
            }
        }

        // Elimina únicamente después de confirmar mediante POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var author = _manager.GetById(id);

            if (author == null)
                return HttpNotFound();

            try
            {
                _manager.Delete(id);
                TempData["Success"] = "Autor eliminado correctamente.";
            }
            catch (Exception)
            {
                // Las llaves foráneas impiden eliminar autores vinculados.
                TempData["Error"] =
                    "No se pudo eliminar. Verificá si está asociado a libros.";
            }

            return RedirectToAction("Index");
        }

        // Reutiliza la misma vista para todas las operaciones.
        private ActionResult ShowIndex(string mode, Author selected)
        {
            ViewBag.Mode = mode;
            ViewBag.SelectedAuthor = selected;

            return View("Index", _manager.GetAll());
        }
    }
}