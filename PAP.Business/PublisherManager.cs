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
    public class PublisherManager
    {
        private readonly PublisherRepository _repository =
            new PublisherRepository();

        // Lista todas las editoriales.
        public IEnumerable<Publisher> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca una editorial por su ID.
        public Publisher GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Guarda una editorial nueva.
        public void Add(Publisher publisher)
        {
            _repository.Add(publisher);
        }

        // Actualiza una editorial existente.
        public void Update(Publisher publisher)
        {
            _repository.Update(publisher);
        }

        // Elimina una editorial por su ID.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
