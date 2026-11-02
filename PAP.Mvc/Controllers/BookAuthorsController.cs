using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using PAP.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class BookAuthorsController : Controller
    {
        private readonly BookAuthorManager _manager = new BookAuthorManager();
        private readonly BookManager _books = new BookManager();
        private readonly AuthorManager _authors = new AuthorManager();

        // Identifica cada relación mediante BookId y AuthorId.
        public ActionResult Index(
            string mode = "", int? bookId = null, int? authorId = null)
        {
            var selected = new BookAuthorLink();

            if (mode == "edit" || mode == "details" || mode == "delete")
            {
                if (!bookId.HasValue || !authorId.HasValue)
                    return HttpNotFound();

                selected = _manager.GetById(bookId.Value, authorId.Value);

                if (selected == null)
                    return HttpNotFound();
            }
            else if (mode != "create")
            {
                mode = "";
            }

            return ShowIndex(mode, selected);
        }

        // Recibe los nuevos identificadores y, al editar, los originales.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "BookId,AuthorId")] BookAuthorLink link,
            int? originalBookId, int? originalAuthorId)
        {
            bool editing = originalBookId.HasValue || originalAuthorId.HasValue;
            string mode = editing ? "edit" : "create";

            if (editing &&
                (!originalBookId.HasValue || !originalAuthorId.HasValue))
                return new HttpStatusCodeResult(400);

            if (editing &&
                _manager.GetById(
                    originalBookId.Value, originalAuthorId.Value) == null)
                return HttpNotFound();

            // Valida ambas llaves foráneas.
            if (_books.GetById(link.BookId) == null)
                ModelState.AddModelError("BookId", "Seleccioná un libro válido.");

            if (_authors.GetById(link.AuthorId) == null)
                ModelState.AddModelError("AuthorId", "Seleccioná un autor válido.");

            bool sameLink = editing &&
                link.BookId == originalBookId.Value &&
                link.AuthorId == originalAuthorId.Value;

            if (!sameLink &&
                _manager.GetById(link.BookId, link.AuthorId) != null)
            {
                ModelState.AddModelError(
                    "", "Ese autor ya está asociado al libro.");
            }

            if (!ModelState.IsValid)
                return ShowIndex(
                    mode, link, originalBookId, originalAuthorId);

            try
            {
                _manager.Save(
                    link.BookId, link.AuthorId,
                    originalBookId, originalAuthorId);

                TempData["Success"] = "Relación guardada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar. Revisá los datos y los duplicados.");

                return ShowIndex(
                    mode, link, originalBookId, originalAuthorId);
            }
        }

        // Elimina la asociación después de confirmar mediante POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int bookId, int authorId)
        {
            if (_manager.GetById(bookId, authorId) == null)
                return HttpNotFound();

            try
            {
                _manager.Delete(bookId, authorId);
                TempData["Success"] = "Relación eliminada correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudo eliminar la relación.";
            }

            return RedirectToAction("Index");
        }

        // Prepara los selectores y conserva la clave original al editar.
        private ActionResult ShowIndex(
            string mode, BookAuthorLink selected,
            int? originalBookId = null, int? originalAuthorId = null)
        {
            var books = _books.GetAllBooksFromDataModel().Select(book => new
            {
                book.Id,
                Label = "#" + book.Id + " — " +
                    (string.IsNullOrWhiteSpace(book.Title) ? book.Name : book.Title)
            }).ToList();

            var authors = _authors.GetAll().Select(author => new
            {
                author.Id,
                Label = "#" + author.Id + " — " +
                    author.FirstName + " " + author.LastName
            }).ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedLink = selected;
            ViewBag.OriginalBookId = originalBookId ?? selected.BookId;
            ViewBag.OriginalAuthorId = originalAuthorId ?? selected.AuthorId;

            ViewBag.BookOptions = new SelectList(
                books, "Id", "Label", selected.BookId);

            ViewBag.AuthorOptions = new SelectList(
                authors, "Id", "Label", selected.AuthorId);

            ViewBag.CanCreate = books.Any() && authors.Any();

            return View("Index", _manager.GetAll());
        }
    }
}