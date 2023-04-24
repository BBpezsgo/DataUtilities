using System;
using System.Collections.Generic;

namespace DataUtilities
{
    using ReadableFileFormat;

    class Yeah
    {
        static void Main(string[] args)
        {
            {
                var yeah = new Serializer.Serializer();

                int w = 16;
                int h = 16;

                int[][] matrix = new int[h][];
                for (int i = 0; i < matrix.Length; i++) matrix[i] = new int[w];

                Random rand = new();
                for (int y = 0; y < matrix.Length; y++)
                {
                    for (int x = 0; x < matrix[y].Length; x++)
                    {
                        matrix[y][x] = rand.Next(int.MinValue, int.MaxValue);
                    }
                }

                yeah.Serialize(matrix);

                yeah.Serialize(new Dictionary<double, int>
                {
                    { rand.NextDouble(), rand.Next() },
                    { rand.NextDouble(), rand.Next() },
                    { rand.NextDouble(), rand.Next() },
                    { rand.NextDouble(), rand.Next() },
                });

                System.IO.File.WriteAllBytes("./yeah.bin", yeah.Result);

                for (int y = 0; y < matrix.Length; y++)
                {
                    for (int x = 0; x < matrix[y].Length; x++)
                    {
                        Console.Write($"{matrix[y][x],4} ");
                    }
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
            {
                var raw = System.IO.File.ReadAllBytes("./yeah.bin");
                var yeah = new Serializer.Deserializer(raw);
                int[][] matrix = yeah.DeserializeArray2D<int>();

                for (int y = 0; y < matrix.Length; y++)
                {
                    for (int x = 0; x < matrix[y].Length; x++)
                    {
                        Console.Write($"{matrix[y][x],4} ");
                    }
                    Console.WriteLine();
                }

                var yeah2 = yeah.DeserializeDictionary<double, int>();
            }
            Console.WriteLine();
            {
                Value yeah = Value.Object();
                yeah["hello"] = Value.Literal("Hello");
                yeah["xd"] = Value.Literal(555);
                yeah["list"] = Value.Object(new string[] { "a", "b", "c", "d", "e", "f" });
                yeah["bol"] = Value.Literal(true);
                yeah["empty"] = Value.Literal("");
                System.IO.File.WriteAllText("./yeah.sdf", yeah.ToSDF(true));
                System.IO.File.WriteAllText("./yeah.json", yeah.ToJSON(true));

                Value yeahLoaded = Parser.LoadFile("./yeah.sdf").Value;

                Console.WriteLine(yeah.ToSDF());
                Console.WriteLine(yeahLoaded.ToSDF());

                Console.WriteLine(yeah.Equals(yeahLoaded));
            }
        }
    }
}
