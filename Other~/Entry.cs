using System;
using System.Collections.Generic;
using System.Linq;

namespace DataUtilities
{
    using ReadableFileFormat;

    class Yeah
    {
        const string Path = @"C:\Users\bazsi\Desktop\Data Util Tests\";

        static void Main(string[] args)
        {
            Serializer.Serializer serializer = new();
            Random rand = new();

            {
                int[][] matrix = new int[16][];
                for (int i = 0; i < matrix.Length; i++) matrix[i] = new int[16];

                for (int y = 0; y < matrix.Length; y++)
                {
                    for (int x = 0; x < matrix[y].Length; x++)
                    {
                        matrix[y][x] = rand.Next(int.MinValue, int.MaxValue);
                    }
                }

                serializer.Serialize(matrix);

                serializer.Serialize(new Dictionary<double, int>
                {
                    { rand.NextDouble(), rand.Next() },
                    { rand.NextDouble(), rand.Next() },
                    { rand.NextDouble(), rand.Next() },
                    { rand.NextDouble(), rand.Next() },
                });

                System.IO.File.WriteAllBytes(Path + "yeah.bin", serializer.Reinitialize());

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
                var raw = System.IO.File.ReadAllBytes(Path + "yeah.bin");
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
                yeah["empty"] = Value.Literal(string.Empty);
                TestSDF(yeah);
            }
            {
                serializer.Serialize(new int[ushort.MaxValue]);
                TestCompression(serializer.Reinitialize());
            }
            {
                FilePacker.Packer packer = new(new FilePacker.PackHeader()
                {
                    SaveMetadata = true,
                });
                packer.Pack(
                    @"C:\Users\bazsi\Desktop\Nothing Assets\",
                    @"C:\Users\bazsi\Desktop\Data Util Tests\bruh.bin");
            }
            {
                var virtualFolder = FilePacker.VirtualUnpacker.Unpack(@"C:\Users\bazsi\Desktop\Data Util Tests\bruh.bin");
            }
        }

        static void TestSDF(Value data)
        {
            Console.WriteLine($" === SDF ===");
            System.IO.File.WriteAllText(Path + "yeah.sdf", data.ToSDF(true));
            System.IO.File.WriteAllText(Path + "yeah.json", data.ToJSON(true));

            Serializer.Serializer serializer = new();
            serializer.Serialize(data);
            System.IO.File.WriteAllBytes(Path + "yeah_sdf.bin", serializer.Result);

            Serializer.Deserializer deserializer = new(System.IO.File.ReadAllBytes(Path + "yeah_sdf.bin"));
            Value loaded_bin = deserializer.DeserializeSdfValue();

            if (!Parser.TryLoadFile(Path + "yeah.sdf", out Value loaded_text))
            { throw new Exception($"Failed to load the file"); }

            // Console.WriteLine(data.ToSDF());
            // Console.WriteLine(loaded_text.ToSDF());

            Console.WriteLine($"Parser: {data.Equals(loaded_text)}");
            Console.WriteLine($"Deserializer: {data.Equals(loaded_bin)}");
        }

        static void TestCompression(byte[] data)
        {
            Console.WriteLine($" === COMPRESSION ===");
            System.IO.File.WriteAllBytes(Path + "compression_0.bin", data);
            System.IO.File.WriteAllBytes(Path + "compression_1.bin", Compression.RLE.Compress(data));

            byte[] readed_1 = Compression.RLE.Decompress(System.IO.File.ReadAllBytes(Path + "compression_1.bin"));

            System.IO.File.WriteAllBytes(Path + "compression_2.bin", readed_1);

            Console.WriteLine($"RLE: {data.SequenceEqual(readed_1)}");
        }
    }
}
