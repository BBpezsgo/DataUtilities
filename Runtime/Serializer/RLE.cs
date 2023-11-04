using System.Collections.Generic;

#nullable enable

namespace DataUtilities.Compression
{
    public static class RLE
    {
        public static byte[] Compress(byte[] data)
        {
            List<byte> result = new();

            int index = 0;
            while (index < data.Length)
            {
                byte v = data[index];
                byte repeatingCount = 1;

                while (index + repeatingCount < data.Length &&
                       data[index + repeatingCount] == v &&
                       repeatingCount < byte.MaxValue)
                { repeatingCount++; }

                result.Add(v);
                result.Add(repeatingCount);
                index += repeatingCount;
            }

            return result.ToArray();
        }
        public static byte[] Decompress(byte[] data)
        {
            List<byte> result = new();

            int index = 0;
            while (index < data.Length)
            {
                byte v = data[index];
                byte repeatingCount = data[index + 1];

                for (byte i = 0; i < repeatingCount; i++)
                { result.Add(v); }

                index += 2;
            }

            return result.ToArray();
        }
    }
}
