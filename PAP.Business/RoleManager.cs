using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.Repositories;

namespace PAP.Business
{
    public class RoleManager
    {
        private readonly RoleRepository _repository = new RoleRepository();

        // Lista todos los roles.
        public IEnumerable<PAP.DataAccess.Role> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca un rol por su ID.
        public PAP.DataAccess.Role GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Detecta nombres duplicados excluyendo el rol editado.
        public bool NameExists(string name, int id)
        {
            return _repository.GetAll().Any(role =>
                role.Id != id &&
                string.Equals(
                    role.Name,
                    name.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        // Guarda un rol nuevo.
        public void Add(PAP.DataAccess.Role role)
        {
            _repository.Add(role);
        }

        // Actualiza un rol existente.
        public void Update(PAP.DataAccess.Role role)
        {
            _repository.Update(role);
        }

        // Elimina un rol por su ID.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
