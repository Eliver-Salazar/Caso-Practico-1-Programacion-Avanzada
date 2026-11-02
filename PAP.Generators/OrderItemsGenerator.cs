using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Generators
{
    public class OrderItemsGenerator : IGenerator
    {
        // Requiere pedidos y libros.
        public GeneratorStatus GetStatus()
        {
            if (new OrderItemRepository().GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "OrderItems",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            var missing = new List<string>();

            if (!new OrderRepository().GetAll().Any())
                missing.Add("Orders");

            if (!new BookRepository().GetAll().Any())
                missing.Add("Books");

            return new GeneratorStatus
            {
                EntityName = "OrderItems",
                CanGenerate = missing.Count == 0,
                Message = missing.Count == 0
                    ? "Disponible para generar."
                    : "Primero agregá o generá: " +
                      string.Join(", ", missing) + "."
            };
        }

        // Genera una línea para cada uno de los primeros diez pedidos.
        public int Generate()
        {
            var repository = new OrderItemRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar artículos de pedidos: " +
                    "la tabla ya contiene datos.");
            }

            var orders = new OrderRepository()
                .GetAll()
                .OrderBy(order => order.Id)
                .Take(10)
                .ToList();

            var books = new BookRepository()
                .GetAll()
                .OrderBy(book => book.Id)
                .Take(10)
                .ToList();

            if (!orders.Any() || !books.Any())
            {
                throw new InvalidOperationException(
                    "Primero agregá o generá Orders y Books.");
            }

            // Evita generar importes inválidos desde pedidos preexistentes.
            if (orders.Any(order => order.Subtotal < 0))
            {
                throw new InvalidOperationException(
                    "Los pedidos seleccionados contienen subtotales negativos.");
            }

            for (int i = 0; i < orders.Count; i++)
            {
                var item = new OrderItem
                {
                    OrderId = orders[i].Id,
                    BookId = books[i % books.Count].Id,
                    Quantity = 1,

                    // Precio de prueba para hacer coincidir la línea
                    // con el subtotal del encabezado, sin modificar el pedido.
                    UnitPrice = orders[i].Subtotal,
                    Discount = 0m
                };

                repository.Add(item);
            }

            return orders.Count;
        }
    }
}
