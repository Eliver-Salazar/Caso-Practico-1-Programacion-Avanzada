using PAP.Business;
using PAP.DataAccess;
using PAP.Mvc.ModelBinders;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    // Ejemplo: Routing de atributos
    // Prefijo común para las acciones de este controlador: /biblioteca/...
    [RoutePrefix("biblioteca")]
    public class LibraryController : Controller
    {
        private readonly BookManager _bookManager;

        public LibraryController()
        {
            _bookManager = new BookManager();
        }

        // Ejemplo: Strongly Typed Views (vistas tipadas)
        // Se pasa IEnumerable<Book> a la vista; el modelo tipado da IntelliSense y seguridad de tipos.
        public ActionResult Index()
        {
            var books = _bookManager.GetAllBooksFromDataModel();
            return View(books);
        }

        // Ejemplo: Vistas múltiples
        // La misma acción puede devolver otra vista (.cshtml) según el parámetro "vista".
        // /Library/Catalog?vista=cards → Cards.cshtml
        // /Library/Catalog → Index.cshtml (tabla)
        public ActionResult Catalog(string vista)
        {
            var books = _bookManager.GetAllBooksFromDataModel();

            if (string.Equals(vista, "cards", StringComparison.OrdinalIgnoreCase))
            {
                return View("Cards", books);
            }

            return View("Index", books);
        }

        // Ejemplo: Routing de atributos
        // URL amigable: /biblioteca/detalle/3
        [Route("detalle/{id:int}")]
        public ActionResult Details(int id)
        {
            var book = _bookManager.GetAllBooksFromDataModel().FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return HttpNotFound();
            }

            return View(book);
        }

        // Ejemplo: Model Binding
        // GET muestra el formulario vacío (sin binding aún).
        [HttpGet]
        [Route("crear")]
        public ActionResult Create()
        {
            return View(new Book
            {
                PublicationDate = DateTime.Today,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsInStock = true
            });
        }

        // Ejemplo: Model Binding
        // ASP.NET MVC mapea automáticamente los campos del formulario a las propiedades de Book.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("crear")]
        public ActionResult Create(Book book)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }

            book.CreatedAt = DateTime.Now;
            book.UpdatedAt = DateTime.Now;
            if (string.IsNullOrWhiteSpace(book.Name))
            {
                book.Name = book.Title;
            }

            TempData["Mensaje"] = string.Format(
                "Libro enlazado por Model Binding: \"{0}\" de {1} — Precio: {2:C}",
                book.Title,
                book.Author,
                book.Price);

            return RedirectToAction("Index");
        }

        // Ejemplo: Model Binder personalizado
        // El parámetro "precio" usa CurrencyModelBinder en lugar del binder por defecto.
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
