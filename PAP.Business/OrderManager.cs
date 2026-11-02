using System;
using System.Collections.Generic;


using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Business
{
    public class OrderManager
    {
        private readonly OrderRepository _repository = new OrderRepository();

        // Lista todos los pedidos.
        public IEnumerable<Order> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un pedido por su ID.
        public Order GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Guarda un pedido mediante el repository base.
        public void Add(Order order)
        {
            _repository.Add(order);
        }

        // Actualiza un pedido mediante el repository base.
        public void Update(Order order)
        {
            _repository.Update(order);
        }

        // Elimina un pedido mediante el repository base.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
