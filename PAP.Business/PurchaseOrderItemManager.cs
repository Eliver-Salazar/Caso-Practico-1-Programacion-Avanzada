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
    public class PurchaseOrderItemManager
    {
        private readonly PurchaseOrderItemRepository _repository =
            new PurchaseOrderItemRepository();

        // Lista todos los artículos de compra.
        public IEnumerable<PurchaseOrderItem> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un artículo por su ID.
        public PurchaseOrderItem GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Agrega usando el método heredado del repository base.
        public void Add(PurchaseOrderItem item)
        {
            _repository.Add(item);
        }

        // Actualiza usando el método heredado del repository base.
        public void Update(PurchaseOrderItem item)
        {
            _repository.Update(item);
        }

        // Elimina usando el método heredado del repository base.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}