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
        private static RegexOptions options = RegexOptions.None;
        private static Regex regex = new Regex("[ ]{2,}", options);
        public Classes.RinexObservation ObservationFile { get; internal set; }

        public Classes.RinexNavigation NavigationFile { get; internal set; }

        public bool ReadFIle(string path)
        {
            // Citirea de fisier ca text
            string fisier = File.ReadAllText(path);

            // Se verifica daca este un fisier de navigatie sau de observatie
            if (!(fisier.Contains("OBSERVATION DATA") || fisier.Contains("NAV DATA")))
            {
                return false;
            }

            if (fisier.Contains("OBSERVATION DATA"))
            {
                ObservationFile = new Classes.RinexObservation();
                string header = fisier.Split("END OF HEADER")[0];
                string[] liniiHeader = header.Split('\n');

                foreach (string linie in liniiHeader)
                {
                    if (linie.Contains("RINEX VERSION / TYPE"))
                    {
                        linie.Replace("RINEX VERSION / TYPE", "");
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        ObservationFile.Version = float.Parse(param[1]);
                        if (param[5].ToLower() == "mixed")
                            ObservationFile.Type = Enums.Rinex.Types.Mixed;
                        else if (param[5].ToLower() == "galileo")
                            ObservationFile.Type = Enums.Rinex.Types.Galileo;
                        else
                            return false;
                    }

                    else if (linie.Contains("PGM / RUN BY / DATE"))
                    {
                        linie.Replace("PGM / RUN BY / DATE", "");
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        ObservationFile.PGM = param[0] + param[1];
                        ObservationFile.RunBy = param[2];
                        ObservationFile.Date = DateTime.SpecifyKind(DateTime.ParseExact(param[3] + param[4], "yyyyMMddHHmmss", CultureInfo.InvariantCulture), DateTimeKind.Utc);
                    }

                    else if (linie.Contains("COMMENT"))
                    {
                        linie.Replace("COMMENT", "");
                        if (ObservationFile.Comments == null)
                            ObservationFile.Comments = regex.Replace(linie, " ") + "\n";
                        else ObservationFile.Comments += regex.Replace(linie, " ") + "\n";
                    }

                    else if (linie.Contains("MARKER NAME"))
                    {
                        ObservationFile.Marker.Name = linie.Split(" ")[0];
                    }

                    else if (linie.Contains("MARKER NUMBER"))
                    {
                        ObservationFile.Marker.Number = linie.Split(" ")[0];
                    }

                    else if (linie.Contains("OBSERVER / AGENCY"))
                    {

                    }

                    else if (linie.Contains("REC # / TYPE / VERS"))
                    {

                    }

                    else if (linie.Contains("ANT # / TYPE"))
                    {

                    }

                    else if (linie.Contains("APPROX POSITION XYZ"))
                    {
                        /*string[] param = regex.Replace(linie, " ").Split(" ");
                        ObservationFile.Position.x = Convert.ToDouble(param[1]);
                        ObservationFile.Position.y = Convert.ToDouble(param[2]);
                        ObservationFile.Position.z = Convert.ToDouble(param[3]);*/
                    }

                    else if (linie.Contains("ANTENNA: DELTA H/E/N"))
                    {

                    }

                    else if (linie.Contains("SYS / # / OBS TYPES"))
                    {

                    }

                    else if (linie.Contains("SIGNAL STRENGTH UNIT"))
                    {

                    }

                    else if (linie.Contains("INTERVAL"))
                    {

                    }

                    else if (linie.Contains("TIME OF FIRST OBS"))
                    {

                    }

                    else if (linie.Contains("TIME OF LAST OBS"))
                    {

                    }

                    else if (linie.Contains("SYS / PHASE SHIFT"))
                    {

                    }

                    else if (linie.Contains("LEAP SECONDS"))
                    {

                    }
                }
            }

            else if (fisier.Contains("NAV DATA"))
            {
                NavigationFile = new Classes.RinexNavigation();
                string header = fisier.Split("END OF HEADER")[0];
                string[] liniiHeader = header.Split('\n');
                foreach (string linie in liniiHeader)
                {
                    if (linie.Contains("IONOSPHERIC CORR"))
                    {

                        if (linie.Contains("GAL"))
                        {

                            NavigationFile.IonosphericCorr = linie.Replace("GAL", "").Replace("IONOSPHERIC CORR", "");
                        }
                    }

                    if (linie.Contains("TIME SYSTEM CORR"))
                    {


                        if (linie.Contains("GAUT"))
                        {

                            NavigationFile.TimeSystemCorr = linie.Replace("GAUT", "").Replace("TIME SYSTEM CORR", "");
                        }
                    }

                }
            }

            return true;
        }
    }
}
