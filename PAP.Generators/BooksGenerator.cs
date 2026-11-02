using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.Repositories;

// Evita confundir PAP.DataAccess.Book con PAP.Models.Book.
using Book = PAP.DataAccess.Book;

namespace PAP.Generators
{
    public class BooksGenerator : IGenerator
    {
        // Comprueba la tabla y los catálogos que utilizará el generador.
        public GeneratorStatus GetStatus()
        {
            if (new BookRepository().GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "Books",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            var missing = new List<string>();

            if (!new CategoryRepository().GetAll().Any())
                missing.Add("Categories");

            if (!new GenreRepository().GetAll().Any())
                missing.Add("Genres");

            if (!new PublisherRepository().GetAll().Any())
                missing.Add("Publishers");

            return new GeneratorStatus
            {
                EntityName = "Books",
                CanGenerate = missing.Count == 0,
                Message = missing.Count == 0
                    ? "Disponible para generar."
                    : "Primero agregá o generá: " + string.Join(", ", missing) + "."
            };
        }

        // Genera diez libros usando IDs reales de los catálogos.
        public int Generate()
        {
            var repository = new BookRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar libros: la tabla ya contiene datos.");
            }

            var categories = new CategoryRepository()
                .GetAll().OrderBy(item => item.Id).ToList();

            var genres = new GenreRepository()
                .GetAll().OrderBy(item => item.Id).ToList();

            var publishers = new PublisherRepository()
                .GetAll().OrderBy(item => item.Id).ToList();

            if (!categories.Any() || !genres.Any() || !publishers.Any())
            {
                throw new InvalidOperationException(
                    "Primero agregá o generá Categories, Genres y Publishers.");
            }

            const int count = 10;

            for (int i = 0; i < count; i++)
            {
                int number = i + 1;
                DateTime now = DateTime.UtcNow;

                var category = categories[i % categories.Count];
                var genre = genres[i % genres.Count];
                var publisher = publishers[i % publishers.Count];

                var book = new Book
                {
                    Name = "Libro " + number,
                    Title = "Libro de prueba " + number,
                    Description = "Libro generado para el caso práctico.",

                    // Conserva los campos originales del proyecto.
                    Category = category.Name,
                    Genre = genre.Name,
                    Publisher = publisher.Name,
                    Author = "Autor de prueba " + number,

                    // Establece las nuevas relaciones mediante sus IDs reales.
                    CategoryId = category.Id,
                    GenreId = genre.Id,
                    PublisherId = publisher.Id,

                    ISBN = CreateISBN(number),
                    Pages = 100 + number * 20,
                    Language = "Español",
                    CoverImageUrl = null,
                    PublicationDate = new DateTime(2020, 1, 1).AddMonths(i),
                    Height = 21m,
                    Width = 14m,
                    Price = 1000m + number * 500m,
                    IsBestSeller = i % 3 == 0,

                    // Las existencias se generarán por separado en Inventory.
                    IsInStock = false,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                repository.Add(book);
            }

            return count;
        }

        // Construye un ISBN-13 de prueba con su dígito de control.
        private static string CreateISBN(int number)
        {
            string prefix = "978" + number.ToString("D9");
            int sum = 0;

            for (int i = 0; i < prefix.Length; i++)
            {
                int digit = prefix[i] - '0';
                sum += digit * (i % 2 == 0 ? 1 : 3);
            }

            int checkDigit = (10 - sum % 10) % 10;

            return prefix + checkDigit;
        }
    }
}
