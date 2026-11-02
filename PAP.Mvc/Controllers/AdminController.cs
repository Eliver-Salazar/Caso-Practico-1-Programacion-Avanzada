using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Generators;
using System;
using System.Web.Mvc;

namespace PAP.Mvc.Controllers
{
    public class AdminController : Controller
    {
        private readonly GeneratorCatalog _catalog = new GeneratorCatalog();

        // Muestra las entidades y su disponibilidad de generación.
        public ActionResult Index()
        {
            return View(_catalog.GetStatuses());
        }

        // La generación modifica datos, por eso requiere POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generate(string entityName)
        {
            try
            {
                int count = _catalog.Generate(entityName);

                TempData["Success"] =
                    "Se generaron " + count + " registros en " + entityName + ".";
            }
            catch (InvalidOperationException exception)
            {
                // Muestra los mensajes de las validaciones del generador.
                TempData["Error"] = exception.Message;
            }
            catch (Exception)
            {
                // Registra el fallo para revisarlo durante el desarrollo.
                System.Diagnostics.Trace.TraceError(
                    "Falló la generación de la entidad: {0}", entityName);

                TempData["Error"] =
                    "No se pudo completar la generación. Revisá la conexión " +
                    "y los datos. Puede haber registros guardados antes del fallo.";
            }

            // Al recargar, vuelve a consultar el estado de todos los botones.
            return RedirectToAction("Index");
        }
    }
}