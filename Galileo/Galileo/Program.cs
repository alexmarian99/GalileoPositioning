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
            Rinex fisier = new Rinex();
            fisier.ReadFIle(@"C:\Users\alexn\Desktop\GNSS\M0SE00ITA_R_20201970000_01D_MN.rnx");

        }
    }
}
