using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class ReviewsGenerator : IGenerator
    {
        // Comprueba la tabla y sus dependencias.
        public GeneratorStatus GetStatus()
        {
            if (new ReviewRepository().GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "Reviews",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            var missing = new List<string>();

            if (!new BookRepository().GetAll().Any())
                missing.Add("Books");

            if (!new CustomerRepository().GetAll().Any())
                missing.Add("Customers");

            return new GeneratorStatus
            {
                EntityName = "Reviews",
                CanGenerate = missing.Count == 0,
                Message = missing.Count == 0
                    ? "Disponible para generar."
                    : "Primero agregá o generá: " +
                      string.Join(", ", missing) + "."
            };
        }

        // Genera una reseña para cada uno de los primeros diez libros.
        public int Generate()
        {
            var repository = new ReviewRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar reseñas: la tabla ya contiene datos.");
            }

            var books = new BookRepository()
                .GetAll()
                .OrderBy(book => book.Id)
                .Take(10)
                .ToList();

            var customers = new CustomerRepository()
                .GetAll()
                .OrderBy(customer => customer.Id)
                .Take(10)
                .ToList();

            if (!books.Any() || !customers.Any())
            {
                throw new InvalidOperationException(
                    "Primero agregá o generá Books y Customers.");
            }

            string[] comments =
            {
                "La lectura cumplió mis expectativas.",
                "El contenido está bien organizado.",
                "Una lectura interesante.",
                "Me gustó la forma de presentar los temas.",
                "Recomendaría este libro."
            };

            for (int i = 0; i < books.Count; i++)
            {
                var review = new Review
                {
                    // Cada libro aparece una sola vez en este lote.
                    // Así no se repite la combinación libro-cliente.
                    BookId = books[i].Id,
                    CustomerId = customers[i % customers.Count].Id,
                    Rating = (byte)(i % 5 + 1),
                    Comment = comments[i % comments.Length],
                    CreatedAt = DateTime.UtcNow
                };

                repository.Add(review);
            }

            return books.Count;
        }
    }
}
