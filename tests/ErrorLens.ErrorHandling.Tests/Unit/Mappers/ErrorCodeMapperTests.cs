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

    // --- Acronym regex tests (T048) ---

    [Fact]
    public void GetErrorCode_AcronymHTTP_HandledCorrectly()
    {
        var code = _mapper.GetErrorCode(new HttpConnectionException());

        code.Should().Be("HTTP_CONNECTION");
    }

    [Fact]
    public void GetErrorCode_AcronymIO_HandledCorrectly()
    {
        var code = _mapper.GetErrorCode(new IOErrorException());

        code.Should().Be("IO_ERROR");
    }

    [Fact]
    public void GetErrorCode_TrailingDigits_PreservedCorrectly()
    {
        var code = _mapper.GetErrorCode(new Error404Exception());

        code.Should().Be("ERROR404");
    }

    private class HttpConnectionException : Exception { }
    private class IOErrorException : Exception { }
    private class Error404Exception : Exception { }

    // --- US4: KebabCase strategy tests ---

    [Fact]
    public void GetErrorCode_KebabCase_UserNotFoundException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase;

        var code = _mapper.GetErrorCode(new UserNotFoundException());

        code.Should().Be("user-not-found");
    }

    [Fact]
    public void GetErrorCode_KebabCase_HTTPException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase;

        var code = _mapper.GetErrorCode(new HttpConnectionException());

        code.Should().Be("http-connection");
    }

    [Fact]
    public void GetErrorCode_KebabCase_IOException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase;

        var code = _mapper.GetErrorCode(new IOErrorException());

        code.Should().Be("io-error");
    }

    [Fact]
    public void GetErrorCode_KebabCase_BaseException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase;

        var code = _mapper.GetErrorCode(new Exception("test"));

        code.Should().Be("internal-error");
    }

    [Fact]
    public void GetErrorCode_KebabCase_SingleWordException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase;

        var code = _mapper.GetErrorCode(new ArgumentException("test"));

        code.Should().Be("argument");
    }

    [Fact]
    public void GetErrorCode_KebabCase_WithNumbers()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.KebabCase;

        var code = _mapper.GetErrorCode(new Error404Exception());

        code.Should().Be("error404");
    }

    // --- US4: PascalCase strategy tests ---

    [Fact]
    public void GetErrorCode_PascalCase_UserNotFoundException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.PascalCase;

        var code = _mapper.GetErrorCode(new UserNotFoundException());

        code.Should().Be("UserNotFound");
    }

    [Fact]
    public void GetErrorCode_PascalCase_HTTPException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.PascalCase;

        var code = _mapper.GetErrorCode(new HttpConnectionException());

        code.Should().Be("HttpConnection");
    }

    [Fact]
    public void GetErrorCode_PascalCase_IOException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.PascalCase;

        var code = _mapper.GetErrorCode(new IOErrorException());

        code.Should().Be("IOError");
    }

    [Fact]
    public void GetErrorCode_PascalCase_BaseException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.PascalCase;

        var code = _mapper.GetErrorCode(new Exception("test"));

        code.Should().Be("InternalError");
    }

    [Fact]
    public void GetErrorCode_PascalCase_SingleWordException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.PascalCase;

        var code = _mapper.GetErrorCode(new ArgumentException("test"));

        code.Should().Be("Argument");
    }

    // --- US4: DotSeparated strategy tests ---

    [Fact]
    public void GetErrorCode_DotSeparated_UserNotFoundException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.DotSeparated;

        var code = _mapper.GetErrorCode(new UserNotFoundException());

        code.Should().Be("user.not.found");
    }

    [Fact]
    public void GetErrorCode_DotSeparated_HTTPException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.DotSeparated;

        var code = _mapper.GetErrorCode(new HttpConnectionException());

        code.Should().Be("http.connection");
    }

    [Fact]
    public void GetErrorCode_DotSeparated_IOException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.DotSeparated;

        var code = _mapper.GetErrorCode(new IOErrorException());

        code.Should().Be("io.error");
    }

    [Fact]
    public void GetErrorCode_DotSeparated_BaseException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.DotSeparated;

        var code = _mapper.GetErrorCode(new Exception("test"));

        code.Should().Be("internal.error");
    }

    [Fact]
    public void GetErrorCode_DotSeparated_SingleWordException()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.DotSeparated;

        var code = _mapper.GetErrorCode(new ArgumentException("test"));

        code.Should().Be("argument");
    }

    [Fact]
    public void GetErrorCode_DotSeparated_WithNumbers()
    {
        _options.DefaultErrorCodeStrategy = ErrorCodeStrategy.DotSeparated;

        var code = _mapper.GetErrorCode(new Error404Exception());

        code.Should().Be("error404");
    }

    private class UserNotFoundException : Exception { }
}
