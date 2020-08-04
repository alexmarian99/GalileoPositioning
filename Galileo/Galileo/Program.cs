﻿using System;
using System.IO;

namespace Galileo
{
    class Program
    {
        static void Main(string[] args)
        {
            Galileo.Rinex fisRinex = new Rinex();
            fisRinex.ReadFIle(@"C:\Users\alexn\Desktop\GNSS\M0SE00ITA_R_20201970000_01D_MN.rnx");
            Console.WriteLine(fisRinex.NavigationFile.PGM);
        }
    }
}
