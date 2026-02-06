namespace AppServices;

public class MazeEntity
{
    public int Id { get; set; }
    public required int Difficulty { get; set; } //1-10
    public required string MazeData {get; set;} //the maze itself, you can change it to a 2d array (but i recommend using a simple string)
    public required string SolutionSteps { get; set; } 
}
