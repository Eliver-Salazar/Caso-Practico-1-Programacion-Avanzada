using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class AddressesGenerator : IGenerator
    {
        // Bloquea si existen direcciones o si faltan clientes.
        public GeneratorStatus GetStatus()
        {
            var repository = new AddressRepository();

            if (repository.GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "Addresses",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            var customers = new CustomerRepository();
            bool hasCustomers = customers.GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Addresses",
                CanGenerate = hasCustomers,
                Message = hasCustomers
                    ? "Disponible para generar."
                    : "Primero agregá o generá Customers."
            };
        }

        // Genera una dirección para cada uno de los primeros diez clientes.
        public int Generate()
        {
            var repository = new AddressRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar direcciones: la tabla ya contiene datos.");
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

            for (int i = 0; i < customers.Count; i++)
            {
                var address = new Address
                {
                    // Usa el ID real, sin asumir numeración consecutiva.
                    CustomerId = customers[i].Id,
                    AddressType = "Shipping",
                    Street = "Dirección de prueba número " + (i + 1),
                    City = "San José",
                    State = "San José",
                    PostalCode = "10101",
                    Country = "Costa Rica",
                    IsDefault = true
                };

                repository.Add(address);
            }

            return customers.Count;
        }
    }
}
