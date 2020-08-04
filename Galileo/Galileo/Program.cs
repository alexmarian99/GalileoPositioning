using System;

namespace Galileo
{
    class Program
    {
        static void Main(string[] args)
        {
            Galileo.Rinex fisRinex = new Rinex();
            fisRinex.ReadFIle(@"caleFisier");
            Console.WriteLine(fisRinex.ObservationFile.Comments);
        }
    }
}
