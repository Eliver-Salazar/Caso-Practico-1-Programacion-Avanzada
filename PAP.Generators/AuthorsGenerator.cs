using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class AuthorsGenerator : IGenerator
    {
        // Consulta el estado actual mediante el repository.
        public GeneratorStatus GetStatus()
        {
            var repository = new AuthorRepository();
            bool hasData = repository.GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Authors",
                CanGenerate = !hasData,
                Message = hasData
                    ? "La tabla ya contiene datos."
                    : "Disponible para generar."
            };
        }

        // Crea diez autores usando el repository de la entidad.
        public int Generate()
        {
            var repository = new AuthorRepository();

            // Valida también en servidor, aunque el botón esté deshabilitado.
            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar autores: la tabla ya contiene datos.");
            }

            string[] firstNames =
            {
                "Ana", "Luis", "María", "Carlos", "Sofía",
                "Diego", "Laura", "Andrés", "Valeria", "Daniel"
            };

            string[] lastNames =
            {
                "Mora", "Vargas", "Soto", "Rojas", "Castro",
                "Ramírez", "Solís", "Herrera", "Jiménez", "Alfaro"
            };

            string[] countries =
            {
                "Costa Rica", "México", "Colombia", "Argentina", "España"
            };

            for (int i = 0; i < firstNames.Length; i++)
            {
                // Construye un registro distinto en cada iteración.
                var author = new Author
                {
                    FirstName = firstNames[i],
                    LastName = lastNames[i],
                    Biography = "Autor de prueba número " + (i + 1) + ".",
                    BirthDate = new DateTime(
                        1965 + i * 3,
                        i % 12 + 1,
                        i + 1),
                    Country = countries[i % countries.Length],
                    CreatedAt = DateTime.UtcNow
                };

                // Add utiliza el guardado del repository base.
                repository.Add(author);
            }

            return firstNames.Length;
        }
    }
}
