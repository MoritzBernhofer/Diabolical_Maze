using AppServices;
using AppServices.Importer;

namespace ImporterTests;

public class DataImporterTests
{
    private readonly IFileReader fileReader;
    private readonly IMazeParser csvParser;
    private readonly IMazeImportDatabaseWriter databaseWriter;
    private readonly MazeImporter importer;

    public DataImporterTests()
    {
        fileReader = Substitute.For<IFileReader>();
        csvParser = Substitute.For<IMazeParser>();
        databaseWriter = Substitute.For<IMazeImportDatabaseWriter>();
        importer = new MazeImporter(fileReader, csvParser, databaseWriter);
    }

    [Fact]
    public async Task ImportFromCsvAsync_SuccessfulImport_ReturnsCount()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Name;DecimalProperty\nTest1;10.5";
        var dummies = new List<Maze>
        {
            new() { Name = "Test1", DecimalProperty = 10.5m }
        };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseMaze(csvContent).Returns(dummies);

        // Act
        var result = await importer.ImportMazeAsync(csvFilePath, isDryRun: false);

        // Assert
        Assert.Equal(1, result);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).ClearAllAsync();
        await databaseWriter.Received(1).WriteMazeAsync(Arg.Is<IEnumerable<Maze>>(d => d.Count() == 1));
        await databaseWriter.Received(1).CommitTransactionAsync();
        await databaseWriter.DidNotReceive().RollbackTransactionAsync();
    }

    [Fact]
    public async Task ImportFromCsvAsync_DryRun_RollsBackTransaction()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Name;DecimalProperty\nTest1;10.5\nTest2;20.75";
        var dummies = new List<Maze>
        {
            new() { Name = "Test1", DecimalProperty = 10.5m },
            new() { Name = "Test2", DecimalProperty = 20.75m }
        };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseMaze(csvContent).Returns(dummies);

        // Act
        var result = await importer.ImportMazeAsync(csvFilePath, isDryRun: true);

        // Assert
        Assert.Equal(2, result);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromCsvAsync_FileReaderThrows_RollsBackAndRethrows()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var expectedException = new FileNotFoundException("File not found");
        fileReader.ReadAllTextAsync(csvFilePath).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await importer.ImportMazeAsync(csvFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromCsvAsync_CsvParserThrows_RollsBackAndRethrows()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Invalid content";
        var expectedException = new InvalidOperationException("Invalid CSV");

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseMaze(csvContent).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await importer.ImportMazeAsync(csvFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromCsvAsync_DatabaseWriterThrows_RollsBackAndRethrows()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Name;DecimalProperty\nTest1;10.5";
        var dummies = new List<Maze>
        {
            new() { Name = "Test1", DecimalProperty = 10.5m }
        };
        var expectedException = new InvalidOperationException("Database error");

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseMaze(csvContent).Returns(dummies);
        databaseWriter.WriteMazeAsync(Arg.Any<IEnumerable<Maze>>()).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await importer.ImportMazeAsync(csvFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromCsvAsync_EmptyFile_ReturnsZero()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Name;DecimalProperty\n";
        var dummies = new List<Maze>();

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseMaze(csvContent).Returns(dummies);

        // Act
        var result = await importer.ImportMazeAsync(csvFilePath, isDryRun: false);

        // Assert
        Assert.Equal(0, result);
        await databaseWriter.Received(1).WriteMazeAsync(Arg.Is<IEnumerable<Maze>>(d => d.Count() == 0));
        await databaseWriter.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromCsvAsync_CallsServicesInCorrectOrder()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Name;DecimalProperty\nTest1;10.5";
        var dummies = new List<Maze> { new() { Name = "Test1", DecimalProperty = 10.5m } };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseMaze(csvContent).Returns(dummies);

        // Act
        await importer.ImportMazeAsync(csvFilePath, isDryRun: false);

        // Assert - Verify order of calls
        Received.InOrder(async () =>
        {
            await databaseWriter.BeginTransactionAsync();
            await databaseWriter.ClearAllAsync();
            await fileReader.ReadAllTextAsync(csvFilePath);
            csvParser.ParseMaze(csvContent);
            await databaseWriter.WriteMazeAsync(Arg.Any<IEnumerable<Maze>>());
            await databaseWriter.CommitTransactionAsync();
        });
    }
}
