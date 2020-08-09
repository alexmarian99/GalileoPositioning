using Galileo.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Galileo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Code
            Rinex fisRin = new Rinex();
            fisRin.ReadFIle(@"D:\M0SE00ITA_R_20201970000_01D_30S_MO.rnx");
            int id = 0;
            foreach (var x in fisRin.ObservationFile.Entries)
            {
                id += 1;
                Console.WriteLine(x.DateOfRecord + "   |   " + id);
            }
        }
    }
}
