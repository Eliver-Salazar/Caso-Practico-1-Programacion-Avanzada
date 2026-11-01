using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Business
{
    public class ShoppingCartManager
    {
        private readonly ShoppingCartRepository _repository =
            new ShoppingCartRepository();

        // Lista todos los carritos.
        public IEnumerable<ShoppingCart> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un carrito por su ID.
        public ShoppingCart GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Guarda un carrito nuevo.
        public void Add(ShoppingCart cart)
        {
            _repository.Add(cart);
        }

        // Actualiza un carrito existente.
        public void Update(ShoppingCart cart)
        {
            _repository.Update(cart);
        }

        // Elimina un carrito por su ID.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}