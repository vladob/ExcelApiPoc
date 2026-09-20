using PdfLayoutEngine.Grouping;
using PdfLayoutEngine.Models;

namespace PdfLayoutEngine.Tests;

public sealed class BaselineGroupBuilderTests
{
    [Fact]
    public void Groups_tokens_within_tolerance_and_orders_them_left_to_right()
    {
        var right = Token(1, "Right", 80, 100, 99.25);
        var left = Token(1, "Left", 10, 30, 100);

        var groups = new BaselineGroupBuilder().Build(new[] { right, left });

        var group = Assert.Single(groups);
        Assert.Equal(new[] { left, right }, group.Tokens);
        Assert.Equal("Left Right", group.Text);
        Assert.Equal(10, group.Left);
        Assert.Equal(100, group.Right);
        Assert.Equal(99.625, group.Baseline);
    }

    [Fact]
    public void Starts_new_group_when_baseline_span_exceeds_tolerance()
    {
        var tokens = new[]
        {
            Token(1, "A", 10, 20, 100),
            Token(1, "B", 30, 40, 96),
            Token(1, "C", 50, 60, 95.99)
        };

        var groups = new BaselineGroupBuilder().Build(tokens, 4);

        Assert.Equal(2, groups.Count);
        Assert.Equal("A B", groups[0].Text);
        Assert.Equal("C", groups[1].Text);
    }

    [Fact]
    public void Never_groups_tokens_from_different_pages()
    {
        var groups = new BaselineGroupBuilder().Build(new[]
        {
            Token(1, "Page one", 10, 20, 100),
            Token(2, "Page two", 10, 20, 100)
        });

        Assert.Equal(2, groups.Count);
        Assert.Equal(1, groups[0].PageNumber);
        Assert.Equal(2, groups[1].PageNumber);
    }

    [Fact]
    public void Composes_normalized_text_without_changing_source_tokens()
    {
        var first = Token(1, " A\u00a0B ", 10, 20, 100);
        var second = Token(1, " C ", 30, 40, 100);

        var group = Assert.Single(new BaselineGroupBuilder().Build(new[] { second, first }));

        Assert.Equal("A B C", group.Text);
        Assert.Equal(" A\u00a0B ", first.OriginalText);
        Assert.Equal(" A B ", first.Text);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Rejects_invalid_tolerance(double tolerance)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BaselineGroupBuilder().Build(Array.Empty<PdfTextToken>(), tolerance));
    }

    private static PdfTextToken Token(
        int page,
        string text,
        double left,
        double right,
        double baseline) =>
        new PdfTextToken(page, text, left, right, baseline, false, false);
}
