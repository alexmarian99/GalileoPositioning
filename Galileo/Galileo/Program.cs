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
            Galileo.Rinex fisRinex = new Rinex();
            fisRinex.ReadFIle(@"D:\M0SE00ITA_R_20201970000_01D_30S_MO.rnx");
            Console.WriteLine(fisRinex.ObservationFile.Entries[0].Satellites[0].Data[0]);

            /*string fisier = File.ReadAllText(@"D:\M0SE00ITA_R_20201970000_01D_30S_MO.rnx");
            string continut = fisier.Split("END OF HEADER")[1];
            Console.WriteLine(continut.Split("> ")[1].Split('\n'));*/
        }
        //buna
    }
}
