// MazeParserErrorTests.cs
namespace AppServicesTests;

using AppServices;
using Xunit;

public class MazeParserErrorTests
{
    private readonly IMazeParser _parser = new MazeParser();

    [Fact]
    public void ParseMaze_ValidMaze_ReturnsParsedMaze()
    {
        const string text = "SXO\nOOO\nXOE";

        var parsed = _parser.ParseMaze(text);

        Assert.Equal(text, parsed.Content);
    }

    [Fact]
    public void ParseMaze_EmptyFile_ThrowsEmptyFile()
    {
        var ex = Assert.Throws<MazeParseException>(() => _parser.ParseMaze(""));
        Assert.Equal(MazeParseError.EmptyFile, ex.ErrorCode);
    }

    [Fact]
    public void ParseMaze_InvalidSymbol_ThrowsInvalidSymbol()
    {
        const string text = "SXO\nO?O\nXOE"; // '?' is invalid

        var ex = Assert.Throws<MazeParseException>(() => _parser.ParseMaze(text));
        Assert.Equal(MazeParseError.InvalidSymbol, ex.ErrorCode);
    }

    [Fact]
    public void ParseMaze_InvalidRowCount_ThrowsInvalidRowCount()
    {
        // An empty row (double newline) is a clean way to represent "invalid row count".
        // If your parser chooses to classify this as InvalidColumnCount instead, swap the expected error code.
        const string text = "SXO\n\nXOE";

        var ex = Assert.Throws<MazeParseException>(() => _parser.ParseMaze(text));
        Assert.Equal(MazeParseError.InvalidRowCount, ex.ErrorCode);
    }

    [Fact]
    public void ParseMaze_InvalidColumnCount_ThrowsInvalidColumnCount()
    {
        // Row lengths differ => invalid column count
        const string text = "SXO\nOOOO\nXOE";

        var ex = Assert.Throws<MazeParseException>(() => _parser.ParseMaze(text));
        Assert.Equal(MazeParseError.InvalidColumnCount, ex.ErrorCode);
    }

    [Fact]
    public void ParseMaze_NotValidStartPoint_ThrowsNotValidStartPoint_WhenMissingStart()
    {
        const string text = "XXO\nOOO\nXOE"; // no 'S'

        var ex = Assert.Throws<MazeParseException>(() => _parser.ParseMaze(text));
        Assert.Equal(MazeParseError.NotValidStartPoint, ex.ErrorCode);
    }

    [Fact]
    public void ParseMaze_NotValidStartPoint_ThrowsNotValidStartPoint_WhenDuplicateStart()
    {
        const string text = "SXO\nSOO\nXOE"; // two 'S'

        var ex = Assert.Throws<MazeParseException>(() => _parser.ParseMaze(text));
        Assert.Equal(MazeParseError.NotValidStartPoint, ex.ErrorCode);
    }

    [Fact]
    public void ParseMaze_NotValidEndPoint_ThrowsNotValidEndPoint_WhenMissingEnd()
    {
        const string text = "SXO\nOOO\nXXX"; // no 'E'

        var ex = Assert.Throws<MazeParseException>(() => _parser.ParseMaze(text));
        Assert.Equal(MazeParseError.NotValidEndPoint, ex.ErrorCode);
    }

    [Fact]
    public void ParseMaze_NotValidEndPoint_ThrowsNotValidEndPoint_WhenDuplicateEnd()
    {
        const string text = "SXE\nOOE\nXXX"; // two 'E'

        var ex = Assert.Throws<MazeParseException>(() => _parser.ParseMaze(text));
        Assert.Equal(MazeParseError.NotValidEndPoint, ex.ErrorCode);
    }

    [Fact]
    public void ParseMaze_NoPossibleSolution_ThrowsNoPossibleSolution()
    {
        // S and E are separated by a full wall row
        const string text =
            "SOO\n" +
            "XXX\n" +
            "OOE";

        var ex = Assert.Throws<MazeParseException>(() => _parser.ParseMaze(text));
        Assert.Equal(MazeParseError.NoPossibleSolution, ex.ErrorCode);
    }
    
}
