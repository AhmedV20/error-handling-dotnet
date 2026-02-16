using System.Text.Json;
using ErrorLens.ErrorHandling.Models;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Models;

public class ApiParameterErrorTests
{
    [Fact]
    public void Constructor_WithRequiredParameters_SetsProperties()
    {
        var error = new ApiParameterError("REQUIRED_NOT_NULL", "id", "Id is required");

        error.Code.Should().Be("REQUIRED_NOT_NULL");
        error.Parameter.Should().Be("id");
        error.Message.Should().Be("Id is required");
        error.RejectedValue.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        var error = new ApiParameterError("INVALID_FORMAT", "count", "Count must be a number", "abc");

        error.Code.Should().Be("INVALID_FORMAT");
        error.Parameter.Should().Be("count");
        error.Message.Should().Be("Count must be a number");
        error.RejectedValue.Should().Be("abc");
    }

    [Fact]
    public void Serialization_ProducesCorrectJson()
    {
        var error = new ApiParameterError("REQUIRED", "userId", "User ID is required");

        var json = JsonSerializer.Serialize(error);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("code").GetString().Should().Be("REQUIRED");
        doc.RootElement.GetProperty("parameter").GetString().Should().Be("userId");
        doc.RootElement.GetProperty("message").GetString().Should().Be("User ID is required");
    }

    [Fact]
    public void Serialization_OmitsNullRejectedValue()
    {
        var error = new ApiParameterError("REQUIRED", "id", "Id is required");

        var json = JsonSerializer.Serialize(error);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("rejectedValue", out _).Should().BeFalse();
    }

    [Fact]
    public void Serialization_IncludesRejectedValueWhenPresent()
    {
        var error = new ApiParameterError("OUT_OF_RANGE", "page", "Page must be positive", -1);

        var json = JsonSerializer.Serialize(error);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("rejectedValue").GetInt32().Should().Be(-1);
    }

    // --- Null guard tests (T030) ---

    [Fact]
    public void Constructor_WithNullCode_ThrowsArgumentNullException()
    {
        var act = () => new ApiParameterError(null!, "id", "Id is required");

        act.Should().Throw<ArgumentNullException>().WithParameterName("code");
    }

    [Fact]
    public void Constructor_WithNullParameter_ThrowsArgumentNullException()
    {
        var act = () => new ApiParameterError("REQUIRED", null!, "Id is required");

        act.Should().Throw<ArgumentNullException>().WithParameterName("parameter");
    }

    [Fact]
    public void Constructor_Full_WithNullCode_ThrowsArgumentNullException()
    {
        var act = () => new ApiParameterError(null!, "id", "Id is required", "abc");

        act.Should().Throw<ArgumentNullException>().WithParameterName("code");
    }

    [Fact]
    public void Constructor_Full_WithNullParameter_ThrowsArgumentNullException()
    {
        var act = () => new ApiParameterError("REQUIRED", null!, "Id is required", "abc");

        act.Should().Throw<ArgumentNullException>().WithParameterName("parameter");
    }
}
