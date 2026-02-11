using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Exception that wraps ModelStateDictionary validation errors.
/// Thrown when [ApiController] automatic model validation fails,
/// allowing ErrorLens to produce structured fieldErrors responses.
/// </summary>
public class ModelStateValidationException : Exception
{
    /// <summary>
    /// The ModelStateDictionary containing validation errors.
    /// </summary>
    public ModelStateDictionary ModelState { get; }

    public ModelStateValidationException(ModelStateDictionary modelState)
        : base("Validation failed")
    {
        ModelState = modelState;
    }
}
