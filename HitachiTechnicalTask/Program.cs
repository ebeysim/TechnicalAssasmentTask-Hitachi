using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HitachiTechnicalTask
{
    internal class Program
    { 
        interface IPathfindingStrategy
        {
            List<Position> FindPath(CosmicMap map, Position start, Position end);
        }

        public record Position(int X, int Y);
        static bool IsInValidRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

       class BFSPathfindingStrategy : IPathfindingStrategy
        {
            public List<Position> FindPath(CosmicMap map, Position start, Position end)
            {
                // Implement BFS pathfinding algorithm here
                // This is a placeholder implementation and should be replaced with actual BFS logic
                return new List<Position> { start, end };
            }
        }

        class Astranout
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

            public Astranout(string id)
            {
                Id = id;
                StartLocation = new Position(0, 0);
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
            public string[,] Grid { get; set; } = new string[0, 0];
            public Position SpaceStationLocation { get; set; } = new Position(0, 0);
            public List<Astranout> Astronauts { get; set; } = new List<Astranout>(2);

            public CosmicMap(int rows, int columns)
            {
                Rows = rows;
                Columns = columns;
                Grid = new string[rows, columns];
            }

        }

        class MissionControl
        {
            private IPathfindingStrategy _pathfindingStrategy;
            public MissionControl(IPathfindingStrategy pathfindingStrategy)
            {
                _pathfindingStrategy = pathfindingStrategy;
            }
            public void ExecuteMission(CosmicMap map, Astranout astronaut)
            {
                List<Position> path = _pathfindingStrategy.FindPath(map, astronaut.StartLocation, map.SpaceStationLocation);
                astronaut.Path = path;
                astronaut.PathCost = path.Count;
                // Additional logic to determine if the astronaut is lost can be added here
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
            Position posStation = new(60, 30);
            Position posAst1 = new(20, 30);
            Position posAst2 = new(20, 40);
            Astranout ast1 = new Astranout("S3");
            ast1.StartLocation = posAst1;
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


            // ---CosmicMap class test---
            //CosmicMap cm = new CosmicMap(3,5);
            //cm.SpaceStationLocation = posStation;
            //cm.Astronauts.Add(ast1);
            //Console.WriteLine(cm.SpaceStationLocation);
            //Console.WriteLine(cm.Astronauts[0].StartLocation);

            string[] inputs = Console.ReadLine().Split(',');

            Position postest = new Position(int.Parse(inputs[0]), int.Parse(inputs[1]));
            Console.WriteLine(postest.X + ", " + postest.Y);





        }
    }
}
