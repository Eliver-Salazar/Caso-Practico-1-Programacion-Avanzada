using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Business
{
    public class CartItemManager
    {
        private readonly CartItemRepository _repository =
            new CartItemRepository();

        // Lista todos los artículos.
        public IEnumerable<CartItem> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un artículo por su ID.
        public CartItem GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Comprueba que el libro no esté repetido en el mismo carrito.
        public bool ItemExists(int cartId, int bookId, int id)
        {
            return _repository.GetAll().Any(item =>
                item.CartId == cartId &&
                item.BookId == bookId &&
                item.Id != id);
        }

        // Agrega un artículo.
        public void Add(CartItem item)
        {
            _repository.Add(item);
        }

        // Actualiza un artículo.
        public void Update(CartItem item)
        {
            _repository.Update(item);
        }

        // Elimina un artículo.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
