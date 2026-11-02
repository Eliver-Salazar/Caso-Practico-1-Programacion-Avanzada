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
    public class PaymentManager
    {
        private readonly PaymentRepository _repository =
            new PaymentRepository();

        // Lista todos los pagos.
        public IEnumerable<Payment> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un pago por su ID.
        public Payment GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Agrega utilizando el repository base.
        public void Add(Payment payment)
        {
            _repository.Add(payment);
        }

        // Actualiza utilizando el repository base.
        public void Update(Payment payment)
        {
            _repository.Update(payment);
        }

        // Elimina utilizando el repository base.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
