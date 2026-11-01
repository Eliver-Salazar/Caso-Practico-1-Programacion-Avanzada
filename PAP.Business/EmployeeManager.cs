using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.Repositories;

// Define exactamente cuál entidad Employee utiliza esta clase.
using Employee = PAP.DataAccess.Employee;

namespace PAP.Business
{
    public class EmployeeManager
    {
        private readonly EmployeeRepository _repository =
            new EmployeeRepository();

        // Obtiene todos los empleados.
        public IEnumerable<Employee> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un empleado por su ID.
        public Employee GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Comprueba correos duplicados excluyendo al empleado editado.
        public bool EmailExists(string email, int id)
        {
            return _repository.GetAll().Any(employee =>
                employee.Id != id &&
                string.Equals(
                    employee.Email,
                    email,
                    StringComparison.OrdinalIgnoreCase));
        }

        // Guarda un empleado nuevo.
        public void Add(Employee employee)
        {
            _repository.Add(employee);
        }

        // Actualiza un empleado existente.
        public void Update(Employee employee)
        {
            _repository.Update(employee);
        }

        // Elimina un empleado por su ID.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
