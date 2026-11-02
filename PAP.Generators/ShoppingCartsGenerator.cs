using System;
using System.Collections.Generic;
using System.Linq;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class ShoppingCartsGenerator : IGenerator
    {
        // Comprueba si la tabla está vacía y existen clientes.
        public GeneratorStatus GetStatus()
        {
            var repository = new ShoppingCartRepository();

            if (repository.GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "ShoppingCarts",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            bool hasCustomers = new CustomerRepository().GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "ShoppingCarts",
                CanGenerate = hasCustomers,
                Message = hasCustomers
                    ? "Disponible para generar."
                    : "Primero agregá o generá Customers."
            };
        }

        // Genera un carrito para cada uno de los primeros diez clientes.
        public int Generate()
        {
            var repository = new ShoppingCartRepository();

            // Valida nuevamente antes de insertar.
            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar carritos: la tabla ya contiene datos.");
            }

            var customers = new CustomerRepository()
                .GetAll()
                .OrderBy(customer => customer.Id)
                .Take(10)
                .ToList();

            if (!customers.Any())
            {
                throw new InvalidOperationException(
                    "Primero agregá o generá Customers.");
            }

            foreach (var customer in customers)
            {
                DateTime now = DateTime.UtcNow;

                var cart = new ShoppingCart
                {
                    CustomerId = customer.Id,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                repository.Add(cart);
            }

            return customers.Count;
        }
    }
}
