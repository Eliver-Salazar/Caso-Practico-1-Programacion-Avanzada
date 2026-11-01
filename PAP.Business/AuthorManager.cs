using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;


namespace PAP.Business
{
    public class AuthorManager
    {
        private readonly AuthorRepository _repository;

        public AuthorManager()
        {
            _repository = new AuthorRepository();
        }

        public IEnumerable<Author> GetAll()
        {
            return _repository.GetAll();
        }

        public Author GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Author author)
        {
            _repository.Add(author);
        }

        public void Update(Author author)
        {
            _repository.Update(author);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
