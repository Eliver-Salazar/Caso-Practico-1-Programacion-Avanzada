using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class PurchaseOrdersGenerator : IGenerator
    {
        // Comprueba si la tabla está vacía y existen proveedores.
        public GeneratorStatus GetStatus()
        {
            var repository = new PurchaseOrderRepository();

            if (repository.GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "PurchaseOrders",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            bool hasSuppliers = new SupplierRepository().GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "PurchaseOrders",
                CanGenerate = hasSuppliers,
                Message = hasSuppliers
                    ? "Disponible para generar."
                    : "Primero agregá o generá Suppliers."
            };
        }

        // Genera una orden para cada uno de los primeros diez proveedores.
        public int Generate()
        {
            var repository = new PurchaseOrderRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar órdenes de compra: " +
                    "la tabla ya contiene datos.");
            }

            var suppliers = new SupplierRepository()
                .GetAll()
                .OrderBy(supplier => supplier.Id)
                .Take(10)
                .ToList();

            if (!suppliers.Any())
            {
                throw new InvalidOperationException(
                    "Primero agregá o generá Suppliers.");
            }

            foreach (var supplier in suppliers)
            {
                DateTime now = DateTime.UtcNow;

                var order = new PurchaseOrder
                {
                    SupplierId = supplier.Id,
                    OrderDate = now,
                    ExpectedDate = now.Date.AddDays(7),
                    Status = "Pending",

                    // La orden se genera inicialmente sin artículos.
                    TotalCost = 0m
                };

                repository.Add(order);
            }

            return suppliers.Count;
        }
    }
}
