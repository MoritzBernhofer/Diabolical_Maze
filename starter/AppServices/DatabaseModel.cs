namespace AppServices;

public class MazeEntity
{
    public int Id { get; set; }
    public required int Difficulty { get; set; }
    public required string MazeData { get; set; }
    public required string SolutionSteps { get; set; }
}