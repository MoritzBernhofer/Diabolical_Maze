using AppServices;

namespace WebApi;

public static class MazeEndpoints
{
    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/Mazes")
            .WithTags("Mazes");

        group.MapPost("/upload", UploadTravelFile)
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery()
            .Produces<MazeDto>(StatusCodes.Status201Created)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithDescription("Uploads a travel .txt file, parses it, stores it, and returns the created travel.");

        throw new NotImplementedException();
        return app;
    }

    private static async Task<IResult> UploadTravelFile(
        IFormFile? file,
        ApplicationDataContext db,
        IMazeParser parser,
        IMazeSolver solver)
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest("Not File uploaded");
        }

        string content;
        await using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            content = await reader.ReadToEndAsync();
        }

        ParsedMaze parsed;
        try
        {
            parsed = parser.ParseMaze(content);
        }
        catch (MazeParseException ex)
        {
            return Results.BadRequest(ex.ErrorCode.ToString() + " " + ex.Message);
        }

        var (solution, difficulty) = solver.SolveMaze(parsed);

        var entity = new MazeEntity
        {
            MazeData = parsed.Content,
            Difficulty = difficulty,
            SolutionSteps = solution
        };


        db.Mazes.Add(entity);
        await db.SaveChangesAsync();

        return Results.Created($"/Mazes/{entity.Id}", MapToDto(entity));
    }

    private record MazeDto(int Id, string MazeData, string SolutionSteps, int Difficulty);

    private static MazeDto MapToDto(MazeEntity maze)
        => new(
            Id: maze.Id,
            MazeData: maze.MazeData,
            SolutionSteps: maze.SolutionSteps,
            Difficulty: maze.Difficulty
        );
}