using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Business
{
    public class PurchaseOrderManager
    {
        private readonly PurchaseOrderRepository _repository =
            new PurchaseOrderRepository();

        // Obtiene todas las órdenes.
        public IEnumerable<PurchaseOrder> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca una orden por su ID.
        public PurchaseOrder GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Guarda una orden nueva.
        public void Add(PurchaseOrder order)
        {
            _repository.Add(order);
        }

        // Actualiza una orden existente.
        public void Update(PurchaseOrder order)
        {
            _repository.Update(order);
        }

        // Elimina una orden por su ID.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
