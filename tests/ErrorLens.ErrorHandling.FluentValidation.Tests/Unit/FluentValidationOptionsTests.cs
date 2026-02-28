using ErrorLens.ErrorHandling.FluentValidation;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace ErrorLens.ErrorHandling.FluentValidation.Tests.Unit;

public class FluentValidationOptionsTests
{
    [Fact]
    public void Default_IncludesSeverityErrorOnly()
    {
        var options = new FluentValidationOptions();
        options.IncludeSeverities.Should().ContainSingle()
            .Which.Should().Be(Severity.Error);
    }

    [Fact]
    public void AddWarning_IncludesErrorAndWarning()
    {
        var options = new FluentValidationOptions();
        options.IncludeSeverities.Add(Severity.Warning);

        options.IncludeSeverities.Should().HaveCount(2);
        options.IncludeSeverities.Should().Contain(Severity.Error);
        options.IncludeSeverities.Should().Contain(Severity.Warning);
    }

    [Fact]
    public void AddAllSeverities_IncludesAll()
    {
        var options = new FluentValidationOptions();
        options.IncludeSeverities.Add(Severity.Warning);
        options.IncludeSeverities.Add(Severity.Info);

        options.IncludeSeverities.Should().HaveCount(3);
        options.IncludeSeverities.Should().Contain(Severity.Error);
        options.IncludeSeverities.Should().Contain(Severity.Warning);
        options.IncludeSeverities.Should().Contain(Severity.Info);
    }

    [Fact]
    public void EmptySet_ExcludesAll()
    {
        var options = new FluentValidationOptions();
        options.IncludeSeverities.Clear();

        options.IncludeSeverities.Should().BeEmpty();
    }
}
