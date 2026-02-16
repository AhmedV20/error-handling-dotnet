using ErrorLens.ErrorHandling.Handlers;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Handlers;

public class ModelStateValidationExceptionTests
{
    [Fact]
    public void Constructor_WithNullModelState_ThrowsArgumentNullException()
    {
        var act = () => new ModelStateValidationException(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("modelState");
    }

    [Fact]
    public void Constructor_WithValidModelState_SetsProperties()
    {
        var modelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        modelState.AddModelError("email", "Email is required");

        var exception = new ModelStateValidationException(modelState);

        exception.ModelState.Should().BeSameAs(modelState);
        exception.Message.Should().Be("Validation failed");
    }
}
