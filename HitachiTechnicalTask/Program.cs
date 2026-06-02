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

            // 3. Check Allowed Characters: Must be an Astronaut (S), Station (F), Open space (0), or Asteroid (X)
            // Note: The assignment brief uses '0' (the number zero) for open spaces, not the letter 'O'!
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
                // Implement BFS pathfinding algorithm here
                // This is a placeholder implementation and should be replaced with actual BFS logic

                bool[,] visited = new bool[map.Rows, map.Columns];
                Queue<Position> queue = new Queue<Position>();
                queue.Enqueue(start);
                visited[start.X, start.Y] = true;
                int[,] dirs = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };

                while (queue.Count > 0)
                {
                    Position current = queue.Dequeue();
                    // Check if we have reached the destination
                }

                
                return new List<Position>(queue);
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
            public List<Astronaut> Astronauts { get; set; } = new List<Astronaut>(2);

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
                    astronaut.PathCost = path.Count;
                    // Additional logic to determine if the astronaut is lost can be added here
                }


                
                
            }

            public void PrintReport(CosmicMap map)
            {
                // Implement logic to print the report of the mission, including the path taken by each astronaut and their status (lost or not)

                /*
                 If an astronaut has no valid path, display: "Mission failed — Astronaut [S1/S2/S3] lost in space!" Failures must be reported on top of the output
                 Example Output
                    Astronaut S2 - Shortest path: 4 steps
                            S1 0 X 0 0 0 S2
                            X 0 0 0 0 X *
                            X X 0 X 0 X *
                            0 X X 0 0 X *
                            0 X X 0 0 0 F
                    Astronaut S1 - Shortest path: 10 steps
                            S1 * X 0 0 0 S2
                            X * * * * X 0
                            X X 0 X * X 0
                            0 X X 0 * X 0
                            0 X X 0 * * F
                 */
            }
        }
        static void Main(string[] args)
        {
            //Position posStation = new(60, 30);
            //Position posAst1 = new(20, 30);
            //Position posAst2 = new(20, 40);
            //Astronaut ast1 = new Astronaut("S3", posAst1);

            //Console.WriteLine(posStation == new Position(30, 60));

            /*
            // ---Astronout class test---
            //Console.WriteLine(pos);
            //Console.WriteLine(ast1.Id);
            //ast1.StartLocation = pos;
            //ast1.Path.Add(pos);
            //ast1.PathCost = ast1.Path.Count;
            //foreach (var coord in ast1.Path )
            //{
            //    Console.WriteLine(coord);
            //}
            //Console.WriteLine(ast1.PathCost);
            */

            /*
            // ---CosmicMap class test---
            //CosmicMap cm = new CosmicMap(3,5);
            //cm.SpaceStationLocation = posStation;
            //cm.Astronauts.Add(ast1);
            //Console.WriteLine(cm.SpaceStationLocation);
            //Console.WriteLine(cm.Astronauts[0].StartLocation);

            //string[] inputs = Console.ReadLine().Split(',');

            //Position postest = new Position(int.Parse(inputs[0]), int.Parse(inputs[1]));
            //Console.WriteLine(postest.X + ", " + postest.Y);
            */

            //string[,] arr2d = new string[2, 2];
            //int counter = 0;
            //bool isStationAdded = false;

            //for (int i = 0; i < arr2d.GetLength(0); i++)
            //{
            //    bool isRowValid = true;
            //    bool rowHasStation = false;

            //    Console.WriteLine($"Enter line {i + 1} (comma-separated). You can add S for astronauts ({3 - counter} left), X for asteroids, and F for the space station:");
            //    string? lineInput = Console.ReadLine();

            //    if (string.IsNullOrWhiteSpace(lineInput))
            //    {
            //        Console.WriteLine("Line cannot be empty. Please retry this line.");
            //        i--; // Decrement i so the loop stays on the exact same row index
            //        continue;
            //    }

            //    string[] inputs = lineInput.Split(',');

            //    // Safety Check: Did they enter the correct number of items for this row?
            //    if (inputs.Length != arr2d.GetLength(1))
            //    {
            //        Console.WriteLine($"Error: This row must have exactly {arr2d.GetLength(1)} items. You entered {inputs.Length}. Please retry.");
            //        i--; // Reset row index
            //        continue;
            //    }

            //    // Validate the entire row first before saving anything to our state
            //    for (int j = 0; j < arr2d.GetLength(1); j++)
            //    {
            //        if (!IsValidInput(inputs[j], counter, isStationAdded))
            //        {
            //            Console.WriteLine($"Invalid element found: '{inputs[j]}'. Please re-enter the entire line.");
            //            isRowValid = false;
            //            break; // Break out of the 'j' loop immediately
            //        }
            //        string token = inputs[j].Trim().ToUpper();
            //        if (token == "F")
            //        {
            //            if (isStationAdded || rowHasStation)
            //            {
            //                Console.WriteLine("Validation Error: You cannot have more than one Space Station (F) on the map.");
            //                isRowValid = false;

            //                break;
            //            }
            //            rowHasStation = true; // Mark that we found an F on this line
            //        }
            //    }

            //    // If a mistake was found, decrement 'i' to force a retry of this row, and skip assignments
            //    if (!isRowValid)
            //    {
            //        i--;
            //        continue;
            //    }

            //    // --- STATE UPDATE ZONE ---
            //    // If we reach this point, the row is 100% valid. Safe to update counters and save data!
            //    for (int j = 0; j < arr2d.GetLength(1); j++)
            //    {
            //        string upperToken = inputs[j].Trim().ToUpper();

            //        if (upperToken.StartsWith("S"))
            //        {
            //            counter++;
            //        }
            //        if (upperToken == "F")
            //        {
            //            isStationAdded = true;
            //        }

            //        arr2d[i, j] = upperToken; // Save clean, normalized uppercase string
            //    }
            //}

            //if (counter < 1)
            //    throw new InvalidOperationException("Invalid Map: Mission requires at least 1 astronaut.");
            //if (!isStationAdded)
            //    throw new InvalidOperationException("Invalid Map: Mission requires exactly 1 space station.");


            //for (int i = 0; i < arr2d.GetLength(0); i++)
            //{
            //    for(int j = 0; j < arr2d.GetLength(1); j++)
            //    {
            //        Console.Write(arr2d[i, j] + " ");
            //    }
            //    Console.WriteLine();
            //}

            //Console.WriteLine(counter);

            //int randomRow = new Random().Next(1, 3);
            //int numberOfAstronaut = 0;
            //int randomCol = new Random().Next(1, 3);
            //Position pos = new(randomRow, randomCol);
            //Position[] positions = new Position[4];
            //positions[0] = new Position(1, 1);

            //Console.WriteLine(positions.Contains(pos));
            //positions.Append(pos);
            //foreach (var p in positions)
            //{
            //    Console.WriteLine(p);

            //}

            CosmicMap map = new CosmicMap();
            map.AutoGenerateGrid(5,7,2);
            map.PrintGrid();

            CosmicMap map2 = new CosmicMap(2,2);
            map2.CreateGrid();
            map2.PrintGrid();
            Console.WriteLine(map2.Astronauts.Count);
            Console.WriteLine(map2.SpaceStationLocation);
        }
    }
}
