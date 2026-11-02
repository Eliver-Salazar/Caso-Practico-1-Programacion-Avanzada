using System;

using System.Linq;


using PAP.DataAccess;
using PAP.Repositories;
using System.Web.Helpers;

namespace PAP.Generators
{
    public class CustomersGenerator : IGenerator
    {
        // Consulta si la tabla permite generar clientes.
        public GeneratorStatus GetStatus()
        {
            var repository = new CustomerRepository();
            bool hasData = repository.GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Customers",
                CanGenerate = !hasData,
                Message = hasData
                    ? "La tabla ya contiene datos."
                    : "Disponible para generar."
            };
        }

        // Genera diez clientes con correos distintos y contraseñas protegidas.
        public int Generate()
        {
            var repository = new CustomerRepository();

            // Impide generar sobre una tabla que ya tiene registros.
            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar clientes: la tabla ya contiene datos.");
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

            for (int i = 0; i < firstNames.Length; i++)
            {
                int number = i + 1;
                DateTime now = DateTime.UtcNow;

                var customer = new Customer
                {
                    FirstName = firstNames[i],
                    LastName = lastNames[i],
                    Email = "cliente" + number + "@example.com",
                    Phone = "8800" + number.ToString("D4"),
                    BirthDate = new DateTime(
                        1980 + i * 2,
                        i % 12 + 1,
                        i + 1),
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,

                    // Genera una contraseña aleatoria y guarda solo su hash.
                    PasswordHash = Crypto.HashPassword(
                        Guid.NewGuid().ToString("N"))
                };

                repository.Add(customer);
            }

            return firstNames.Length;
        }
    }
}