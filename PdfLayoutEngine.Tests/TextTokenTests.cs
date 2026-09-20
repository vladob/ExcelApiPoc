using PdfLayoutEngine.Models;

namespace PdfLayoutEngine.Tests;

public sealed class TextTokenTests
{
    [Fact]
    public void Token_normalizes_nbsp_but_preserves_original_text()
    {
        var token = new PdfTextToken(1, "A\u00a0B", 1.25, 4.75, 9.5, false, false);

        Assert.Equal("A B", token.Text);
        Assert.Equal("A\u00a0B", token.OriginalText);
        Assert.Equal(1.25, token.Left);
        Assert.Equal(9.5, token.Baseline);
    }
}
