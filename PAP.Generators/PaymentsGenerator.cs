using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class PaymentsGenerator : IGenerator
    {
        // Requiere al menos un pedido con total positivo.
        public GeneratorStatus GetStatus()
        {
            if (new PaymentRepository().GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "Payments",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            var orders = new OrderRepository().GetAll().ToList();

            if (!orders.Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "Payments",
                    CanGenerate = false,
                    Message = "Primero agregá o generá Orders."
                };
            }

            bool hasPositiveOrders = orders.Any(order => order.Total > 0);

            return new GeneratorStatus
            {
                EntityName = "Payments",
                CanGenerate = hasPositiveOrders,
                Message = hasPositiveOrders
                    ? "Disponible para generar."
                    : "Debe existir un pedido con total mayor que cero."
            };
        }

        // Genera un pago por cada uno de hasta diez pedidos con total positivo.
        public int Generate()
        {
            var repository = new PaymentRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar pagos: la tabla ya contiene datos.");
            }

            var orders = new OrderRepository()
                .GetAll()
                .Where(order => order.Total > 0)
                .OrderBy(order => order.Id)
                .Take(10)
                .ToList();

            if (!orders.Any())
            {
                throw new InvalidOperationException(
                    "Primero agregá o generá pedidos con total mayor que cero.");
            }

            string[] methods = { "Cash", "Card", "Transfer", "SINPE" };

            for (int i = 0; i < orders.Count; i++)
            {
                var payment = new Payment
                {
                    OrderId = orders[i].Id,
                    PaymentMethod = methods[i % methods.Length],
                    Amount = orders[i].Total,
                    PaymentDate = DateTime.UtcNow,
                    Status = "Completed",
                    TransactionRef = "PRUEBA-" + Guid.NewGuid().ToString("N")
                };

                repository.Add(payment);
            }

            return orders.Count;
        }
    }
}
