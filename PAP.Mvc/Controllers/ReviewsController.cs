using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using PAP.DataAccess;

namespace PAP.Mvc.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ReviewManager _manager = new ReviewManager();
        private readonly CustomerManager _customers = new CustomerManager();
        private readonly BookManager _books = new BookManager();

        // Muestra la tabla y la operación seleccionada dentro de Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Review { Rating = 5 };

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

        // La fecha de creación se controla desde el servidor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,BookId,CustomerId,Rating,Comment")]
            Review review)
        {
            string mode = review.Id == 0 ? "create" : "edit";

            // Valida las llaves foráneas.
            bool validCustomer =
                _customers.GetById(review.CustomerId) != null;

            bool validBook = _books.GetAllBooksFromDataModel()
                .Any(book => book.Id == review.BookId);

            if (!validCustomer)
            {
                ModelState.AddModelError(
                    "CustomerId", "Seleccioná un cliente válido.");
            }

            if (!validBook)
            {
                ModelState.AddModelError(
                    "BookId", "Seleccioná un libro válido.");
            }

            // Respeta el rango permitido por la base de datos.
            if (review.Rating < 1 || review.Rating > 5)
            {
                ModelState.AddModelError(
                    "Rating", "La calificación debe estar entre 1 y 5.");
            }

            // El comentario es opcional y admite hasta 1000 caracteres.
            review.Comment = string.IsNullOrWhiteSpace(review.Comment)
                ? null
                : review.Comment.Trim();

            if (review.Comment != null && review.Comment.Length > 1000)
            {
                ModelState.AddModelError(
                    "Comment", "El comentario admite máximo 1000 caracteres.");
            }

            // Impide reseñas duplicadas del mismo cliente sobre el mismo libro.
            if (validCustomer && validBook &&
                _manager.ReviewExists(
                    review.BookId, review.CustomerId, review.Id))
            {
                ModelState.AddModelError(
                    "", "Este cliente ya tiene una reseña de ese libro.");
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, review);

            try
            {
                if (review.Id == 0)
                {
                    review.CreatedAt = DateTime.UtcNow;
                    _manager.Add(review);
                }
                else
                {
                    // Conserva la fecha original al editar.
                    var existing = _manager.GetById(review.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.BookId = review.BookId;
                    existing.CustomerId = review.CustomerId;
                    existing.Rating = review.Rating;
                    existing.Comment = review.Comment;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Reseña guardada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar. Revisá los datos y los duplicados.");

                return ShowIndex(mode, review);
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
                TempData["Success"] = "Reseña eliminada correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudo eliminar la reseña.";
            }

            return RedirectToAction("Index");
        }

        // Prepara los selectores y sus etiquetas para la única vista.
        private ActionResult ShowIndex(string mode, Review selected)
        {
            var customers = _customers.GetAll().Select(customer => new
            {
                customer.Id,
                Label = "#" + customer.Id + " — " +
                    customer.FirstName + " " + customer.LastName +
                    " — " + customer.Email
            }).ToList();

            var books = _books.GetAllBooksFromDataModel().Select(book => new
            {
                book.Id,
                Label = "#" + book.Id + " — " +
                    (string.IsNullOrWhiteSpace(book.Title)
                        ? book.Name
                        : book.Title)
            }).ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedReview = selected;

            ViewBag.CustomerOptions = new SelectList(
                customers, "Id", "Label", selected.CustomerId);

            ViewBag.BookOptions = new SelectList(
                books, "Id", "Label", selected.BookId);

            ViewBag.CustomerNames = customers.ToDictionary(
                customer => customer.Id, customer => customer.Label);

            ViewBag.BookNames = books.ToDictionary(
                book => book.Id, book => book.Label);

            return View("Index", _manager.GetAll());
        }
    }
}