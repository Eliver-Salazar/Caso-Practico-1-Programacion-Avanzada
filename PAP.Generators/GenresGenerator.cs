using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class GenresGenerator : IGenerator
    {
        // Permite generar únicamente si la tabla está vacía.
        public GeneratorStatus GetStatus()
        {
            var repository = new GenreRepository();
            bool hasData = repository.GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Genres",
                CanGenerate = !hasData,
                Message = hasData
                    ? "La tabla ya contiene datos."
                    : "Disponible para generar."
            };
        }

        // Genera géneros mediante el repository de la entidad.
        public int Generate()
        {
            var repository = new GenreRepository();

            // Impide generar aunque se intente enviar el formulario manualmente.
            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar géneros: la tabla ya contiene datos.");
            }

            string[] names =
            {
                "Novela",
                "Cuento",
                "Poesía",
                "Ensayo",
                "Biografía",
                "Fantasía",
                "Ciencia ficción",
                "Misterio",
                "Aventura",
                "Drama"
            };

            foreach (string name in names)
            {
                var genre = new Genre
                {
                    Name = name,
                    Description = "Obras del género " + name + "."
                };

                repository.Add(genre);
            }

            return names.Length;
        }
    }
}
