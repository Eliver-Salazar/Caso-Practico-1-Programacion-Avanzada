using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class OrdersGenerator : IGenerator
    {
        // Los pedidos requieren clientes; la dirección es opcional.
        public GeneratorStatus GetStatus()
        {
            if (new OrderRepository().GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "Orders",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            bool hasCustomers = new CustomerRepository().GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Orders",
                CanGenerate = hasCustomers,
                Message = hasCustomers
                    ? "Disponible para generar."
                    : "Primero agregá o generá Customers."
            };
        }

        // Genera un pedido por cada uno de los primeros diez clientes.
        public int Generate()
        {
            var repository = new OrderRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar pedidos: la tabla ya contiene datos.");
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

            var addresses = new AddressRepository()
                .GetAll()
                .OrderBy(address => address.Id)
                .ToList();

            for (int i = 0; i < customers.Count; i++)
            {
                var customer = customers[i];

                // Solo utiliza direcciones pertenecientes al cliente del pedido.
                var address = addresses.FirstOrDefault(
                    item => item.CustomerId == customer.Id);

                decimal subtotal = 1000m + i * 500m;
                decimal tax = decimal.Round(subtotal * 0.13m, 2);
                decimal shipping = 500m;
                DateTime now = DateTime.UtcNow;

                var order = new Order
                {
                    CustomerId = customer.Id,
                    ShippingAddressId = address == null
                        ? (int?)null
                        : address.Id,
                    OrderDate = now,
                    Status = "Pending",
                    Subtotal = subtotal,
                    TaxAmount = tax,
                    ShippingCost = shipping,
                    Total = subtotal + tax + shipping,
                    Notes = "Pedido generado para el caso práctico.",
                    UpdatedAt = now
                };

                repository.Add(order);
            }

            return customers.Count;
        }
    }
}