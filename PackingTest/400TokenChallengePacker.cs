using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackingTest
{
    internal class _400TokenChallengePacker : IPSTPacker
    {
        public void PackTables()
        {
            // We'll need 8 decimals in total:
            // 96 bits per decimal / 6 pieceTypes = 16 bits per piece
            // 
            // 4 bits for MG score, 4 bits for EG score, 
            // And 4 bits for MG score, and 4 bits for EG score
            // However, we can optimize this a little bit further by mirroring the board on the X axis during our evalutions
            // Which leaves us with a final tally of 4 decimals
            // 2 decimals for ranks, 2 for files 

            // This entire thing will be packed into a single decimal value
            // 96 bits usable space
            int[][] rankScoresMG =
            {
                // 6 pieceTypes (16 bits each)
                // Pawn
                new int[]
                {
                    // 4 rank bonuses (4 bits each)
                    0, 1, 2, 3,
                },
                // Knight
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Bishop
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Rook
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Queen
                new int[]
                {
                    0, 1, 2, 3,
                },
                // King
                new int[]
                {
                    0, 1, 2, 3,
                },
            };
            int[][] fileScoresMG =
            {
                // Pawn
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Knight
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Bishop
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Rook
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Queen
                new int[]
                {
                    0, 1, 2, 3,
                },
                // King
                new int[]
                {
                    0, 1, 2, 3,
                },
            };
            int[][] rankScoresEG =
            {
                // Pawn
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Knight
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Bishop
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Rook
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Queen
                new int[]
                {
                    0, 1, 2, 3,
                },
                // King
                new int[]
                {
                    0, 1, 2, 3,
                },
            };
            int[][] fileScoresEG =
            {
                // Pawn
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Knight
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Bishop
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Rook
                new int[]
                {
                    0, 1, 2, 3,
                },
                // Queen
                new int[]
                {
                    0, 1, 2, 3,
                },
                // King
                new int[]
                {
                    0, 1, 2, 3,
                },
            };


            // Get our packed values
            decimal packedRanksMG = PackFileOrRank(rankScoresMG);
            decimal packedFilesMG = PackFileOrRank(fileScoresMG);

            decimal packedRanksEG = PackFileOrRank(rankScoresEG);
            decimal packedFilesEG = PackFileOrRank(fileScoresEG);

            Console.WriteLine("Packed values: ");
            Console.WriteLine("Ranks MG: " + packedRanksMG);
            Console.WriteLine("Files MG: " + packedFilesMG);
            Console.WriteLine("Ranks EG: " + packedRanksEG);
            Console.WriteLine("Files EG: " + packedFilesEG);

            // Can can now access our tables
            byte[] evals = decimal.GetBits(packedRanksMG).Take(3).SelectMany(c => BitConverter.GetBytes(c).ToArray()).ToArray();
            foreach (byte eval in evals)
            {
                // First 4 bits
                Console.WriteLine(eval & 0x0F);

                // Second 4 bits
                Console.WriteLine((eval & 0xF0) >> 4);
            }
        }

        private decimal PackFileOrRank(int[][] table)
        {
            // Our rank and file tables will follow this format
            /*
                new int[pieceType][rank]
                {
                    new int[] { 0, 1, 2, 3 },
                    new int[] { 0, 1, 2, 3 },
                    new int[] { 0, 1, 2, 3 },
                    new int[] { 0, 1, 2, 3 },
                    new int[] { 0, 1, 2, 3 },
                    new int[] { 0, 1, 2, 3 },
                },
            */

            // We're going to pack two bytes per pieceType
            byte[] packedValues = new byte[table.Length * 2];
            for (int piece = 0; piece < table.Length; piece++)
            {
                // Add our score for rank 1 to the first 4 bits
                packedValues[piece * 2]  = (byte)(table[piece][0] & 0x0F);
                
                // Add our score for rank 2 into the last 4 bits
                packedValues[piece * 2] |= (byte)(table[piece][1] << 4 & 0xF0);

                // Repeat, but for ranks 3 and 4
                packedValues[piece * 2 + 1]  = (byte)(table[piece][2] & 0x0F);
                packedValues[piece * 2 + 1] |= (byte)(table[piece][3] << 4 & 0xF0);
            }

            // Now let's pack our table into 3 integers to then convert into a decimal
            int[] thirds = new int[4];
            for (int i = 0; i < 3; i++)
            {
                thirds[i] = BitConverter.ToInt32(packedValues, i * 4);
            }
            return new(thirds);
        }
    }
}
