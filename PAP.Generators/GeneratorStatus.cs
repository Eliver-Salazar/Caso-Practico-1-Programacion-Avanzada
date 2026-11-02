using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAP.Generators
{
    // Representa el estado de una entidad en la pantalla Admin.
    public class GeneratorStatus
    {
        // Nombre de la entidad o tabla.
        public string EntityName { get; set; }

        // Indica si el botón de generación puede habilitarse.
        public bool CanGenerate { get; set; }

        // Explica por qué se permite o se bloquea la generación.
        public string Message { get; set; }
    }
}
