using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MiRoti.ModelBinders;

public sealed class InvariantDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var rawValue = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            if (Nullable.GetUnderlyingType(bindingContext.ModelType) != null)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "El valor no puede estar vacío.");
            return Task.CompletedTask;
        }

        var styles = NumberStyles.Number;
        var candidates = new[]
        {
            rawValue,
            rawValue.Replace(',', '.'),
        };

        foreach (var candidate in candidates)
        {
            if (decimal.TryParse(candidate, styles, CultureInfo.InvariantCulture, out var invariantParsed))
            {
                bindingContext.Result = ModelBindingResult.Success(invariantParsed);
                return Task.CompletedTask;
            }

            if (decimal.TryParse(candidate, styles, CultureInfo.CurrentCulture, out var currentParsed))
            {
                bindingContext.Result = ModelBindingResult.Success(currentParsed);
                return Task.CompletedTask;
            }
        }

        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"Valor decimal inválido: '{rawValue}'.");
        return Task.CompletedTask;
    }
}
