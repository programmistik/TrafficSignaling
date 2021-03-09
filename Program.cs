using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TrafficSignaling
{
    public class Street
    {
        public string Name { get; set; }
        public Node Start { get; set; }
        public Node Finish { get; set; }
        public int Time { get; set; }

        public int GreenLightTime { get; set; }
        public int UsedInCarsPath { get; set; }

        public void Used()
        {
            UsedInCarsPath++;
        }
    }

    public class Car
    {
        public int StreetCount { get; set; }
        public List<Street> Streets { get; set; }
        public int FullTime { get; set; }

        public LinkedList<Node> Path { get; set; }

        public void AddTime(Street street)
        {
            FullTime += street.Time;
        }
    }

    public class Node
    {
        public int id { get; set; }
        public List<Street> StreetsStarts { get; set; }
        public List<Street> StreetsEnds { get; set; }

        public HashSet<Car> Cars { get; set; } // present in car path

    }


    class Program
    {
        static void Main(string[] args)
        {
            var exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var thisAppPath = appPathMatcher.Match(exePath).Value;

            string outputPath = Path.Combine(thisAppPath, "OutputFiles");
            //string PathToFile = Path.Combine(thisAppPath, "InputFiles\\a.txt");
            //string PathToFile = Path.Combine(thisAppPath, "InputFiles\\b.txt");
            //string PathToFile = Path.Combine(thisAppPath, "InputFiles\\c.txt");
            //string PathToFile = Path.Combine(thisAppPath, "InputFiles\\d.txt");
            //string PathToFile = Path.Combine(thisAppPath, "InputFiles\\e.txt");
            string PathToFile = Path.Combine(thisAppPath, "InputFiles\\f.txt");

            Simulation sim = Simulation.LoadProblem(PathToFile);

            //var Intersections = sim.AllNodes.Where(n => n.Cars.Count >= 1).ToArray();
            //var IntCount = Intersections.Count();


            var UsedStreets = sim.AllStreets.Where(s => s.UsedInCarsPath > 0).ToArray().OrderBy(s => s.UsedInCarsPath).ToList();
            // add GreenLight time +1

            var top10 = UsedStreets.Take(10);
            foreach (var item in top10)
            {
                item.GreenLightTime++;
            }

            var Intersections = new HashSet<Node>();
            foreach (var item in UsedStreets)
            {
                Intersections.Add(item.Start);
                Intersections.Add(item.Finish);
            }

            var IntCount = Intersections.Count();

            Console.WriteLine(IntCount);            


            // Output
            var fileName = Path.GetFileNameWithoutExtension(PathToFile);
            var pp = outputPath + "\\" + fileName + ".out.txt";
            using (StreamWriter sw = new StreamWriter(pp))
            {
                sw.WriteLine(IntCount);
                var orderedInt = Intersections.OrderBy(i => i.id).ToList();
                foreach (var item in orderedInt)
                {
                    sw.WriteLine(item.id);
                    sw.WriteLine(item.StreetsEnds.Count);
                    var orderedStreets = item.StreetsEnds.OrderBy(s => s.Time).ToList();
                    foreach (var str in orderedStreets)
                    {
                        if (str.GreenLightTime > 1) { Console.WriteLine(str.GreenLightTime.ToString()); }
                        sw.WriteLine(str.Name + " " + str.GreenLightTime.ToString());
                    }
                }
            }

            Console.WriteLine("Done!");

            Console.ReadKey();
        }
    }

    class Simulation
    {
        public int TotalTime { get; set; }
        public int TotalNodeCount { get; set; }
        public int TotalStreetCount { get; set; }
        public int TotalCarCount { get; set; }
        public int Score { get; set; }

        public List<Node> AllNodes { get; set; }
        public List<Street> AllStreets { get; set; }
        public List<Car> Cars { get; set; }


        public static Simulation LoadProblem(string fileName)
        {

            using (StreamReader sr = new StreamReader(fileName))
            {
                var line = sr.ReadLine();
                var Parts = line.Split(' ');

                int sec = int.Parse(Parts[0]);          // TotalTime
                int inters = int.Parse(Parts[1]);       // TotalNodeCount
                int strs = int.Parse(Parts[2]);         // TotalStreetCount
                int crs = int.Parse(Parts[3]);          // TotalCarCount
                int sc = int.Parse(Parts[4]);           // Score

                var streets = new List<Street>();       // AllStreets
                var nodes = new List<Node>();           // AllNodes

                for (int k = 0; k < strs; k++)
                {
                    line = sr.ReadLine();
                    Parts = line.Split(' ');
                    var st = int.Parse(Parts[0]);
                    var fin = int.Parse(Parts[1]);
                    var name = Parts[2];
                    var secs = int.Parse(Parts[3]);

                    var n1 = nodes.Find(n => n.id == st);
                    var n2 = nodes.Find(n => n.id == fin);

                    if (nodes.Exists(n => n == n1) == false)
                    {
                        n1 = new Node { id = st, StreetsStarts = new List<Street>(), StreetsEnds = new List<Street>(), Cars = new HashSet<Car>() };
                        nodes.Add(n1);
                    }

                    if (nodes.Exists(n => n == n2) == false)
                    {
                        n2 = new Node { id = fin, StreetsStarts = new List<Street>(), StreetsEnds = new List<Street>(), Cars = new HashSet<Car>() };
                        nodes.Add(n2);
                    }

                    var newStreet = new Street() { Name = name, Start = n1, Finish = n2, Time = secs, GreenLightTime = 1, UsedInCarsPath = 0 };
                    n2.StreetsEnds.Add(newStreet);
                    n1.StreetsStarts.Add(newStreet);

                    streets.Add(newStreet);

                }

                var cars = new List<Car>();


                for (int k = 0; k < crs; k++)
                {

                    line = sr.ReadLine();
                    Parts = line.Split(' ');
                    var strCount = int.Parse(Parts[0]);

                    var newCar = new Car
                    {
                        StreetCount = strCount,
                        Streets = new List<Street>(),
                        Path = new LinkedList<Node>(),
                        FullTime = 0
                    };


                    for (int m = 1; m <= strCount; m++)
                    {
                        var strName = Parts[m];

                        var str = streets.Where(x => x.Name.Equals(strName)).FirstOrDefault();
                        str.Used();

                        if (m == 1)
                        {
                            newCar.Path.AddLast(str.Start);
                            str.Start.Cars.Add(newCar);
                        }

                        newCar.Streets.Add(str);
                        newCar.Path.AddLast(str.Finish);
                        newCar.AddTime(str);


                        str.Start.Cars.Add(newCar);
                        str.Finish.Cars.Add(newCar);
                    }
                    cars.Add(newCar);

                }

                return new Simulation
                {
                    TotalTime = sec,
                    TotalNodeCount = inters,
                    TotalStreetCount = strs,
                    TotalCarCount = crs,
                    Score = sc,
                    AllStreets = streets,
                    AllNodes = nodes,
                    Cars = cars

                };
            }
        }

    }
}



