using System.Globalization;
using System.Web.Mvc;

namespace PAP.Mvc.ModelBinders
{
    // Ejemplo: Model Binder personalizado
    // Convierte valores como "$18.99" o "18,99" a decimal para el precio del libro.
    public class CurrencyModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == null || string.IsNullOrWhiteSpace(valueResult.AttemptedValue))
            {
                return 0m;
            }

            var raw = valueResult.AttemptedValue.Trim();
            raw = raw.Replace("$", string.Empty).Replace("€", string.Empty).Trim();

            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
            {
                return price;
            }

            if (decimal.TryParse(raw, NumberStyles.Number, new CultureInfo("es-MX"), out price))
            {
                return price;
            }

            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName,
                "El precio no tiene un formato válido.");

            return 0m;
        }
    }
}
