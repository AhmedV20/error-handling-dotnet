using System.Net;
using ErrorLens.ErrorHandling.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Integration;

/// <summary>
/// Integration tests verifying YAML configuration binding for error handling options.
/// </summary>
public class YamlConfigurationTests
{
    private static IConfiguration BuildYamlConfiguration(string yamlContent)
    {
        var tempFile = Path.GetTempFileName();
        var yamlFile = Path.ChangeExtension(tempFile, ".yml");
        File.Move(tempFile, yamlFile);
        File.WriteAllText(yamlFile, yamlContent);

        try
        {
            var builder = new ConfigurationBuilder()
                .AddYamlFile(yamlFile, optional: false, reloadOnChange: false);

            return builder.Build();
        }
        finally
        {
            // Clean up in background — file may still be locked briefly
            _ = Task.Run(async () =>
            {
                await Task.Delay(100);
                try { File.Delete(yamlFile); } catch { /* ignore */ }
            });
        }
    }

    [Fact]
    public void YamlConfig_BindsBasicOptions()
    {
        var yaml = @"
ErrorHandling:
  Enabled: false
  HttpStatusInJsonResponse: true
  SearchSuperClassHierarchy: true
  AddPathToError: false
";
        var config = BuildYamlConfiguration(yaml);
        var options = new ErrorHandlingOptions();
        config.GetSection(ErrorHandlingOptions.SectionName).Bind(options);

        options.Enabled.Should().BeFalse();
        options.HttpStatusInJsonResponse.Should().BeTrue();
        options.SearchSuperClassHierarchy.Should().BeTrue();
        options.AddPathToError.Should().BeFalse();
    }

    [Fact]
    public void YamlConfig_BindsEnumValues()
    {
        var yaml = @"
ErrorHandling:
  DefaultErrorCodeStrategy: FullQualifiedName
  ExceptionLogging: WithStacktrace
";
        var config = BuildYamlConfiguration(yaml);
        var options = new ErrorHandlingOptions();
        config.GetSection(ErrorHandlingOptions.SectionName).Bind(options);

        options.DefaultErrorCodeStrategy.Should().Be(ErrorCodeStrategy.FullQualifiedName);
        options.ExceptionLogging.Should().Be(ExceptionLogging.WithStacktrace);
    }

