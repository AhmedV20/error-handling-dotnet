using ErrorLens.ErrorHandling.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ErrorLens.ErrorHandling.Handlers;

/// <summary>
/// Handles AggregateException by unwrapping single-inner-exception aggregates
/// and re-dispatching to the appropriate specific handler.
/// Multi-exception or empty aggregates are delegated to the fallback handler.
/// </summary>
/// <remarks>
/// Uses <see cref="IServiceProvider"/> for lazy resolution of handlers to avoid
/// a circular dependency (this handler is itself an IApiExceptionHandler).
/// </remarks>
public class AggregateExceptionHandler : AbstractApiExceptionHandler
{
    private readonly IServiceProvider _serviceProvider;

    public AggregateExceptionHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public override int Order => 50;

    /// <inheritdoc />
    public override bool CanHandle(Exception exception)
    {
        return exception is AggregateException;
    }

    /// <inheritdoc />
    public override ApiErrorResponse Handle(Exception exception)
    {
        if (exception is not AggregateException aggregateException)
            throw new InvalidOperationException($"Cannot handle exception of type {exception.GetType()}. Call CanHandle first.");

        // Flatten nested aggregates and get all inner exceptions
        var flattened = aggregateException.Flatten();
        var innerExceptions = flattened.InnerExceptions;

        // Single inner exception: unwrap and re-dispatch to the handler chain
        if (innerExceptions.Count == 1)
        {
            var innerException = innerExceptions[0];

            // Lazily resolve handlers to break the circular dependency
            var handlers = _serviceProvider.GetServices<IApiExceptionHandler>();
            var fallbackHandler = _serviceProvider.GetRequiredService<IFallbackApiExceptionHandler>();

            // Find a specific handler for the inner exception (skip this handler to avoid recursion)
            var handler = handlers
                .Where(h => h != this)
                .OrderBy(h => h.Order)
                .FirstOrDefault(h => h.CanHandle(innerException));

            return handler != null
                ? handler.Handle(innerException)
                : fallbackHandler.Handle(innerException);
        }

        // Multi-exception or zero-exception: delegate to fallback
        var fallback = _serviceProvider.GetRequiredService<IFallbackApiExceptionHandler>();
        return fallback.Handle(aggregateException);
    }
}
