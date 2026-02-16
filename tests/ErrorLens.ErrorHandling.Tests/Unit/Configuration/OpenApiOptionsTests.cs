using ErrorLens.ErrorHandling.OpenApi;
using FluentAssertions;
using Xunit;

namespace ErrorLens.ErrorHandling.Tests.Unit.Configuration;

public class OpenApiOptionsTests
{
    [Fact]
    public void DefaultStatusCodes_ContainsExpectedDefaults()
    {
        var options = new OpenApiOptions();

        options.DefaultStatusCodes.Should().BeEquivalentTo(new[] { 400, 404, 500 });
    }

    [Fact]
    public void DefaultStatusCodes_IsNotNull()
    {
        var options = new OpenApiOptions();

        options.DefaultStatusCodes.Should().NotBeNull();
    }

    [Fact]
    public void DefaultStatusCodes_CanBeModified()
    {
        var options = new OpenApiOptions();

        options.DefaultStatusCodes.Add(422);
        options.DefaultStatusCodes.Remove(404);

        options.DefaultStatusCodes.Should().Contain(422);
        options.DefaultStatusCodes.Should().NotContain(404);
    }

    [Fact]
    public void DefaultStatusCodes_CanBeReplaced()
    {
        var options = new OpenApiOptions
        {
            DefaultStatusCodes = new HashSet<int> { 400, 500 }
        };

        options.DefaultStatusCodes.Should().BeEquivalentTo(new[] { 400, 500 });
    }

    [Fact]
    public void DefaultStatusCodes_NoDuplicatesInDefault()
    {
        var options = new OpenApiOptions();

        options.DefaultStatusCodes.Should().OnlyHaveUniqueItems();
    }
}
