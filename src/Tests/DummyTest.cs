namespace Tests;

public class DummyTests
{
    [Fact]
    public void OnePlusOne_ShouldEqualTwo()
    {
        // Arrange
        var a = 1;
        var b = 1;

        // Act
        var result = a + b;

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void True_ShouldBeTrue()
    {
        // Arrange & Act & Assert
        Assert.True(true);
    }

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 3, 5)]
    [InlineData(10, -5, 5)]
    public void AddNumbers_ShouldReturnCorrectSum(int a, int b, int expected)
    {
        // Act
        var result = a + b;

        // Assert
        Assert.Equal(expected, result);
    }
}
