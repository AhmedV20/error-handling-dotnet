using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Mappers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Mappers;

public class ErrorCodeMapperTests
{
    private readonly ErrorHandlingOptions _options;
    private readonly ErrorCodeMapper _mapper;

    public ErrorCodeMapperTests()
    {
        _options = new ErrorHandlingOptions();
        var optionsWrapper = Substitute.For<IOptions<ErrorHandlingOptions>>();
        optionsWrapper.Value.Returns(_options);
        _mapper = new ErrorCodeMapper(optionsWrapper);
    }

    [Fact]
    public void GetErrorCode_ConvertsToPascalCase_RemovesExceptionSuffix()
    {
        var exception = new InvalidOperationException("test");

        var code = _mapper.GetErrorCode(exception);

        code.Should().Be("INVALID_OPERATION");
    }

    [Fact]
    public void GetErrorCode_ArgumentNullException_ReturnsArgumentNull()
    {
        var exception = new ArgumentNullException("param");

        var code = _mapper.GetErrorCode(exception);

        code.Should().Be("ARGUMENT_NULL");
    }

    [Fact]
    public void GetErrorCode_WithConfiguredOverride_ReturnsConfiguredCode()
    {
        _options.Codes["System.ArgumentException"] = "INVALID_ARGUMENT";
        var exception = new ArgumentException("test");

        var code = _mapper.GetErrorCode(exception);

        code.Should().Be("INVALID_ARGUMENT");
    }

    [Fact]
    public void GetErrorCode_WithFullQualifiedNameStrategy_ReturnsFullName()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.FullQualifiedName;
        var exception = new InvalidOperationException("test");

        var code = _mapper.GetErrorCode(exception);

        code.Should().Be("System.InvalidOperationException");
    }

    [Fact]
    public void GetErrorCode_WithSuperClassHierarchy_FindsBaseClassConfig()
    {
        _options.SearchSuperClassHierarchy = true;
        _options.Codes["System.SystemException"] = "SYSTEM_ERROR";
        var exception = new InvalidOperationException("test");

        var code = _mapper.GetErrorCode(exception);

        code.Should().Be("SYSTEM_ERROR");
    }

    [Fact]
    public void GetErrorCode_FieldSpecific_ReturnsFieldOverride()
    {
        _options.Codes["email.Required"] = "EMAIL_REQUIRED";

        var code = _mapper.GetErrorCode("email.Required", "REQUIRED_NOT_NULL");

        code.Should().Be("EMAIL_REQUIRED");
    }

    [Fact]
    public void GetErrorCode_FieldSpecific_FallsBackToDefault()
    {
        var code = _mapper.GetErrorCode("email.Required", "REQUIRED_NOT_NULL");

        code.Should().Be("REQUIRED_NOT_NULL");
    }
}
