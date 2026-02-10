using System.Text.Json;
using ErrorLens.ErrorHandling.Models;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Models;

public class ApiGlobalErrorTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var error = new ApiGlobalError("PASSWORDS_MUST_MATCH", "Password and confirmation must match");

        error.Code.Should().Be("PASSWORDS_MUST_MATCH");
        error.Message.Should().Be("Password and confirmation must match");
    }

    [Fact]
    public void Serialization_ProducesCorrectJson()
    {
        var error = new ApiGlobalError("CROSS_FIELD_ERROR", "Fields are inconsistent");

        var json = JsonSerializer.Serialize(error);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("code").GetString().Should().Be("CROSS_FIELD_ERROR");
        doc.RootElement.GetProperty("message").GetString().Should().Be("Fields are inconsistent");
    }

    [Fact]
    public void Serialization_OnlyContainsTwoProperties()
    {
        var error = new ApiGlobalError("ERROR", "Message");

        var json = JsonSerializer.Serialize(error);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.EnumerateObject().Count().Should().Be(2);
    }
}
