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
                packedTables[i] =                 result[0];
                packedTables[i + tables.Length] = result[1];
            }

            string[] names = new string[]
            {
                "Pawn", "Knight", "Bishop", "Rook", "Queen", "King",
            };
            Console.WriteLine("{");
            for (int i = 0; i < packedTables.Length; i++)
            {
                Console.WriteLine("//" + names[i % names.Length] + (i < names.Length ? " files" : " ranks"));
                Console.WriteLine(packedTables[i] + "ul, ");
            }
            Console.WriteLine("};");

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
                int result = (int)((tables[6] >> (i * 8)) & 0xFFul);
                Console.WriteLine(result);
            }
        }

        private int[][][] GetQuantizedTables()
        {
            // Tables tuned with Gedas' tuner (https://github.com/GediminasMasaitis/texel-tuner)
            // For approximately 33200 epochs
            double[][][] tunedTables = new double[][][]
            {
                // Pawn
                new double[][]
                {
                    // Files
                    new double[] { 6.37693, 8.25274, 6.36264, 7.27107, 7.89938, 7.62474, 10.8345, 6.15893 },

                    // Ranks
                    new double[] { 8, 38.6946, 25.976, 14.8538, 12.4039, 12.1668, 12.847, 15 },
                },
                // Knight
                new double[][]
                {
                    new double[] { 23.8633, 28.6201, 31.352, 33.1159, 33.1128, 33.0569, 30.0908, 27.1066 },
                    new double[] { 27.446, 38.7536, 44.3117, 43.5668, 40.5332, 37.9316, 34.7009, 30.6918 },
                },
                // Bishop
                new double[][]
                {
                    new double[] { 30.0394, 32.5581, 33.4164, 33.4935, 33.6424, 32.9437, 34.1131, 31.0565 },
                    new double[] { 37.3327, 40.6508, 44.8988, 43.9623, 43.3967, 42.9432, 41.217, 36.7398 },
                },
                // Rook
                new double[][]
                {
                    new double[] { 45.3296, 47.0674, 48.7155, 49.4218, 49.0558, 47.8178, 47.1922, 42.7897 },
                    new double[] { 71.0018, 71.7638, 69.9652, 67.8846, 64.8977, 63.0683, 62.3737, 63.576 },
                },
                // Queen
                new double[][]
                {
                    new double[] { 104.309, 106.592, 108.553, 109.729, 109.434, 110.193, 110.296, 110.696 },
                    new double[] { 114.849, 114.324, 117.898, 114.726, 113.059, 111.951, 111.246, 109.235 },
                },
                // King
                new double[][]
                {
                    new double[] { 0.664526, 5.67965, 5.82646, 3.96895, 3.79627, 3.67599, 6.6648, 3.48458 },
                    new double[] { 8.09455, 15.3056, 17.8796, 16.2865, 13.7376, 11.4234, 10.3787, 9.30962 },
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