    [Fact]
    public void YamlConfig_BindsHttpStatusDictionary()
    {
        var yaml = @"
ErrorHandling:
  HttpStatuses:
    System.UnauthorizedAccessException: 401
    System.Collections.Generic.KeyNotFoundException: 404
    System.InvalidOperationException: 409
";
        var config = BuildYamlConfiguration(yaml);
        var options = new ErrorHandlingOptions();
        config.GetSection(ErrorHandlingOptions.SectionName).Bind(options);

        options.HttpStatuses.Should().ContainKey("System.UnauthorizedAccessException")
            .WhoseValue.Should().Be(HttpStatusCode.Unauthorized);
        options.HttpStatuses.Should().ContainKey("System.Collections.Generic.KeyNotFoundException")
            .WhoseValue.Should().Be(HttpStatusCode.NotFound);
        options.HttpStatuses.Should().ContainKey("System.InvalidOperationException")
            .WhoseValue.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public void YamlConfig_BindsCodesDictionary()
    {
        var yaml = @"
ErrorHandling:
  Codes:
    System.ArgumentException: INVALID_ARGUMENT
    email.Required: EMAIL_IS_REQUIRED
    email.EmailAddress: EMAIL_FORMAT_INVALID
";
        var config = BuildYamlConfiguration(yaml);
        var options = new ErrorHandlingOptions();
        config.GetSection(ErrorHandlingOptions.SectionName).Bind(options);

        options.Codes.Should().ContainKey("System.ArgumentException")
            .WhoseValue.Should().Be("INVALID_ARGUMENT");
        options.Codes.Should().ContainKey("email.Required")
            .WhoseValue.Should().Be("EMAIL_IS_REQUIRED");
        options.Codes.Should().ContainKey("email.EmailAddress")
            .WhoseValue.Should().Be("EMAIL_FORMAT_INVALID");
    }

    [Fact]
    public void YamlConfig_BindsMessagesDictionary()
    {
        var yaml = @"
ErrorHandling:
  Messages:
    System.ArgumentException: The argument provided was invalid
    email.Required: A valid email address is required
";
        var config = BuildYamlConfiguration(yaml);
        var options = new ErrorHandlingOptions();
        config.GetSection(ErrorHandlingOptions.SectionName).Bind(options);

        options.Messages.Should().ContainKey("System.ArgumentException")
            .WhoseValue.Should().Be("The argument provided was invalid");
        options.Messages.Should().ContainKey("email.Required")
            .WhoseValue.Should().Be("A valid email address is required");
    }

    [Fact]
    public void YamlConfig_BindsLogLevelsDictionary()
    {
        var yaml = @"
ErrorHandling:
  LogLevels:
    4xx: Warning
    5xx: Error
    404: Debug
";
        var config = BuildYamlConfiguration(yaml);
        var options = new ErrorHandlingOptions();
        config.GetSection(ErrorHandlingOptions.SectionName).Bind(options);

        options.LogLevels.Should().ContainKey("4xx")
            .WhoseValue.Should().Be(LogLevel.Warning);
        options.LogLevels.Should().ContainKey("5xx")
            .WhoseValue.Should().Be(LogLevel.Error);
        options.LogLevels.Should().ContainKey("404")
            .WhoseValue.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void YamlConfig_BindsFullStacktraceHttpStatuses()
    {
        var yaml = @"
ErrorHandling:
  FullStacktraceHttpStatuses:
    - 5xx
    - 400
";
        var config = BuildYamlConfiguration(yaml);
        var options = new ErrorHandlingOptions();
        config.GetSection(ErrorHandlingOptions.SectionName).Bind(options);

        options.FullStacktraceHttpStatuses.Should().Contain("5xx");
        options.FullStacktraceHttpStatuses.Should().Contain("400");
    }

    [Fact]
    public void YamlConfig_BindsJsonFieldNames()
    {
        var yaml = @"
ErrorHandling:
  JsonFieldNames:
    Code: type
    Message: detail
    Status: statusCode
    FieldErrors: fields
    GlobalErrors: errors
    ParameterErrors: params
    Property: field
    RejectedValue: value
    Path: jsonPath
    Parameter: paramName
";
        var config = BuildYamlConfiguration(yaml);
        var options = new ErrorHandlingOptions();
        config.GetSection(ErrorHandlingOptions.SectionName).Bind(options);

        options.JsonFieldNames.Code.Should().Be("type");
        options.JsonFieldNames.Message.Should().Be("detail");
        options.JsonFieldNames.Status.Should().Be("statusCode");
        options.JsonFieldNames.FieldErrors.Should().Be("fields");
        options.JsonFieldNames.GlobalErrors.Should().Be("errors");
        options.JsonFieldNames.ParameterErrors.Should().Be("params");
        options.JsonFieldNames.Property.Should().Be("field");
        options.JsonFieldNames.RejectedValue.Should().Be("value");
        options.JsonFieldNames.Path.Should().Be("jsonPath");
        options.JsonFieldNames.Parameter.Should().Be("paramName");
    }

    [Fact]
    public void YamlConfig_BindsProblemDetailOptions()
    {
        var yaml = @"
ErrorHandling:
  UseProblemDetailFormat: true
  ProblemDetailTypePrefix: https://api.myapp.com/errors/
  ProblemDetailConvertToKebabCase: false
";
        var config = BuildYamlConfiguration(yaml);
        var options = new ErrorHandlingOptions();
        config.GetSection(ErrorHandlingOptions.SectionName).Bind(options);

        options.UseProblemDetailFormat.Should().BeTrue();
        options.ProblemDetailTypePrefix.Should().Be("https://api.myapp.com/errors/");
        options.ProblemDetailConvertToKebabCase.Should().BeFalse();
    }

    [Fact]
    public void YamlConfig_FullConfiguration_AllOptionsBindCorrectly()
    {
        var yaml = @"
ErrorHandling:
  Enabled: true
  HttpStatusInJsonResponse: true
  DefaultErrorCodeStrategy: AllCaps
  SearchSuperClassHierarchy: true
  ExceptionLogging: WithStacktrace
  UseProblemDetailFormat: false

  JsonFieldNames:
    Code: type
    Message: detail

  FullStacktraceHttpStatuses:
    - 5xx

  HttpStatuses:
    MyApp.UserNotFoundException: 404
    MyApp.DuplicateEmailException: 409

  Codes:
    MyApp.UserNotFoundException: USER_NOT_FOUND
    email.Required: EMAIL_IS_REQUIRED

  Messages:
    MyApp.UserNotFoundException: The requested user was not found

  LogLevels:
    4xx: Warning
    5xx: Error
";
        var config = BuildYamlConfiguration(yaml);
        var options = new ErrorHandlingOptions();
        config.GetSection(ErrorHandlingOptions.SectionName).Bind(options);

        // Boolean options
        options.Enabled.Should().BeTrue();
        options.HttpStatusInJsonResponse.Should().BeTrue();
        options.SearchSuperClassHierarchy.Should().BeTrue();
        options.UseProblemDetailFormat.Should().BeFalse();

        // Enum
        options.DefaultErrorCodeStrategy.Should().Be(ErrorCodeStrategy.AllCaps);
        options.ExceptionLogging.Should().Be(ExceptionLogging.WithStacktrace);

        // JsonFieldNames
        options.JsonFieldNames.Code.Should().Be("type");
        options.JsonFieldNames.Message.Should().Be("detail");

        // Dictionaries
        options.HttpStatuses.Should().HaveCount(2);
        options.Codes.Should().HaveCount(2);
        options.Messages.Should().HaveCount(1);
        options.LogLevels.Should().HaveCount(2);

        // Sets
        options.FullStacktraceHttpStatuses.Should().Contain("5xx");
    }
}
