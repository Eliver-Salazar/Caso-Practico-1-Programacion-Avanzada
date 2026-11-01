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
    public class PublishersController : Controller
    {
        private readonly PublisherManager _manager = new PublisherManager();

        // Muestra el listado y la operación seleccionada en Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Publisher();

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

        // Solo recibe los campos editables; CreatedAt se controla en servidor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,Name,Country,Website,Email")]
            Publisher publisher)
        {
            string mode = publisher.Id == 0 ? "create" : "edit";

            // Limpia espacios y convierte los campos opcionales vacíos en null.
            publisher.Name = Clean(publisher.Name);
            publisher.Country = Clean(publisher.Country);
            publisher.Website = Clean(publisher.Website);
            publisher.Email = Clean(publisher.Email);

            // Valida los tamaños definidos en la base de datos.
            if (publisher.Name == null)
                ModelState.AddModelError("Name", "El nombre es obligatorio.");
            else if (publisher.Name.Length > 200)
                ModelState.AddModelError("Name", "Máximo 200 caracteres.");

            if (publisher.Country != null && publisher.Country.Length > 100)
                ModelState.AddModelError("Country", "Máximo 100 caracteres.");

            if (publisher.Website != null)
            {
                Uri website;

                // Acepta únicamente direcciones web HTTP o HTTPS completas.
                if (publisher.Website.Length > 255)
                {
                    ModelState.AddModelError("Website", "Máximo 255 caracteres.");
                }
                else if (!Uri.TryCreate(
                             publisher.Website, UriKind.Absolute, out website) ||
                         (website.Scheme != Uri.UriSchemeHttp &&
                          website.Scheme != Uri.UriSchemeHttps))
                {
                    ModelState.AddModelError(
                        "Website", "Ingresá una URL como https://editorial.com.");
                }
            }

            if (publisher.Email != null)
            {
                if (publisher.Email.Length > 150)
                {
                    ModelState.AddModelError("Email", "Máximo 150 caracteres.");
                }
                else
                {
                    try
                    {
                        // Valida que sea una dirección de correo sin nombre adicional.
                        var email = new MailAddress(publisher.Email);

                        if (email.Address != publisher.Email)
                            ModelState.AddModelError("Email", "Correo inválido.");
                    }
                    catch (FormatException)
                    {
                        ModelState.AddModelError("Email", "Correo inválido.");
                    }
                }
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, publisher);

            try
            {
                if (publisher.Id == 0)
                {
                    publisher.CreatedAt = DateTime.UtcNow;
                    _manager.Add(publisher);
                }
                else
                {
                    // Conserva la fecha original de creación al editar.
                    var existing = _manager.GetById(publisher.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.Name = publisher.Name;
                    existing.Country = publisher.Country;
                    existing.Website = publisher.Website;
                    existing.Email = publisher.Email;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Editorial guardada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar la editorial. Revisá los datos.");

                return ShowIndex(mode, publisher);
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
                TempData["Success"] = "Editorial eliminada correctamente.";
            }
            catch (Exception)
            {
                // Las relaciones de la base impiden eliminar editoriales en uso.
                TempData["Error"] =
                    "No se pudo eliminar. Verificá si está asociada a libros.";
            }

            return RedirectToAction("Index");
        }

        // Prepara los datos para la única vista del módulo.
        private ActionResult ShowIndex(string mode, Publisher selected)
        {
            ViewBag.Mode = mode;
            ViewBag.SelectedPublisher = selected;

            return View("Index", _manager.GetAll());
        }

        // Normaliza los textos recibidos del formulario.
        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}