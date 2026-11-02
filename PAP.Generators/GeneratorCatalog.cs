using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Generators
{
    public class GeneratorCatalog
    {
        // Registra los generadores de las veinte tablas del caso.
        private readonly Dictionary<string, IGenerator> _generators =
            new Dictionary<string, IGenerator>(StringComparer.OrdinalIgnoreCase)
            {
                { "Authors", new AuthorsGenerator() },
                { "Categories", new CategoriesGenerator() },
                { "Genres", new GenresGenerator() },
                { "Roles", new RolesGenerator() },
                { "Publishers", new PublishersGenerator() },
                { "Suppliers", new SuppliersGenerator() },
                { "Customers", new CustomersGenerator() },
                { "Employees", new EmployeesGenerator() },
                { "Addresses", new AddressesGenerator() },
                { "ShoppingCarts", new ShoppingCartsGenerator() },
                { "PurchaseOrders", new PurchaseOrdersGenerator() },
                { "Books", new BooksGenerator() },
                { "Inventory", new InventoryGenerator() },
                { "BookAuthors", new BookAuthorsGenerator() },
                { "Reviews", new ReviewsGenerator() },
                { "CartItems", new CartItemsGenerator() },
                { "PurchaseOrderItems", new PurchaseOrderItemsGenerator() },
                { "Orders", new OrdersGenerator() },
                { "OrderItems", new OrderItemsGenerator() },
                { "Payments", new PaymentsGenerator() }
            };

        // Consulta los estados actuales de todas las entidades.
        public IEnumerable<GeneratorStatus> GetStatuses()
        {
            return _generators.Values
                .Select(generator => generator.GetStatus())
                .OrderBy(status => status.EntityName)
                .ToList();
        }

        // Valida las condiciones desde el servidor antes de generar.
        public int Generate(string entityName)
        {
            IGenerator generator;

            if (string.IsNullOrWhiteSpace(entityName) ||
                !_generators.TryGetValue(entityName, out generator))
            {
                throw new InvalidOperationException(
                    "La entidad seleccionada no tiene un generador registrado.");
            }

            var status = generator.GetStatus();

            if (!status.CanGenerate)
                throw new InvalidOperationException(status.Message);

            return generator.Generate();
        }
    }
}