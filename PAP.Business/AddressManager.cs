using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Business
{
    public class AddressManager
    {
        private readonly AddressRepository _repository =
            new AddressRepository();

        // Lista todas las direcciones.
        public IEnumerable<Address> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca una dirección por su ID.
        public Address GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Guarda una dirección nueva.
        public void Add(Address address)
        {
            _repository.Add(address);
        }

        // Actualiza una dirección existente.
        public void Update(Address address)
        {
            _repository.Update(address);
        }

        // Elimina una dirección por su ID.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
