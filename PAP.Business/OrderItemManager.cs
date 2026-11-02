using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Business
{
    public class OrderItemManager
    {
        private readonly OrderItemRepository _repository =
            new OrderItemRepository();

        // Lista todos los artículos.
        public IEnumerable<OrderItem> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un artículo por su ID.
        public OrderItem GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Agrega usando el repository base.
        public void Add(OrderItem item)
        {
            _repository.Add(item);
        }

        // Actualiza usando el repository base.
        public void Update(OrderItem item)
        {
            _repository.Update(item);
        }

        // Elimina usando el repository base.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
