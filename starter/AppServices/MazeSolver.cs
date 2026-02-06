namespace AppServices;

public interface IMazeSolver
{
    (string, int) SolveMaze(ParsedMaze travel);
}

public class MazeSolver : IMazeSolver
{
    public (string, int) SolveMaze(ParsedMaze parsedMaze)
    {
        throw new NotImplementedException();
    }
}