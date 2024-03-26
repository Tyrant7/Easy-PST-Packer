using PackingTest;
using System;

public class PieceTableGenerator
{
    public static void Main()
    {
        IPSTPacker fullPSTPacker = new _1024TokenChallengePacker();
        IPSTPacker partialPSTPacker = new _400TokenChallengePacker();
        IPSTPacker miniPacker = new _400TokenPackingMini();

<<<<<<< HEAD
        miniPacker.PackTables();
=======
            eg_pawn_table,
            eg_knight_table,
            eg_bishop_table,
            eg_rook_table,
            eg_queen_table,
            eg_king_table
        };

        Console.WriteLine("Packed tables:\n");
        decimal[] packedData = PackData(table);

        Console.WriteLine("Unpacked tables:");
        int[][] unpackedData = UnpackData(packedData);

        PrintUnpackedData(unpackedData);
    }

    private const int tableSize = 64;
    private const int tableCount = 12;

    // Packs data in the following form: 
    // Square data in the first 12 bytes of each decimal (1 byte per piece type, 6 per gamephase)
    private static decimal[] PackData(List<int[]> tablesToPack)
    {
        decimal[] packedData = new decimal[tableSize];

        for (int square = 0; square < tableSize; square++)
        {
            // Pack all sets for this square into a byte array
            byte[] packedSquares = new byte[tableCount];
            for (int set = 0; set < tableCount; set++)
            {
                int[] setToPack = tablesToPack[set];
                sbyte valueToPack = (sbyte)Math.Round(setToPack[square] / 1.461);
                packedSquares[set] = (byte)(valueToPack & 0xFF);
            }

            // Create a new decimal based on the packed values for this square
            int[] thirds = new int[4];
            for (int i = 0; i < 3; i++)
            {
                thirds[i] = BitConverter.ToInt32(packedSquares, i * 4);
            }
            packedData[square] = new(thirds);
        }

        // Print the newly created table
        Console.Write("{ ");
        for (int square = 0; square < tableSize; square++)
        {
            if (square % 8 == 0)
                Console.WriteLine();
            Console.Write(packedData[square] + "m, ");
        }
        Console.WriteLine("\n};\n");

        return packedData;
    }

    // Unpacks a packed square table to be accessed with
    // pestoUnpacked[square][pieceType]
    private static int[][] UnpackData(decimal[] tablesToUnpack)
    {
        var pestoUnpacked = new int[tableSize][];
        pestoUnpacked = tablesToUnpack.Select(packedTable =>
        {
            int pieceType = 0;
            return decimal.GetBits(packedTable).Take(3)
                .SelectMany(c => BitConverter.GetBytes(c)
                    .Select(square => (int)((sbyte)square * 1.461) + (PrintBakedPieceValues ? PieceValues[pieceType++] : 0)))
                .ToArray();
        }).ToArray();

        return pestoUnpacked;
    }

    // Simply prints the unpacked data for debugging
    private static void PrintUnpackedData(int[][] unpackedData)
    {
        // Print all of the unpacked values
        for (int type = 0; type < tableCount; type++)
        {
            Console.WriteLine("\n\nTable for type: " + (ScoreType)type);
            for (int square = 0; square < tableSize; square++)
            {
                if (square % 8 == 0)
                    Console.WriteLine();

                Console.Write(unpackedData[square][type] + ", ");
            }
            Console.WriteLine();
        }
>>>>>>> e2725d07ad7f38f860bd10b3633ea69e22cc330c
    }
}
