using Wex.Domain;
using Wex.Domain.Exceptions;
using Xunit;

namespace Wex.Tests.Domain;

public sealed class GuardTests
{
    [Theory]
    [InlineData("valid")]
    [InlineData("  valid  ")]
    public void NotNullOrWhiteSpace_WithValidString_ReturnsString(string value)
    {
        var result = Guard.NotNullOrWhiteSpace(value, "TestParam");
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotNullOrWhiteSpace_WithInvalidString_ThrowsDomainException(string? value)
    {
        var ex = Assert.Throws<DomainException>(() => Guard.NotNullOrWhiteSpace(value, "TestParam"));
        Assert.Equal("TestParam is required.", ex.Message);
    }

    [Fact]
    public void MaxLength_WithinLimit_ReturnsValue()
    {
        var result = Guard.MaxLength("hello", 10, "TestParam");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void MaxLength_ExactlyAtLimit_ReturnsValue()
    {
        var result = Guard.MaxLength("hello", 5, "TestParam");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void MaxLength_ExceedsLimit_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => Guard.MaxLength("hello world", 5, "TestParam"));
        Assert.Equal("TestParam must not exceed 5 characters.", ex.Message);
    }
}
