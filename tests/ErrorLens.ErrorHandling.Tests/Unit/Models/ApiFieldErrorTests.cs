using System.Text.Json;
using ErrorLens.ErrorHandling.Models;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Models;

public class ApiFieldErrorTests
{
    [Fact]
    public void Constructor_WithRequiredParameters_SetsProperties()
    {
        var error = new ApiFieldError("REQUIRED_NOT_NULL", "email", "Email is required");

        error.Code.Should().Be("REQUIRED_NOT_NULL");
        error.Property.Should().Be("email");
        error.Message.Should().Be("Email is required");
        error.RejectedValue.Should().BeNull();
        error.Path.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        var error = new ApiFieldError("INVALID_SIZE", "password", "Password too short", "123", "user.password");

        error.Code.Should().Be("INVALID_SIZE");
        error.Property.Should().Be("password");
        error.Message.Should().Be("Password too short");
        error.RejectedValue.Should().Be("123");
        error.Path.Should().Be("user.password");
    }

    [Fact]
    public void Serialization_ProducesCorrectJson()
    {
        var error = new ApiFieldError("REQUIRED_NOT_NULL", "email", "Email is required");

        var json = JsonSerializer.Serialize(error);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("code").GetString().Should().Be("REQUIRED_NOT_NULL");
        doc.RootElement.GetProperty("property").GetString().Should().Be("email");
        doc.RootElement.GetProperty("message").GetString().Should().Be("Email is required");
    }

    [Fact]
    public void Serialization_OmitsNullRejectedValue()
    {
        var error = new ApiFieldError("REQUIRED", "email", "Email is required");

        var json = JsonSerializer.Serialize(error);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("rejectedValue", out _).Should().BeFalse();
    }

    [Fact]
    public void Serialization_IncludesRejectedValueWhenPresent()
    {
        var error = new ApiFieldError("INVALID_EMAIL", "email", "Invalid email format", "not-an-email", "email");

        var json = JsonSerializer.Serialize(error);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("rejectedValue").GetString().Should().Be("not-an-email");
        doc.RootElement.GetProperty("path").GetString().Should().Be("email");
    }

    [Fact]
    public void Serialization_OmitsNullPath()
    {
        var error = new ApiFieldError("REQUIRED", "email", "Email is required");

        var json = JsonSerializer.Serialize(error);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("path", out _).Should().BeFalse();
    }

    // --- Null guard tests (T029) ---

    [Fact]
    public void Constructor_WithNullCode_ThrowsArgumentNullException()
    {
        var act = () => new ApiFieldError(null!, "email", "Email is required");

        act.Should().Throw<ArgumentNullException>().WithParameterName("code");
    }

    [Fact]
    public void Constructor_WithNullProperty_ThrowsArgumentNullException()
    {
        var act = () => new ApiFieldError("REQUIRED", null!, "Email is required");

        act.Should().Throw<ArgumentNullException>().WithParameterName("property");
    }

    [Fact]
    public void Constructor_Full_WithNullCode_ThrowsArgumentNullException()
    {
        var act = () => new ApiFieldError(null!, "email", "Email is required", "bad@", "email");

        act.Should().Throw<ArgumentNullException>().WithParameterName("code");
    }

    [Fact]
    public void Constructor_Full_WithNullProperty_ThrowsArgumentNullException()
    {
        var act = () => new ApiFieldError("REQUIRED", null!, "Email is required", "bad@", "email");

        act.Should().Throw<ArgumentNullException>().WithParameterName("property");
    }
}
