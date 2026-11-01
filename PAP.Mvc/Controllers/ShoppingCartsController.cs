using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using PAP.DataAccess;

namespace PAP.Mvc.Controllers
{
    public class ShoppingCartsController : Controller
    {
        private readonly ShoppingCartManager _manager =
            new ShoppingCartManager();

        private readonly CustomerManager _customers = new CustomerManager();

        // Muestra el listado y la operación seleccionada en Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new ShoppingCart();

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

        // Solo recibe el ID y el cliente; las fechas se controlan en servidor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include = "Id,CustomerId")] ShoppingCart cart)
        {
            string mode = cart.Id == 0 ? "create" : "edit";

            // Verifica la existencia del cliente antes de guardar.
            if (cart.CustomerId <= 0 ||
                _customers.GetById(cart.CustomerId) == null)
            {
                ModelState.AddModelError(
                    "CustomerId", "Seleccioná un cliente válido.");
            }

            if (!ModelState.IsValid)
                return ShowIndex(mode, cart);

            try
            {
                if (cart.Id == 0)
                {
                    // Inicializa ambas fechas al crear.
                    cart.CreatedAt = DateTime.UtcNow;
                    cart.UpdatedAt = cart.CreatedAt;

                    _manager.Add(cart);
                }
                else
                {
                    // Conserva la fecha de creación del carrito.
                    var existing = _manager.GetById(cart.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.CustomerId = cart.CustomerId;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Carrito guardado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar el carrito. Revisá los datos.");

                return ShowIndex(mode, cart);
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
                TempData["Success"] = "Carrito eliminado correctamente.";
            }
            catch (Exception)
            {
                // Los artículos asociados deben eliminarse primero.
                TempData["Error"] =
                    "No se pudo eliminar. Verificá si contiene artículos.";
            }

            return RedirectToAction("Index");
        }

        // Prepara el selector y los nombres de clientes para la única vista.
        private ActionResult ShowIndex(string mode, ShoppingCart selected)
        {
            var customers = _customers.GetAll().ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedCart = selected;

            ViewBag.CustomerOptions = new SelectList(
                customers.Select(customer => new
                {
                    customer.Id,
                    Label = customer.FirstName + " " +
                            customer.LastName + " — " + customer.Email
                }),
                "Id",
                "Label",
                selected.CustomerId);

            ViewBag.CustomerNames = customers.ToDictionary(
                customer => customer.Id,
                customer => customer.FirstName + " " + customer.LastName);

            return View("Index", _manager.GetAll());
        }
    }
}