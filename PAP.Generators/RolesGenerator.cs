using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.Repositories;


// Evitar confundir PAP.DataAccess.Role con PAP.Models.Role.
using Role = PAP.DataAccess.Role;

namespace PAP.Generators
{
    public class RolesGenerator : IGenerator
    {
        // Habilita la generación únicamente cuando la tabla está vacía.
        public GeneratorStatus GetStatus()
        {
            var repository = new RoleRepository();
            bool hasData = repository.GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Roles",
                CanGenerate = !hasData,
                Message = hasData
                    ? "La tabla ya contiene datos."
                    : "Disponible para generar."
            };
        }

        // Genera los roles mediante su repository.
        public int Generate()
        {
            var repository = new RoleRepository();

            // Verifica nuevamente antes de insertar.
            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar roles: la tabla ya contiene datos.");
            }

            string[] names =
            {
                "Admin",
                "Manager",
                "Cashier"
            };

            foreach (string name in names)
            {
                repository.Add(new Role
                {
                    Name = name
                });
            }

            return names.Length;
        }
    }
}
