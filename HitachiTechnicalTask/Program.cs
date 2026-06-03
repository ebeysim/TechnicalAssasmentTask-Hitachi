using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HitachiTechnicalTask
{
    internal class Program
    {
        public record Position(int X, int Y);

        interface IPathfindingStrategy
        {
            List<Position> FindPath(CosmicMap map, Position start, Position end);
        }

        static bool IsInValidRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }
        static (int rows, int cols, int astronauts) GetAutoGenerationParameters(bool requireAstronauts)
        {
            int rows, cols, astronauts = 1; // Default to 1 if not required

            Console.Write("Enter number of rows (2-100): ");
            while (!int.TryParse(Console.ReadLine(), out rows) || !IsInValidRange(rows, 2, 100))
            {
                Console.Write("Invalid range. Enter rows (2-100): ");
            }

            Console.Write("Enter number of columns (2-100): ");
            while (!int.TryParse(Console.ReadLine(), out cols) || !IsInValidRange(cols, 2, 100))
            {
                Console.Write("Invalid range. Enter columns (2-100): ");
            }

            if (requireAstronauts)
            {
                Console.Write("Enter number of astronauts (1-3): ");
                while (!int.TryParse(Console.ReadLine(), out astronauts) || !IsInValidRange(astronauts, 1, 3))
                {
                    Console.Write("Invalid range. Enter astronauts (1-3): ");
                }
            }

            return (rows, cols, astronauts);
        }

        static bool IsValidInput(string input, int astCounter, bool isStationAdded)
        {
            string upperInput = input.Trim().ToUpper();

            // 1. Check Astronaut Limit: If we already have 3, don't allow any more 'S' tokens
            if (upperInput.StartsWith("S") && astCounter >= 3)
            {
                return false;
            }

            // 2. Check Space Station Limit: Only 1 'F' allowed on the entire map
            if (upperInput == "F" && isStationAdded)
            {
                return false;
            }

            if (!upperInput.StartsWith("S") && upperInput != "F" && upperInput != "0" && upperInput != "X" && upperInput != "D")
            {
                return false;
            }

            // If it passed every single guard clause above, the input is perfectly valid!
            return true;
        }

        class BFSPathfindingStrategy : IPathfindingStrategy
        {
            public List<Position> FindPath(CosmicMap map, Position start, Position end)
            {
               
                string[,] matrix = map.Grid;
                bool[,] visited = new bool[map.Rows, map.Columns];
                List<Position> path = new List<Position>();
                Queue<Position> queue = new Queue<Position>();
                Dictionary<Position, Position> cameFrom = new Dictionary<Position, Position>(); // To reconstruct the path later
                queue.Enqueue(start);
                visited[start.X, start.Y] = true;
                while(queue.Count > 0)
                {
                    Position current = queue.Dequeue();
                    if (current == end)
                    {
                        // Reconstruct path
                        Position step = end;
                        while (step != start)
                        {
                            path.Add(step);
                            step = cameFrom[step];
                        }
                        path.Add(start);
                        path.Reverse();
                        return path;
                    }

                    // Explore neighbors (up, down, left, right)
                    List<Position> neighbors = new List<Position>
                    {
                        new Position(current.X - 1, current.Y), // Up
                        new Position(current.X + 1, current.Y), // Down
                        new Position(current.X, current.Y - 1), // Left
                        new Position(current.X, current.Y + 1)  // Right
                    };
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor.X >= 0 && neighbor.X < map.Rows && neighbor.Y >= 0 && neighbor.Y < map.Columns) // Check bounds
                        {
                            if (!visited[neighbor.X, neighbor.Y] && matrix[neighbor.X, neighbor.Y] != "X" && !matrix[neighbor.X, neighbor.Y].StartsWith("S")) // Check if not visited and not an asteroid
                            {
                                queue.Enqueue(neighbor);
                                visited[neighbor.X, neighbor.Y] = true;
                                cameFrom[neighbor] = current; // Track the path
                            }
                        }
                    }
                }
                return new List<Position>(); // Return null if no path found
            }
            
        }

        class Astronaut
        {
            private string _id;
            public string Id { get { return _id; }
                set {
                    if(value != "S1" && value!="S2" && value!= "S3")
                    {
                        throw new ArgumentException(nameof(value), "Astronout id must be one of the folowing: S1, S2, S3");
                    }
                    _id = value;
                }
            }

            public Position StartLocation {  get; set; }
            public List<Position> Path { get; set; }
            public int PathCost { get; set; }
            public bool IsLost { get; set; }

            public Astronaut(string id, Position startLocation)
            {
                Id = id;
                StartLocation = startLocation;
                Path = new List<Position>();
            }

        }


        class CosmicMap
        {
            private int _rows;
            public int Rows
            {
                get { return _rows; }
                set
                {
                    if (!IsInValidRange(value, 2, 100))
                    {
                        throw new ArgumentException(nameof(value), "Rows must be between 2 and 100");
                    }
                    _rows = value;
                }
            }
            private int _colums;
            public int Columns
            {
                get { return _colums; }
                set
                {
                    if (!IsInValidRange(value, 2, 100))
                    {
                        throw new ArgumentException(nameof(value), "Columns must be between 2 and 100");
                    }
                    _colums = value;
                }
            }
            public string[,] Grid { get; set; }
            public Position SpaceStationLocation { get; set; } = new Position(0, 0);
            public List<Astronaut> Astronauts { get; set; } = new List<Astronaut>();

            public CosmicMap()
            {
                Rows = 2;
                Columns = 2;
            }
            public CosmicMap(int rows, int columns)
            {
                Rows = rows;
                Columns = columns;
                Grid = new string[Rows, Columns];
            }
            

            public void CreateGrid()
            {

                int astronautCounter = 0;
                bool isStationAdded = false;
                
                for (int i = 0; i < Rows; i++)
                {
                    bool isRowValid = true;
                    bool rowHasStation = false;

                    Console.WriteLine($"Enter line {i + 1} (comma-separated). You can add S for astronauts ({3 - astronautCounter} left), X for asteroids, and F for the space station:");
                    string? lineInput = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(lineInput))
                    {
                        Console.WriteLine("Line cannot be empty. Please retry this line.");
                        i--; // Decrement i so the loop stays on the exact same row index
                        continue;
                    }

                    string[] inputs = lineInput.Split(',');

                    if (inputs.Length != Columns)
                    {
                        Console.WriteLine($"Error: This row must have exactly {Columns} items. You entered {inputs.Length}. Please retry.");
                        i--;
                        continue;
                    }

                    for(int j = 0; j < Columns; j++)
                    {
                        if (!IsValidInput(inputs[j], astronautCounter, isStationAdded))
                        {
                            Console.WriteLine($"Invalid element found: '{inputs[j]}'. Please re-enter the entire line.");
                            isRowValid = false;
                            break; // Break out of the 'j' loop immediately
                        }
                        string token = inputs[j].Trim().ToUpper();
                        if (token == "F")
                        {
                            if (isStationAdded || rowHasStation)
                            {
                                Console.WriteLine("Validation Error: You cannot have more than one Space Station (F) on the map.");
                                isRowValid = false;
                                break;
                            }
                            rowHasStation = true; // Mark that we found an F on this line
                        }
                    }

                    if(!isRowValid)
                    {
                        i--;
                        continue;
                    }

                    for (int j = 0; j < Columns; j++)
                    {
                        string upperToken = inputs[j].Trim().ToUpper();

                        if (upperToken.StartsWith("S"))
                        {
                            astronautCounter++;
                            Astronauts.Add(new Astronaut(upperToken, new Position(i, j)));
                        }

                        if (upperToken == "F")
                        {
                            isStationAdded = true;
                            SpaceStationLocation = new Position(i, j);
                        }
                        Grid[i, j] = upperToken;

                    }
                }

                if (astronautCounter < 1)
                    throw new InvalidOperationException("Invalid Map: Mission requires at least 1 astronaut.");
                if (!isStationAdded)
                    throw new InvalidOperationException("Invalid Map: Mission requires exactly 1 space station.");
            }

            public void AutoGenerateGrid(int rows, int columns, int numAstronauts)
            {
                Rows = rows;
                Columns = columns;
                Grid = new string[Rows, Columns];
                Astronauts = new List<Astronaut>();
                bool isStationAdded = false;
                Random rand = new Random();


                Position[] astronautCoords = new Position[numAstronauts];
               
                for(int i = 0; i<numAstronauts; i++)
                {
                    int randomRow = rand.Next(0, rows);
                    int randomCol = rand.Next(0, columns);
                    Position pos = new(randomRow, randomCol);
                    if(astronautCoords.Contains(pos))
                    {
                        i--; // If we randomly generated a duplicate coordinate, try again for this astronaut
                        continue;
                    }
                    astronautCoords[i] = pos;
                    Astronauts.Add(new Astronaut($"S{i + 1}", pos));
                }

                while (!isStationAdded)
                {
                    int randomRow = rand.Next(0, rows);
                    int randomCol = rand.Next(0, columns);
                    Position pos = new(randomRow, randomCol);
                    if (astronautCoords.Contains(pos))
                    {
                        continue; // If we randomly generated a coordinate that already has an astronaut, try again
                    }
                    SpaceStationLocation = pos;
                    isStationAdded = true;
                }

                for (int i = 0; i < Rows; i++) {
                    for (int j = 0; j < Columns; j++) {
                        Position pos = new(i, j);
                        if (Astronauts.Any(a => a.StartLocation == pos))
                        {
                            Grid[i, j] = $"S{Astronauts.FindIndex(a => a.StartLocation == pos) + 1}";
                        }
                        else if (SpaceStationLocation == pos)
                        {
                            Grid[i, j] = "F";
                        }
                        else
                        {
                            Grid[i, j] = rand.NextDouble() < 0.2 ? "X" : "0"; ; // Open space
                        }
                    }
                }

                // Implement logic to auto-generate a valid grid with astronauts, asteroids, and a space station. This can be used for testing purposes.
                
            }

            public void PrintGrid()
            {
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        Console.Write(Grid[i, j] + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }



        }

        class MissionControl
        {
            private IPathfindingStrategy _pathfindingStrategy;
            public MissionControl(IPathfindingStrategy pathfindingStrategy)
            {
                _pathfindingStrategy = pathfindingStrategy;
            }
            public void ExecuteMission(CosmicMap map)
            {
                foreach (var astronaut in map.Astronauts)
                {
                    List<Position> path = _pathfindingStrategy.FindPath(map, astronaut.StartLocation, map.SpaceStationLocation);
                    astronaut.Path = path;
                    astronaut.PathCost = path.Count-1;
                    if(path.Count == 0)
                    {
                        astronaut.IsLost = true;
                    }
                    //Console.WriteLine(astronaut.PathCost);
                    //Console.WriteLine(astronaut.IsLost);
                }
               

                
                
            }

            public void PrintReport(CosmicMap map)
            {
                var astronauts = map.Astronauts.OrderBy(a => a.IsLost).ThenBy(a => a.PathCost).ToList();
                string[,] grid = new string[map.Rows, map.Columns];

                for(int i =0; i<map.Rows; i++)
                {
                    for(int j =0; j<map.Columns; j++)
                    {
                        grid[i, j] = map.Grid[i, j];
                    }
                }
                foreach (var astronaut in astronauts)
                {

                    if (astronaut.IsLost)
                    {
                        Console.WriteLine($"Mission failed — Astronaut {astronaut.Id} lost in space!\n");
                        continue;
                    }
                    Console.WriteLine($"Astronaut {astronaut.Id} - Shortest path:  {(astronaut.PathCost + " steps")}");
                    for(int i = 0; i<map.Rows; i++)
                    {
                        for(int j = 0; j<map.Columns; j++)
                        {
                            if (astronaut.Path.Contains(new Position(i, j)) && map.Grid[i, j] != "F" && !map.Grid[i, j].StartsWith("S"))
                            {
                                grid[i, j] = "*";
                            }
                            Console.Write(grid[i, j] + " ");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();

                }
            }
        }
        static void Main(string[] args)
        {
            string option;
            MissionControl ms = new MissionControl(new BFSPathfindingStrategy());
            CosmicMap map=new CosmicMap();

            do
            {
                Console.WriteLine("----------Space Station----------");
                Console.WriteLine("1. Create map");
                Console.WriteLine("2. Auto-generate map");
                Console.WriteLine("3. Show map");
                Console.WriteLine("4. Find shortest path");
                Console.WriteLine("0. Exit");

                Console.Write("\nEnter an option: ");
                option = Console.ReadLine();
                try
                {
                    switch (option)
                    {
                        case "1":
                            var (rows, cols, _) = GetAutoGenerationParameters(false);
                            map = new CosmicMap(rows, cols);
                            map.CreateGrid();
                            Console.WriteLine("Map created successfully!\n");   
                            break;
                        case "0":
                            break;
                        case "2":
                            var(rowsA, colsA, astronauts) = GetAutoGenerationParameters(true);
                            map.AutoGenerateGrid(rowsA,colsA,astronauts);
                            Console.WriteLine("Map auto-generated successfully!\n");
                            break;
                        case "3":
                            if(map.Grid == null)
                            {
                                Console.WriteLine("No map found, please create or auto-generate a map first.\n");
                            }
                            else
                            {
                                map.PrintGrid();
                            }
                            break;
                        case "4":
                            ms.ExecuteMission(map);
                            ms.PrintReport(map);
                            break;
                        default:
                            Console.WriteLine("Not an valid option try again!\n");
                            break;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            } while (option != "0");

        }
    }
}
