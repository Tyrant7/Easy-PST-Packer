using PackingTest;
using System;

public class PieceTableGenerator
{
    public static void Main()
    {
        IPSTPacker fullPSTPacker = new _1024TokenChallengePacker();
        IPSTPacker partialPSTPacker = new _400TokenChallengePacker();
        IPSTPacker miniPacker = new _400TokenPackingMini();

        miniPacker.PackTables();
    }
}
