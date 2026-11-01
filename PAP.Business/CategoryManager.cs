using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Business
{
    public class CategoryManager
    {
        private readonly CategoryRepository _repository =
            new CategoryRepository();

        // Obtiene todas las categorías.
        public IEnumerable<Category> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca una categoría por su identificador.
        public Category GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Comprueba nombres duplicados, excluyendo el registro editado.
        public bool NameExists(string name, int id)
        {
            return _repository.GetAll().Any(category =>
                category.Id != id &&
                string.Equals(
                    category.Name,
                    name.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        // Agrega una categoría.
        public void Add(Category category)
        {
            _repository.Add(category);
        }

        // Actualiza una categoría.
        public void Update(Category category)
        {
            _repository.Update(category);
        }

        // Elimina una categoría.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
