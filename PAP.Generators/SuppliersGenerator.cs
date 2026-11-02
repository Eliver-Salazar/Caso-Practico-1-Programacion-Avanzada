using System;
using System.Linq;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class SuppliersGenerator : IGenerator
    {
        // Habilita la generación únicamente cuando la tabla está vacía.
        public GeneratorStatus GetStatus()
        {
            var repository = new SupplierRepository();
            bool hasData = repository.GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Suppliers",
                CanGenerate = !hasData,
                Message = hasData
                    ? "La tabla ya contiene datos."
                    : "Disponible para generar."
            };
        }

        // Genera diez proveedores utilizando su repository.
        public int Generate()
        {
            var repository = new SupplierRepository();

            // Valida nuevamente antes de insertar.
            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar proveedores: la tabla ya contiene datos.");
            }

            string[] names =
            {
                "Distribuidora Central",
                "Libros del Valle",
                "Distribuciones Horizonte",
                "Suministros del Norte",
                "Comercial del Libro",
                "Distribuidora Aurora",
                "Libros del Pacífico",
                "Distribuciones Cumbre",
                "Suministros Educativos",
                "Distribuidora Semilla"
            };

            for (int i = 0; i < names.Length; i++)
            {
                int number = i + 1;

                var supplier = new Supplier
                {
                    Name = names[i],
                    ContactName = "Contacto de prueba " + number,
                    Email = "proveedor" + number + "@example.com",
                    Phone = "2200" + number.ToString("D4"),
                    Address = "Dirección de prueba " + number + ", Costa Rica",
                    IsActive = true
                };

                repository.Add(supplier);
            }

            return names.Length;
        }
    }
}
