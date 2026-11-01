using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Business
{
    public class InventoryManager
    {
        private readonly InventoryRepository _repository =
            new InventoryRepository();

        // Lista todos los registros de inventario.
        public IEnumerable<Inventory> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un registro por su ID.
        public Inventory GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Cada libro puede tener un único registro de inventario.
        public bool BookExists(int bookId, int id)
        {
            return _repository.GetAll().Any(item =>
                item.BookId == bookId && item.Id != id);
        }

        // Agrega un registro.
        public void Add(Inventory inventory)
        {
            _repository.Add(inventory);
        }

        // Actualiza un registro.
        public void Update(Inventory inventory)
        {
            _repository.Update(inventory);
        }

        // Elimina un registro.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
