using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class SupplierManager
    {
        private readonly SupplierRepository _repository =
            new SupplierRepository();

        // Obtiene todos los proveedores.
        public IEnumerable<Supplier> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un proveedor por su ID.
        public Supplier GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Agrega un proveedor.
        public void Add(Supplier supplier)
        {
            _repository.Add(supplier);
        }

        // Actualiza un proveedor existente.
        public void Update(Supplier supplier)
        {
            _repository.Update(supplier);
        }

        // Elimina un proveedor por su ID.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
