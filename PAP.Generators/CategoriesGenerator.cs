using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class CategoriesGenerator : IGenerator
    {
        // Permite generar únicamente si la tabla está vacía.
        public GeneratorStatus GetStatus()
        {
            var repository = new CategoryRepository();
            bool hasData = repository.GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Categories",
                CanGenerate = !hasData,
                Message = hasData
                    ? "La tabla ya contiene datos."
                    : "Disponible para generar."
            };
        }

        // Genera categorías mediante el repository de la entidad.
        public int Generate()
        {
            var repository = new CategoryRepository();

            // Repite la validación en servidor antes de insertar.
            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar categorías: la tabla ya contiene datos.");
            }

            string[] names =
            {
                "Literatura",
                "Tecnología",
                "Ciencias",
                "Historia",
                "Educación",
                "Arte",
                "Negocios",
                "Salud",
                "Infantil",
                "Idiomas"
            };

            foreach (string name in names)
            {
                var category = new Category
                {
                    Name = name,
                    Description = "Libros de la categoría " + name + "."
                };

                repository.Add(category);
            }

            return names.Length;
        }
    }
}
