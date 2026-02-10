using System.Text.Json;
using System.Text.Json.Serialization;
using ErrorLens.ErrorHandling.Configuration;
using ErrorLens.ErrorHandling.Models;

namespace ErrorLens.ErrorHandling.Serialization;

/// <summary>
/// Custom JSON converter for ApiErrorResponse that uses configurable field names.
/// </summary>
public class ApiErrorResponseConverter : JsonConverter<ApiErrorResponse>
{
    private readonly JsonFieldNamesOptions _fieldNames;

    public ApiErrorResponseConverter(JsonFieldNamesOptions fieldNames)
    {
        _fieldNames = fieldNames;
    }

    public override ApiErrorResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialization uses default behavior — only Write is customized
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var code = root.TryGetProperty(_fieldNames.Code, out var codeProp)
            ? codeProp.GetString() ?? ""
            : "";
        var message = root.TryGetProperty(_fieldNames.Message, out var msgProp)
            ? msgProp.GetString()
            : null;

        return new ApiErrorResponse(code, message);
    }

    public override void Write(Utf8JsonWriter writer, ApiErrorResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Code (always written)
        writer.WriteString(_fieldNames.Code, value.Code);

        // Message (omit if null)
        if (value.Message != null)
        {
            writer.WriteString(_fieldNames.Message, value.Message);
        }

        // Status (omit if 0 / default)
        if (value.Status != 0)
        {
            writer.WriteNumber(_fieldNames.Status, value.Status);
        }

        // Field errors (omit if null)
        if (value.FieldErrors is { Count: > 0 })
        {
            writer.WritePropertyName(_fieldNames.FieldErrors);
            WriteFieldErrors(writer, value.FieldErrors);
        }

        // Global errors (omit if null)
        if (value.GlobalErrors is { Count: > 0 })
        {
            writer.WritePropertyName(_fieldNames.GlobalErrors);
            WriteGlobalErrors(writer, value.GlobalErrors);
        }

        // Parameter errors (omit if null)
        if (value.ParameterErrors is { Count: > 0 })
        {
            writer.WritePropertyName(_fieldNames.ParameterErrors);
            WriteParameterErrors(writer, value.ParameterErrors);
        }

        // Extension data / custom properties
        if (value.Properties is { Count: > 0 })
        {
            foreach (var prop in value.Properties)
            {
                writer.WritePropertyName(prop.Key);
                JsonSerializer.Serialize(writer, prop.Value, options);
            }
        }

        writer.WriteEndObject();
    }

    private void WriteFieldErrors(Utf8JsonWriter writer, List<ApiFieldError> fieldErrors)
    {
        writer.WriteStartArray();
        foreach (var error in fieldErrors)
        {
            writer.WriteStartObject();
            writer.WriteString(_fieldNames.Code, error.Code);
            writer.WriteString(_fieldNames.Property, error.Property);
            writer.WriteString(_fieldNames.Message, error.Message);

            if (error.RejectedValue != null)
            {
                writer.WritePropertyName(_fieldNames.RejectedValue);
                JsonSerializer.Serialize(writer, error.RejectedValue);
            }

            if (error.Path != null)
            {
                writer.WriteString(_fieldNames.Path, error.Path);
            }

            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private void WriteGlobalErrors(Utf8JsonWriter writer, List<ApiGlobalError> globalErrors)
    {
        writer.WriteStartArray();
        foreach (var error in globalErrors)
        {
            writer.WriteStartObject();
            writer.WriteString(_fieldNames.Code, error.Code);
            writer.WriteString(_fieldNames.Message, error.Message);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private void WriteParameterErrors(Utf8JsonWriter writer, List<ApiParameterError> parameterErrors)
    {
        writer.WriteStartArray();
        foreach (var error in parameterErrors)
        {
            writer.WriteStartObject();
            writer.WriteString(_fieldNames.Code, error.Code);
            writer.WriteString(_fieldNames.Parameter, error.Parameter);
            writer.WriteString(_fieldNames.Message, error.Message);

            if (error.RejectedValue != null)
            {
                writer.WritePropertyName(_fieldNames.RejectedValue);
                JsonSerializer.Serialize(writer, error.RejectedValue);
            }

            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}
