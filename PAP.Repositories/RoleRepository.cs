using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAP.Repositories
{
    // Usa la entidad de DataAccess para evitar confundirla con PAP.Models.Role.
    public class RoleRepository : RepositoryBase<PAP.DataAccess.Role>
    {
    }
}
