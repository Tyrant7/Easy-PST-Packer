# Easy-PST-Packer
A program made for SebLague's Chess Challenge that takes a database of up to 12 tables of 64 squares and packs and unpacks them densly.

## How to use it?
Download the reposity open up Program.cs. From there you should see the appropriate tables. Simply paste in your values, indexed from a7-h1 into the tables, ensuring that there are 64 values per table, and run the program. You should see the packed tables, followed by a large array of numbers, and below that, the unpacked tables with the piece values baked in, indexed from PawnMG to KingEG. If you would like to see the tables without the baked piece values, you can disable baking the piece values in the debug by changing PrintBakedPieceValues to false in Program.cs.
Once you have your output, copy the packed tables array, and paste it into your bot. Additionally, you will need to copy your PieceValues array from Program.cs as that is not included as part of packing. 
From there you'll want to take a look at ExampleEvaluation.cs, which shows an example of how you would unpack these tables in your bot. If need be, you can copy the code from ExampleEvaluation directly into your bot by simply changing the constructor to "MyBot".

## Notes and Limitations
Table values are accessed through [square][scoreType] where scoreType is arranged as a number from 0-11 as PawnMG to KingEG.
Tables lose a small amount of precision due to the scaling factor which is used when packing and unpacking. Certain values main get rounded up or down and unpack one value away from where they originally sat.
Tables are limited to values between -167 and 187 with the default scaling factor. If you would like to use values outside this range be sure to update the scaling factor when packing and unpacking your tables. Doing so may result in an additional decrease in precision.
