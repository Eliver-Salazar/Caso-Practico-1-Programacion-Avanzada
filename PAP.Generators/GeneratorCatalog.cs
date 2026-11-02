using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAP.Generators
{
    public class GeneratorCatalog
    {
        // Registra los generadores disponibles.
        private readonly Dictionary<string, IGenerator> _generators =
            new Dictionary<string, IGenerator>(StringComparer.OrdinalIgnoreCase)
            {
                { "Authors", new AuthorsGenerator() }
            };

        // Consulta el estado de cada entidad para construir la pantalla.
        public IEnumerable<GeneratorStatus> GetStatuses()
        {
            return _generators.Values
                .Select(generator => generator.GetStatus())
                .OrderBy(status => status.EntityName)
                .ToList();
        }

        // Valida la entidad y sus condiciones antes de generar.
        public int Generate(string entityName)
        {
            IGenerator generator;

            if (string.IsNullOrWhiteSpace(entityName) ||
                !_generators.TryGetValue(entityName, out generator))
            {
                throw new InvalidOperationException(
                    "La entidad seleccionada no tiene un generador registrado.");
            }

            var status = generator.GetStatus();

            if (!status.CanGenerate)
                throw new InvalidOperationException(status.Message);

            // El generador también comprueba sus condiciones internamente.
            return generator.Generate();
        }
    }
}
