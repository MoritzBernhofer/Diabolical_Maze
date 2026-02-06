namespace AppServices;

public interface IMazeSolver
{
    (string, int) SolveMaze(ParsedMaze travel);
}

public record MazeResult(
    decimal Mileage,
    decimal PerDiem,
    decimal Expenses
);

public class MazeSolver : IMazeSolver
{
    public (string, int) SolveMaze(ParsedMaze parsedMaze)
    {
        throw new NotImplementedException();
    }
}