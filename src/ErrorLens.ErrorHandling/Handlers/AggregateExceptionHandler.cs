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
    private readonly Lazy<(IReadOnlyList<IApiExceptionHandler> handlers, IFallbackApiExceptionHandler fallback)> _resolved;

    public AggregateExceptionHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _resolved = new Lazy<(IReadOnlyList<IApiExceptionHandler>, IFallbackApiExceptionHandler)>(() =>
        {
            var handlers = _serviceProvider.GetServices<IApiExceptionHandler>()
                .Where(h => h.GetType() != typeof(AggregateExceptionHandler))
                .OrderBy(h => h.Order)
                .ToList();
            var fallback = _serviceProvider.GetRequiredService<IFallbackApiExceptionHandler>();
            return (handlers, fallback);
        });
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

        var (resolvedHandlers, resolvedFallback) = _resolved.Value;

        // Single inner exception: unwrap and re-dispatch to the handler chain
        if (innerExceptions.Count == 1)
        {
            var innerException = innerExceptions[0];

            // Find a specific handler for the inner exception (already filtered and sorted)
            var handler = resolvedHandlers.FirstOrDefault(h => h.CanHandle(innerException));

            return handler != null
                ? handler.Handle(innerException)
                : resolvedFallback.Handle(innerException);
        }

        // Multi-exception or zero-exception: delegate to fallback
        return resolvedFallback.Handle(aggregateException);
    }
}
