using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;


namespace PAP.Business
{
    public class GenreManager
    {
        private readonly GenreRepository _repository =
            new GenreRepository();

        // Obtiene todos los géneros.
        public IEnumerable<Genre> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un género por su ID.
        public Genre GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Comprueba duplicados excluyendo el registro que se está editando.
        public bool NameExists(string name, int id)
        {
            return _repository.GetAll().Any(genre =>
                genre.Id != id &&
                string.Equals(
                    genre.Name,
                    name.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        // Guarda un género nuevo.
        public void Add(Genre genre)
        {
            _repository.Add(genre);
        }

        // Actualiza un género existente.
        public void Update(Genre genre)
        {
            _repository.Update(genre);
        }

        // Elimina un género por su ID.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
