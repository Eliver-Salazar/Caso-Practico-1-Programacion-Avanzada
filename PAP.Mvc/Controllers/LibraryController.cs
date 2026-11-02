using PAP.Business;
using PAP.DataAccess;
using PAP.Mvc.ModelBinders;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    [RoutePrefix("biblioteca")]
    public class LibraryController : Controller
    {
        private readonly BookManager _bookManager = new BookManager();
        private readonly CategoryManager _categories = new CategoryManager();
        private readonly GenreManager _genres = new GenreManager();
        private readonly PublisherManager _publishers = new PublisherManager();

        // Muestra el listado y las operaciones dentro de Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Book { IsInStock = true };

            if (mode == "edit" || mode == "details" || mode == "delete")
            {
                if (!id.HasValue)
                    return HttpNotFound();

                selected = _bookManager.GetById(id.Value);

                if (selected == null)
                    return HttpNotFound();
            }
            else if (mode != "create")
            {
                mode = "";
            }

            return ShowIndex(mode, selected);
        }

        // Conserva la vista de tarjetas del profe.
        public ActionResult Catalog(string vista)
        {
            if (string.Equals(vista, "cards",
                StringComparison.OrdinalIgnoreCase))
            {
                return View("Cards", _bookManager.GetAllBooksFromDataModel());
            }

            return RedirectToAction("Index");
        }

        // Conserva la dirección anterior del detalle y utiliza Index.
        [Route("detalle/{id:int}")]
        public ActionResult Details(int id)
        {
            return RedirectToAction("Index", new { mode = "details", id });
        }

        // Conserva el enlace anterior de crear, usando el formulario de Index.
        [HttpGet]
        [Route("crear")]
        public ActionResult Create()
        {
            return RedirectToAction("Index", new { mode = "create" });
        }

        // Recibe campos escalares; no recibe fechas ni colecciones relacionadas.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include =
                "Id,Name,Title,Category,Publisher,Author,Description," +
                "PublicationDate,Height,Width,Price,IsBestSeller,Genre," +
                "IsInStock,PublisherId,CategoryId,GenreId,ISBN,Pages," +
                "Language,CoverImageUrl")]
            Book book)
        {
            string mode = book.Id == 0 ? "create" : "edit";

            // Limpia los textos recibidos.
            book.Name = Clean(book.Name);
            book.Title = Clean(book.Title);
            book.Category = Clean(book.Category);
            book.Publisher = Clean(book.Publisher);
            book.Author = Clean(book.Author);
            book.Description = Clean(book.Description);
            book.Genre = Clean(book.Genre);
            book.ISBN = Clean(book.ISBN);
            book.Language = Clean(book.Language);
            book.CoverImageUrl = Clean(book.CoverImageUrl);

            if (book.Title == null)
                ModelState.AddModelError("Title", "El título es obligatorio.");

            // Conserva el comportamiento original del profesor.
            if (book.Name == null)
                book.Name = book.Title;

            // Valida los campos nuevos según el script de la base.
            ValidateLength("ISBN", book.ISBN, 20);
            ValidateLength("Language", book.Language, 50);
            ValidateLength("CoverImageUrl", book.CoverImageUrl, 500);

            if (book.ISBN != null && _bookManager.ISBNExists(book.ISBN, book.Id))
                ModelState.AddModelError("ISBN", "Ya existe un libro con ese ISBN.");

            if (book.Price < 0)
                ModelState.AddModelError("Price", "El precio no puede ser negativo.");
            else if (decimal.Round(book.Price, 2) != book.Price)
                ModelState.AddModelError("Price", "Usá máximo dos decimales.");

            if (book.Pages.HasValue && book.Pages.Value <= 0)
                ModelState.AddModelError("Pages", "Las páginas deben ser mayores que cero.");

            if (book.Height.HasValue && book.Height.Value <= 0)
                ModelState.AddModelError("Height", "La altura debe ser mayor que cero.");

            if (book.Width.HasValue && book.Width.Value <= 0)
                ModelState.AddModelError("Width", "El ancho debe ser mayor que cero.");

            // Las relaciones son opcionales; si se seleccionan, deben existir.
            if (book.CategoryId.HasValue)
            {
                var category = _categories.GetById(book.CategoryId.Value);

                if (category == null)
                    ModelState.AddModelError("CategoryId", "Categoría inválida.");
                else
                    book.Category = category.Name;
            }

            if (book.GenreId.HasValue)
            {
                var genre = _genres.GetById(book.GenreId.Value);

                if (genre == null)
                    ModelState.AddModelError("GenreId", "Género inválido.");
                else
                    book.Genre = genre.Name;
            }

            if (book.PublisherId.HasValue)
            {
                var publisher = _publishers.GetById(book.PublisherId.Value);

                if (publisher == null)
                    ModelState.AddModelError("PublisherId", "Editorial inválida.");
                else
                    book.Publisher = publisher.Name;
            }

            // Valida la URL sin descargar ni cargar imágenes externas.
            if (book.CoverImageUrl != null)
            {
                Uri url;

                if (!Uri.TryCreate(book.CoverImageUrl, UriKind.Absolute, out url) ||
                    (url.Scheme != Uri.UriSchemeHttp &&
                     url.Scheme != Uri.UriSchemeHttps))
                {
                    ModelState.AddModelError(
                        "CoverImageUrl", "Ingresá una URL HTTP o HTTPS válida.");
                }
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, book);

            try
            {
                // Usa una entidad nueva o recupera la existente.
                var target = book.Id == 0
                    ? new Book()
                    : _bookManager.GetById(book.Id);

                if (target == null)
                    return HttpNotFound();

                CopyFields(book, target);
                target.UpdatedAt = DateTime.UtcNow;

                if (book.Id == 0)
                {
                    target.CreatedAt = target.UpdatedAt;
                    _bookManager.Add(target);
                }
                else
                {
                    _bookManager.Update(target);
                }

                TempData["Success"] = "Libro guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar. Revisá los datos, tamaños e ISBN.");

                return ShowIndex(mode, book);
            }
        }

        // Elimina después de confirmar mediante POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (_bookManager.GetById(id) == null)
                return HttpNotFound();

            try
            {
                _bookManager.Delete(id);
                TempData["Success"] = "Libro eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "No se pudo eliminar. Verificá sus registros relacionados.";
            }

            return RedirectToAction("Index");
        }

        // Prepara los selectores y los nombres que muestra la vista.
        private ActionResult ShowIndex(string mode, Book selected)
        {
            var categories = _categories.GetAll().ToList();
            var genres = _genres.GetAll().ToList();
            var publishers = _publishers.GetAll().ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedBook = selected;

            ViewBag.CategoryOptions = new SelectList(
                categories, "Id", "Name", selected.CategoryId);

            ViewBag.GenreOptions = new SelectList(
                genres, "Id", "Name", selected.GenreId);

            ViewBag.PublisherOptions = new SelectList(
                publishers, "Id", "Name", selected.PublisherId);

            ViewBag.CategoryNames = categories.ToDictionary(x => x.Id, x => x.Name);
            ViewBag.GenreNames = genres.ToDictionary(x => x.Id, x => x.Name);
            ViewBag.PublisherNames = publishers.ToDictionary(x => x.Id, x => x.Name);

            return View("Index", _bookManager.GetAllBooksFromDataModel());
        }

        // Copia únicamente los campos editables, conservando fechas y relaciones.
        private static void CopyFields(Book source, Book target)
        {
            target.Name = source.Name;
            target.Title = source.Title;
            target.Category = source.Category;
            target.Publisher = source.Publisher;
            target.Author = source.Author;
            target.Description = source.Description;
            target.PublicationDate = source.PublicationDate;
            target.Height = source.Height;
            target.Width = source.Width;
            target.Price = source.Price;
            target.IsBestSeller = source.IsBestSeller;
            target.Genre = source.Genre;
            target.IsInStock = source.IsInStock;
            target.PublisherId = source.PublisherId;
            target.CategoryId = source.CategoryId;
            target.GenreId = source.GenreId;
            target.ISBN = source.ISBN;
            target.Pages = source.Pages;
            target.Language = source.Language;
            target.CoverImageUrl = source.CoverImageUrl;
        }

        // Valida las longitudes conocidas de los campos nuevos.
        private void ValidateLength(string field, string value, int maximum)
        {
            if (value != null && value.Length > maximum)
                ModelState.AddModelError(
                    field, "Máximo " + maximum + " caracteres.");
        }

        // Normaliza campos opcionales vacíos.
        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        // Conserva la demostración del binder personalizado del profe.
        [HttpGet]
        [Route("precio")]
        public ActionResult PrecioDemo()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("precio")]
        public ActionResult PrecioDemo(
            [ModelBinder(typeof(CurrencyModelBinder))] decimal precio)
        {
            ViewBag.PrecioConvertido = precio;
            return View();
        }
    }
}