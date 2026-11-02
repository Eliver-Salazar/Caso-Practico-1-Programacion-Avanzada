using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.Repositories;

namespace PAP.Generators
{
    public class BookAuthorsGenerator : IGenerator
    {
        // Comprueba si existen asociaciones y sus entidades requeridas.
        public GeneratorStatus GetStatus()
        {
            if (new BookAuthorRepository().GetLinks().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "BookAuthors",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            var missing = new List<string>();

            if (!new BookRepository().GetAll().Any())
                missing.Add("Books");

            if (!new AuthorRepository().GetAll().Any())
                missing.Add("Authors");

            return new GeneratorStatus
            {
                EntityName = "BookAuthors",
                CanGenerate = missing.Count == 0,
                Message = missing.Count == 0
                    ? "Disponible para generar."
                    : "Primero agregá o generá: " +
                      string.Join(", ", missing) + "."
            };
        }

        // Genera una asociación para cada uno de los primeros diez libros.
        public int Generate()
        {
            var repository = new BookAuthorRepository();

            if (repository.GetLinks().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar asociaciones: la tabla ya contiene datos.");
            }

            var books = new BookRepository()
                .GetAll()
                .OrderBy(book => book.Id)
                .Take(10)
                .ToList();

            var authors = new AuthorRepository()
                .GetAll()
                .OrderBy(author => author.Id)
                .Take(10)
                .ToList();

            if (!books.Any() || !authors.Any())
            {
                throw new InvalidOperationException(
                    "Primero agregá o generá Books y Authors.");
            }

            for (int i = 0; i < books.Count; i++)
            {
                // Usa IDs reales y distribuye los autores disponibles.
                // Los identificadores originales son null porque es una inserción.
                repository.SaveLink(
                    books[i].Id,
                    authors[i % authors.Count].Id,
                    null,
                    null);
            }

            return books.Count;
        }
    }
}
