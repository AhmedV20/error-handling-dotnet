using System.Net;
using ErrorLens.ErrorHandling.Attributes;
using ErrorLens.ErrorHandling.Internal;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Internal;

public class ExceptionMetadataCacheTests
{
    [Fact]
    public void GetMetadata_PlainException_ReturnsEmptyMetadata()
    {
        var metadata = ExceptionMetadataCache.GetMetadata(typeof(Exception));

        metadata.ErrorCode.Should().BeNull();
        metadata.StatusCode.Should().BeNull();
        metadata.Properties.Should().BeEmpty();
    }

    [Fact]
    public void GetMetadata_WithResponseErrorCode_ReturnsCode()
    {
        var metadata = ExceptionMetadataCache.GetMetadata(typeof(TestCodeException));

        metadata.ErrorCode.Should().Be("CUSTOM_CODE");
    }

    [Fact]
    public void GetMetadata_WithResponseStatus_ReturnsStatus()
    {
        var metadata = ExceptionMetadataCache.GetMetadata(typeof(TestStatusException));

        metadata.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void GetMetadata_WithResponseErrorProperty_ReturnsProperties()
    {
        var metadata = ExceptionMetadataCache.GetMetadata(typeof(TestPropertyException));

        metadata.Properties.Should().HaveCount(1);
        metadata.Properties[0].Name.Should().Be("userId");
    }

    [Fact]
    public void GetMetadata_PropertyNameDefaultsToCamelCase()
    {
        var metadata = ExceptionMetadataCache.GetMetadata(typeof(TestCamelCasePropertyException));

        metadata.Properties.Should().HaveCount(1);
        // Property "OrderId" should default to "orderId" via ToCamelCase
        metadata.Properties[0].Name.Should().Be("orderId");
    }

    [Fact]
    public void GetMetadata_CachesBetweenCalls_ReturnsSameInstance()
    {
        var first = ExceptionMetadataCache.GetMetadata(typeof(TestCodeException));
        var second = ExceptionMetadataCache.GetMetadata(typeof(TestCodeException));

        first.Should().BeSameAs(second);
    }

    // Test exception classes

    [ResponseErrorCode("CUSTOM_CODE")]
    private class TestCodeException : Exception { }

    [ResponseStatus(HttpStatusCode.NotFound)]
    private class TestStatusException : Exception { }

    private class TestPropertyException : Exception
    {
        [ResponseErrorProperty("userId")]
        public string UserId { get; set; } = "123";
    }

    private class TestCamelCasePropertyException : Exception
    {
        [ResponseErrorProperty]
        public string OrderId { get; set; } = "456";
    }
}
