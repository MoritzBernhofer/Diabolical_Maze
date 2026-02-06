namespace AppServices;

public record ParsedMaze(string content);

public interface IMazeParser
{
    ParsedMaze ParseMaze(string content);
}

public enum MazeParseError
{
    EmptyFile,
    InvalidSymbol,
    InvalidRowCount,
    InvalidColumnCount,
    NoPossibleSolution,
    PathToShort,  
    NotValidStartPoint,
    NotValidEndPoint
}

public class MazeParseException(MazeParseError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message)
        ? message
        : "Unknown maze parsing error.")
{
    private static readonly Dictionary<MazeParseError, string> ErrorMessages = new()
    {
        { MazeParseError.EmptyFile, "The maze file is empty." },

        { MazeParseError.InvalidSymbol,
            "The maze contains an invalid symbol. Allowed symbols are: '#', '.', 'S', 'E'." },

        { MazeParseError.InvalidRowCount,
            "The maze has an invalid number of rows (it must contain at least 1 row and match the expected size, if specified)." },

        { MazeParseError.InvalidColumnCount,
            "The maze has an invalid number of columns (rows must be non-empty and all rows must have the same length)." },

        { MazeParseError.NoPossibleSolution,
            "No valid path exists from the start point to the end point." },

        { MazeParseError.PathToShort,
            "The computed path is too short (it does not meet the minimum required length)." },

        { MazeParseError.NotValidStartPoint,
            "The maze start point is invalid (missing, duplicated, or placed on a blocked cell)." },

        { MazeParseError.NotValidEndPoint,
            "The maze end point is invalid (missing, duplicated, or placed on a blocked cell)." },
    };

    public MazeParseError ErrorCode { get; } = errorCode;
}


public class MazeParser : IMazeParser
{
    public ParsedMaze ParseMaze(string content)
    {
        throw new NotImplementedException();
    }
}
