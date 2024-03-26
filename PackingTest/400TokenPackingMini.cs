using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackingTest
{
    internal class _400TokenPackingMini : IPSTPacker
    {
        public void PackTables()
        {
            // While decimals can contain more data, it's actually more efficient to pack such
            // little amount of data into ulongs for more token-efficient unpacking

            // Declare our tables
            Console.WriteLine("Getting quantized tables...");
            int[][][] tables = GetQuantizedTables();

            Console.WriteLine("\nPacking tables...");
            ulong[] packedTables = new ulong[tables.Length * 2];
            for (int i = 0; i < tables.Length; i++)
            {
                ulong[] result = PackTable(tables[i]);
                packedTables[i * 2] =     result[0];
                packedTables[i * 2 + 1] = result[1];
                Console.WriteLine(packedTables[i * 2]     + "ul,");
                Console.WriteLine(packedTables[i * 2 + 1] + "ul,");
            }

            Console.WriteLine("\nUnpacking tables...");
            ValidateTablesByUnpacking(packedTables);
        }

        private ulong[] PackTable(int[][] table)
        {
            // Each ulong gets 64 bits total
            // 8 ranks gives us 8 bits per rank
            // We'll use an additional ulong to cover files
            const int bitsPerEntry = 8;
            ulong mask = (ulong)Math.Pow(2, bitsPerEntry) - 1;

            // Tables comes in this format:
            /*
                new int[][]
                {
                    // File bonuses
                    new int[] { 0, 0, 0, 0, 0, 0, 0, 0, },

                    // Rank bonuses
                    new int[] { 0, 0, 0, 0, 0, 0, 0, 0, },
                };
            */

            ulong packedFiles = 0;
            const int files = 8;
            for (int i = 0; i < files; i++)
            {
                packedFiles |= ((ulong)table[0][i] & mask) << (i * bitsPerEntry);
            }

            ulong packedRanks = 0;
            const int ranks = 8;
            for (int i = 0; i < ranks; i++)
            {
                packedRanks |= ((ulong)table[1][i] & mask) << (i * bitsPerEntry);
            }

            return new ulong[] { packedFiles, packedRanks };
        }

        private void ValidateTablesByUnpacking(ulong[] tables)
        {
            // Let's just print the values to make sure we've unpacking correctly
            Console.WriteLine("Pawn files: ");
            for (int i = 0; i < 8; i++)
            {
                int result = (int)((tables[0] >> (i * 8)) & 0xFFul);
                Console.WriteLine(result);
            }

            Console.WriteLine("Pawn ranks: ");
            for (int i = 0; i < 8; i++)
            {
                int result = (int)((tables[1] >> (i * 8)) & 0xFFul);
                Console.WriteLine(result);
            }
        }

        private int[][][] GetQuantizedTables()
        {
            // Tables tuned with Gedas' tuner (https://github.com/GediminasMasaitis/texel-tuner)
            double[][][] tunedTables = new double[][][]
            {
                // Pawn
                new double[][]
                {
                    // Files
                    new double[] { 6.38393, 8.25037, 6.34143, 7.28548, 7.91305, 7.62703, 10.8092, 6.14806 },

                    // Ranks
                    new double[] { 8, 38.6862, 25.9661, 14.8493, 12.4129, 12.1783, 12.834, 15 },
                },
                // Knight
                new double[][]
                {
                    new double[] { 23.8536, 28.6229, 31.3426, 33.1235, 33.1266, 33.0407, 30.1135, 27.1036 },
                    new double[] { 27.4406, 38.744, 44.3008, 43.5844, 40.5335, 37.9205, 34.7114, 30.7055 },
                },
                // Bishop
                new double[][]
                {
                    new double[] { 30.0289, 32.5641, 33.4458, 33.4804, 33.6129, 32.9522, 34.1189, 31.0432 },
                    new double[] { 37.3264, 40.6477, 44.8869, 43.9472, 43.3935, 42.9297, 41.2194, 36.7717 },
                },
                // Rook
                new double[][]
                {
                    new double[] { 45.3384, 47.057, 48.7252, 49.4151, 49.0465, 47.811, 47.2039, 42.8001 },
                    new double[] { 70.9934, 71.758, 69.9503, 67.8935, 64.8939, 63.0599, 62.3776, 63.5789 },
                },
                // Queen
                new double[][]
                {
                    new double[] { 104.31, 106.598, 108.569, 109.718, 109.438, 110.194, 110.286, 110.683 },
                    new double[] { 114.84, 114.324, 117.889, 114.722, 113.055, 111.95, 111.256, 109.232 },
                },
                // King
                new double[][]
                {
                    new double[] { 0.690797, 5.67324, 5.82118, 3.98028, 3.81101, 3.6857, 6.65352, 3.49414 },
                    new double[] { 8.09363, 15.302, 17.8725, 16.2859, 13.736, 11.4353, 10.3925, 9.29613 },
                },
            };

            // Let's find the largest value in the table so we can calculate our compression factor to get them within our allowed range
            // Flatten the array twice to get all values, then grab max
            double largest = tunedTables.SelectMany(x => x).SelectMany(x => x).Max();

            Console.Write("{\n");

            // We'll have to scale some of our values to fit into our range
            double maxAllowed = 255;
            double scalingFactor = largest / maxAllowed;

            Console.WriteLine(scalingFactor);

            int[][][] quantizedTables = new int[tunedTables.Length][][];
            for (int pieceType = 0; pieceType < tunedTables.Length; pieceType++)
            {
                Console.Write("{\n");

                double[][] rankFileTable = tunedTables[pieceType];
                quantizedTables[pieceType] = new int[rankFileTable.Length][];
                for (int rankFile = 0; rankFile < rankFileTable.Length; rankFile++)
                {
                    Console.Write("{ ");

                    double[] bonuses = rankFileTable[rankFile];
                    quantizedTables[pieceType][rankFile] = new int[bonuses.Length];
                    for (int bonusIndex = 0; bonusIndex < bonuses.Length; bonusIndex++)
                    {
                        quantizedTables[pieceType][rankFile][bonusIndex] = (int)Math.Floor(bonuses[bonusIndex] / scalingFactor);
                        Console.Write((int)Math.Floor(bonuses[bonusIndex] / scalingFactor) + ", ");
                    }

                    Console.Write("}\n");
                }
                Console.Write("},\n");
            }
            Console.Write("}\n");
            return quantizedTables;
        }
    }
}
