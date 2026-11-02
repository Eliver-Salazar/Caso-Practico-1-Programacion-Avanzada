using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Linq;

namespace PAP.Generators
{
    public class PublishersGenerator : IGenerator
    {
        // Consulta si existen editoriales.
        public GeneratorStatus GetStatus()
        {
            var repository = new PublisherRepository();
            bool hasData = repository.GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Publishers",
                CanGenerate = !hasData,
                Message = hasData
                    ? "La tabla ya contiene datos."
                    : "Disponible para generar."
            };
        }

        // Construye diez editoriales de prueba.
        public int Generate()
        {
            var repository = new PublisherRepository();

            // Impide generar sobre una tabla que ya tiene registros.
            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar editoriales: la tabla ya contiene datos.");
            }

            string[] names =
            {
                "Editorial Horizonte",
                "Letras del Valle",
                "Ediciones del Sol",
                "Editorial Bosque",
                "Páginas del Sur",
                "Ediciones Aurora",
                "Editorial Saber",
                "Letras del Mar",
                "Ediciones Cumbre",
                "Editorial Semilla"
            };

            string[] countries =
            {
                "Costa Rica",
                "México",
                "Colombia",
                "Argentina",
                "España"
            };

            for (int i = 0; i < names.Length; i++)
            {
                int number = i + 1;

                var publisher = new Publisher
                {
                    Name = names[i],
                    Country = countries[i % countries.Length],

                    // Direcciones reservadas para ejemplos y datos de prueba.
                    Website = "https://editorial" + number + ".example.com",
                    Email = "contacto" + number + "@example.com",

                    CreatedAt = DateTime.UtcNow
                };

                repository.Add(publisher);
            }

            return names.Length;
        }
    }
}
