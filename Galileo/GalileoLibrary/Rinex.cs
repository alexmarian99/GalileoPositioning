using Galileo.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Galileo
{
    public class Rinex
    {
        public Classes.RinexObservation ObservationFile { get; internal set; }

        public Classes.RinexNavigation NavigationFile { get; internal set; }

        public void ReadFIle(string path)
        {
            string file = File.ReadAllText(path);

            string firstLine = file.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0];

            // Load data from observation file
            if (firstLine.Contains("OBSERVATION"))
            {

            }

            // Load data from navigation file
            else if (firstLine.Contains("NAV"))
            {

            }

            // TO ADD METEOROLOGICAL

            else
                throw new Exception("Invalid file to load");
        }

    }
}
