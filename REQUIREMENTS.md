# Maze Generator - Project Requirements

## Overview
The Maze Generator is a full-stack application that allows users to upload maze files, parse and validate them, solve them algorithmically, store the results in a database, and visualize them through a modern web interface.

## Project Goals

1. **Maze Parsing & Validation** - Implement a robust maze parser that validates maze files according to defined rules
2. **Maze Solving Algorithm** - Implement a maze solver that finds the optimal path and calculates difficulty
3. **Web API Endpoints** - Create RESTful endpoints for maze management
4. **Frontend Application** - Build an Angular-based UI to display all valid mazes and their difficulty in a list
5. **Data Persistence** - Store mazes, difficulty, totalLength and solutions in a database


## 1. Maze Format Specification

### Symbols
Mazes are represented as text files using the following symbols:

- `S` - Start point (exactly one required)
- `E` - End point (exactly one required)
- `X` - Wall (impassable)
- `O` - Open path (passable)

### Structure Rules
- Mazes are rectangular grids
- Each row is separated by newline (`\n`)
- All rows must have the same length
- Must contain at least one row and one column
- No empty rows allowed

### Example - Simple 3x3 Maze

```
SXO
OOO
XOE
```

**Visual representation:**
```
S = Start    X = Wall     O = Open
O = Open     O = Open     O = Open
X = Wall     O = Open     E = End
```

**Solution:** `D,R,D,R` (Down, Right, Down, Right)

**Difficulty:** 1.97

D = Reachable Dead Ends (Any point that is surrounded by 3X, 2X and 1 border tile or 1X and 2 border tiles);
(border tiles are just the border off the site) 

L = Total reachable lenght of the maze, any O that is reachable from the Start

$$
\mathrm{Difficulty}(D,L)=10\left(1-e^{-(0.18D+0.008L)}\right)
$$

---

## 2. Implementation Requirements

### 2.1 Maze Parser Implementation

**Location:** `AppServices/MazeParser.cs`

**Interface:**
```csharp
public interface IMazeParser
{
    ParsedMaze ParseMaze(string content);
}
```

**Validation Rules (in order of checking):**

1. **EmptyFile** - File must not be empty
2. **InvalidSymbol** - Only `S`, `E`, `X`, `O` are allowed
3. **InvalidRowCount** - Must have at least 1 row; no empty rows
4. **InvalidColumnCount** - All rows must have the same length
5. **NotValidStartPoint** - Exactly one `S` required (not zero, not multiple)
6. **NotValidEndPoint** - Exactly one `E` required (not zero, not multiple)
7. **NoPossibleSolution** - A valid path must exist from S to E (use pathfinding algorithm like BFS/DFS)

**Test Coverage:**
All validation rules are covered in `AppServicesTests/MazeParserTests.cs`. The implementation must pass all existing tests.

**Error Handling:**
Throw `MazeParseException` with the appropriate `MazeParseError` enum value when validation fails.

---

### 2.2 Maze Solver Implementation

**Location:** `AppServices/MazeSolver.cs`

**Interface:**
```csharp
public interface IMazeSolver
{
    (string, int) SolveMaze(ParsedMaze travel);
}
```

**Return Value:**
- **string**: Solution steps as comma-separated directions (`U`, `D`, `L`, `R`)
  - `U` = Up (row - 1)
  - `D` = Down (row + 1)
  - `L` = Left (column - 1)
  - `R` = Right (column + 1)
- **double**: Difficulty rating (rounded to 2 decimal places)

**Algorithm Requirements:**

1 **Calculate Difficulty**
   - Based on maze complexity
   - Consider: reachable dead ends, total path length
   - Formula should produce results matching test expectations:
     - Simple 3x3 maze: ~1.97
     - Open 10x10 maze: ~8.59
     - Complex 100x100 maze: 10.00 (maximum)

**Test Coverage:**
Multiple test scenarios in `AppServicesTests/MazeSolverTests.cs` including:
- `SolveVerySimpleMaze` - 3x3 basic maze
- `SolveOpen10X10Maze` - Medium complexity
- `Solve100X100MazeWithDifferentStartingPoint` - High complexity

The `WalkThroughMaze` helper function validates that the solution correctly navigates from S to E.

---

### 2.3 Web API Endpoints

**Location:** `WebApi/MazeEndpoints.cs`

**Base Route:** `/Mazes`

#### Existing Endpoint

✅ **POST `/Mazes/upload`** - Upload and process a maze file
- **Input:** Multipart form data with maze text file
- **Process:** Parse → Validate → Solve → Store in database
- **Response:** 
  - `201 Created` with `MazeDto` on success
  - `400 Bad Request` with error details on failure

#### Required New Endpoints

📝 **GET `/Mazes`** - Retrieve all mazes
- **Response:** `200 OK` with array of `MazeDto`
- **Purpose:** Display list of all uploaded mazes in the frontend

📝 **GET `/Mazes/{id}`** - Retrieve a specific maze by ID
- **Parameters:** `id` (integer, path parameter)
- **Response:** 
  - `200 OK` with `MazeDto` if found
  - `404 Not Found` if maze doesn't exist
- **Purpose:** View details of a specific maze

📝 **DELETE `/Mazes/{id}`** - Delete a maze
- **Parameters:** `id` (integer, path parameter)
- **Response:** 
  - `204 No Content` on successful deletion
  - `404 Not Found` if maze doesn't exist
- **Purpose:** Remove unwanted mazes from the system

**DTO Structure:**
```csharp
public record MazeDto(
    int Id,
    string MazeData,
    string SolutionSteps,
    int Difficulty
);
```

**OpenAPI Documentation:**
- All endpoints should be documented with:
  - Clear descriptions
  - Request/response examples
  - Status code documentation
- OpenAPI spec is auto-generated to `Frontend/WebApi.json`

---

### 2.4 Frontend Implementation

**Location:** `Frontend/src/app/`

**Framework:** Angular 19+ with standalone components

**API Integration:**
- TypeScript API clients are auto-generated from `WebApi.json` using `ng-openapi-gen`
- Located in `src/app/api/`
- Run `npm run generate-web-api` to regenerate after API changes

#### Required Features

##### 2.4.1 Maze List View

**Component:** Create new component `maze-list`

**Features:**
- Display table/cards showing all uploaded mazes
- Show: ID, difficulty rating, preview of maze data (first few lines)
- (EXTRA FEATURE, NOT REQUIRED BY DEFAULT) Action buttons: View Details, Delete
- Error handling with user-friendly messages