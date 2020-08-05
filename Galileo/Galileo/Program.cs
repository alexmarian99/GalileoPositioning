using Galileo.Classes;
using System;
using System.Globalization;
using System.IO;

namespace Galileo
{
    class Program
    {
        static void Main(string[] args)
        {
            /*Galileo.Rinex fisRinex = new Rinex();
            fisRinex.ReadFIle(@"caleFisier");
            Console.WriteLine(fisRinex.ObservationFile.Comments);*/

            string fisier = File.ReadAllText(@"D:\navData.rnx");
            string continut = fisier.Split("END OF HEADER")[1];
            string[] intrare = continut.Split('\n');

            EntryNavigation entry = new EntryNavigation();
            /*            double x = Double.Parse(intrare[1].Split(" ")[7], CultureInfo.InvariantCulture);
            */
            double x = double.Parse("2.541998401284", CultureInfo.InvariantCulture);
            int p = 05;
            x -= p;
            string putere = "-02";
            double y = 0.170000000000D * Math.Pow(10, Convert.ToInt32(putere));
            Console.WriteLine(x);
            Console.WriteLine(y);
        }
    }
}
