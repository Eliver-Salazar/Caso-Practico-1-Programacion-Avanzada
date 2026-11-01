using System.Web.Mvc;
using System.Web.Routing;

namespace PAP.Mvc
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Ejemplo: Routing de atributos
            // Habilita rutas definidas con [Route] / [RoutePrefix] en los controladores.
            routes.MapMvcAttributeRoutes();

            // Ejemplo: URL Routing
            // Ruta convencional personalizada: /catalogo/libros → LibraryController.Index
            routes.MapRoute(
                name: "CatalogoLibros",
                url: "catalogo/libros",
                defaults: new { controller = "Library", action = "Index" }
            );

            // Ejemplo: Restricciones de ruta
            // Solo coincide si "id" es un entero positivo (p. ej. /libro/5).
            // Una URL como /libro/abc no usa esta ruta.
            routes.MapRoute(
                name: "LibroPorId",
                url: "libro/{id}",
                defaults: new { controller = "Library", action = "Details" },
                constraints: new { id = @"\d+" }
            );

            // Ejemplo: URL Routing (ruta por defecto)
            // Patrón {controller}/{action}/{id} → Home/Index si no hay valores.
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
