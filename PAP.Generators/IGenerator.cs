using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAP.Generators
{
    // Contrato común para todos los generadores.
    public interface IGenerator
    {
        // Consulta si la entidad puede generarse.
        GeneratorStatus GetStatus();

        // Genera los registros y devuelve la cantidad creada.
        int Generate();
    }
}
