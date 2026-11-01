using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Business
{
    public class CustomerManager
    {
        private readonly CustomerRepository _repository =
            new CustomerRepository();

        // Lista todos los clientes.
        public IEnumerable<Customer> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un cliente por su ID.
        public Customer GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Verifica correos duplicados excluyendo al cliente editado.
        public bool EmailExists(string email, int id)
        {
            return _repository.GetAll().Any(customer =>
                customer.Id != id &&
                string.Equals(
                    customer.Email,
                    email,
                    StringComparison.OrdinalIgnoreCase));
        }

        // Guarda un cliente nuevo.
        public void Add(Customer customer)
        {
            _repository.Add(customer);
        }

        // Actualiza un cliente existente.
        public void Update(Customer customer)
        {
            _repository.Update(customer);
        }

        // Elimina un cliente por su ID.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
