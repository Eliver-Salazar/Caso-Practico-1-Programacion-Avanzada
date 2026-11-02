using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class InventoryGenerator : IGenerator
    {
        // Inventario requiere libros y debe estar vacío.
        public GeneratorStatus GetStatus()
        {
            if (new InventoryRepository().GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "Inventory",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            bool hasBooks = new BookRepository().GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Inventory",
                CanGenerate = hasBooks,
                Message = hasBooks
                    ? "Disponible para generar."
                    : "Primero agregá o generá Books."
            };
        }

        // Genera inventario para hasta diez libros existentes.
        public int Generate()
        {
            var repository = new InventoryRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se puede generar inventario: la tabla ya contiene datos.");
            }

            var books = new BookRepository()
                .GetAll()
                .OrderBy(book => book.Id)
                .Take(10)
                .ToList();

            if (!books.Any())
            {
                throw new InvalidOperationException(
                    "Primero agregá o generá Books.");
            }

            for (int i = 0; i < books.Count; i++)
            {
                // Cada libro aparece una sola vez, respetando la clave única.
                var inventory = new Inventory
                {
                    BookId = books[i].Id,
                    QuantityInStock = 10 + i * 5,
                    ReorderLevel = 5,
                    LastRestockedAt = DateTime.UtcNow
                };

                repository.Add(inventory);
            }

            return books.Count;
        }
    }
}
