using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class PurchaseOrderItemsGenerator : IGenerator
    {
        // Comprueba que existan órdenes de compra y libros.
        public GeneratorStatus GetStatus()
        {
            if (new PurchaseOrderItemRepository().GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "PurchaseOrderItems",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            var missing = new List<string>();

            if (!new PurchaseOrderRepository().GetAll().Any())
                missing.Add("PurchaseOrders");

            if (!new BookRepository().GetAll().Any())
                missing.Add("Books");

            return new GeneratorStatus
            {
                EntityName = "PurchaseOrderItems",
                CanGenerate = missing.Count == 0,
                Message = missing.Count == 0
                    ? "Disponible para generar."
                    : "Primero agregá o generá: " +
                      string.Join(", ", missing) + "."
            };
        }

        // Genera un artículo por cada una de las primeras diez órdenes.
        public int Generate()
        {
            var repository = new PurchaseOrderItemRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar artículos de compra: " +
                    "la tabla ya contiene datos.");
            }

            var orders = new PurchaseOrderRepository()
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
                    "Primero agregá o generá PurchaseOrders y Books.");
            }

            for (int i = 0; i < orders.Count; i++)
            {
                var item = new PurchaseOrderItem
                {
                    PurchaseOrderId = orders[i].Id,
                    BookId = books[i % books.Count].Id,
                    Quantity = 5 + i,
                    UnitCost = 1000m + i * 100m
                };

                // Utiliza Add heredado de RepositoryBase.
                repository.Add(item);
            }

            return orders.Count;
        }
    }
}
