using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PAP.Business;
using PAP.DataAccess;


namespace PAP.Mvc.Controllers
{
    public class AddressesController : Controller
    {
        private readonly AddressManager _manager = new AddressManager();
        private readonly CustomerManager _customers = new CustomerManager();

        // Muestra el listado y la operación seleccionada dentro de Index.
        public ActionResult Index(string mode = "", int? id = null)
        {
            var selected = new Address
            {
                AddressType = "Shipping"
            };

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

        // Recibe únicamente los campos editables de la dirección.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(
            [Bind(Include =
                "Id,CustomerId,AddressType,Street,City,State,PostalCode,Country,IsDefault")]
            Address address)
        {
            string mode = address.Id == 0 ? "create" : "edit";

            // Normaliza los campos de texto.
            address.AddressType = Clean(address.AddressType);
            address.Street = Clean(address.Street);
            address.City = Clean(address.City);
            address.State = Clean(address.State);
            address.PostalCode = Clean(address.PostalCode);
            address.Country = Clean(address.Country);

            // Verifica la llave foránea antes de guardar.
            if (address.CustomerId <= 0 ||
                _customers.GetById(address.CustomerId) == null)
            {
                ModelState.AddModelError(
                    "CustomerId", "Seleccioná un cliente válido.");
            }

            // Tipos de dirección que ofrece este módulo.
            if (address.AddressType != "Shipping" &&
                address.AddressType != "Billing")
            {
                ModelState.AddModelError(
                    "AddressType", "Seleccioná un tipo de dirección válido.");
            }

            // Valida campos obligatorios y tamaños de la base de datos.
            ValidateText("Street", address.Street, 200, true);
            ValidateText("City", address.City, 100, true);
            ValidateText("State", address.State, 100, false);
            ValidateText("PostalCode", address.PostalCode, 20, true);
            ValidateText("Country", address.Country, 100, true);

            if (!ModelState.IsValid)
                return ShowIndex(mode, address);

            try
            {
                if (address.Id == 0)
                {
                    _manager.Add(address);
                }
                else
                {
                    // Modifica únicamente los campos permitidos.
                    var existing = _manager.GetById(address.Id);

                    if (existing == null)
                        return HttpNotFound();

                    existing.CustomerId = address.CustomerId;
                    existing.AddressType = address.AddressType;
                    existing.Street = address.Street;
                    existing.City = address.City;
                    existing.State = address.State;
                    existing.PostalCode = address.PostalCode;
                    existing.Country = address.Country;
                    existing.IsDefault = address.IsDefault;

                    _manager.Update(existing);
                }

                TempData["Success"] = "Dirección guardada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "", "No se pudo guardar la dirección. Revisá los datos.");

                return ShowIndex(mode, address);
            }
        }

        // Elimina después de confirmar mediante POST.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (_manager.GetById(id) == null)
                return HttpNotFound();

            try
            {
                _manager.Delete(id);
                TempData["Success"] = "Dirección eliminada correctamente.";
            }
            catch (Exception)
            {
                // Las relaciones protegen direcciones utilizadas en pedidos.
                TempData["Error"] =
                    "No se pudo eliminar. Verificá si está asociada a pedidos.";
            }

            return RedirectToAction("Index");
        }

        // Prepara el listado y las opciones del selector de clientes.
        private ActionResult ShowIndex(string mode, Address selected)
        {
            var customers = _customers.GetAll().ToList();

            ViewBag.Mode = mode;
            ViewBag.SelectedAddress = selected;

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

            // Permite mostrar nombres sin depender de la carga de navegación.
            ViewBag.CustomerNames = customers.ToDictionary(
                customer => customer.Id,
                customer => customer.FirstName + " " + customer.LastName);

            return View("Index", _manager.GetAll());
        }

        // Reutiliza las validaciones de texto.
        private void ValidateText(
            string field, string value, int maximum, bool required)
        {
            if (required && value == null)
                ModelState.AddModelError(field, "Este campo es obligatorio.");
            else if (value != null && value.Length > maximum)
                ModelState.AddModelError(
                    field, "Máximo " + maximum + " caracteres.");
        }

        // Limpia espacios y convierte textos vacíos en null.
        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}