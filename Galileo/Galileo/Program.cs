using Galileo.Classes;
using System;
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
            string fisier = File.ReadAllText(@"D:\M0SE00ITA_R_20201970000_01D_30S_MO.rnx");
            string firstLine = fisier.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Contains
        }
    }
}
