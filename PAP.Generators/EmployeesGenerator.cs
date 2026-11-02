using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.Repositories;
using System.Web.Helpers;

// Evita confundir la entidad con PAP.Models.Employee.
using Employee = PAP.DataAccess.Employee;

namespace PAP.Generators
{
    public class EmployeesGenerator : IGenerator
    {
        // Determina si el botón debe habilitarse.
        public GeneratorStatus GetStatus()
        {
            var repository = new EmployeeRepository();

            if (repository.GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "Employees",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            var roles = new RoleRepository();
            bool hasRoles = roles.GetAll().Any();

            return new GeneratorStatus
            {
                EntityName = "Employees",
                CanGenerate = hasRoles,
                Message = hasRoles
                    ? "Disponible para generar."
                    : "Primero agregá o generá Roles."
            };
        }

        // Genera empleados utilizando IDs reales de los roles existentes.
        public int Generate()
        {
            var repository = new EmployeeRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar empleados: la tabla ya contiene datos.");
            }

            var roles = new RoleRepository()
                .GetAll()
                .OrderBy(role => role.Id)
                .ToList();

            if (!roles.Any())
            {
                throw new InvalidOperationException(
                    "Primero agregá o generá Roles.");
            }

            string[] firstNames =
            {
                "Gabriela", "Pablo", "Andrea", "Jorge", "Daniela",
                "Marco", "Natalia", "Esteban", "Paola", "Ricardo"
            };

            string[] lastNames =
            {
                "Mora", "Soto", "Vargas", "Rojas", "Castro",
                "Solís", "Herrera", "Ramírez", "Alfaro", "Jiménez"
            };

            for (int i = 0; i < firstNames.Length; i++)
            {
                var employee = new Employee
                {
                    // Distribuye los empleados entre los roles disponibles.
                    RoleId = roles[i % roles.Count].Id,
                    FirstName = firstNames[i],
                    LastName = lastNames[i],
                    Email = "empleado" + (i + 1) + "@example.com",
                    PasswordHash = Crypto.HashPassword(
                        Guid.NewGuid().ToString("N")),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                repository.Add(employee);
            }

            return firstNames.Length;
        }
    }
}
