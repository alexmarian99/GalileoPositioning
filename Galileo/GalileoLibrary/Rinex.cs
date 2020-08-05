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
                        linie.Replace("OBSERVER / AGENCY", "");
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        ObservationFile.Observer = param[0];
                        ObservationFile.Agency = param[1];

                    }

                    else if (linie.Contains("REC # / TYPE / VERS"))
                    {
                        linie.Replace("REC # / TYPE / VERS", "");
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        ObservationFile.Receiver.Number = Convert.ToInt64(param[0]);
                        ObservationFile.Receiver.Type = param[1] + param[2];
                        string[] parametru = regex.Replace(param[3], " ").Split("/");
                        ObservationFile.Receiver.Version = parametru[0] + " " + parametru[1];

                    }

                    else if (linie.Contains("ANT # / TYPE"))
                    {
                        linie.Replace("ANT # / TYPE", "");
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        ObservationFile.Antenna.Number = Convert.ToInt64(param[0]);
                        ObservationFile.Antenna.Type = param[1];

                    }

                    else if (linie.Contains("APPROX POSITION XYZ"))
                    {
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        ObservationFile.Position.x = Convert.ToDouble(param[1]);
                        ObservationFile.Position.y = Convert.ToDouble(param[2]);
                        ObservationFile.Position.z = Convert.ToDouble(param[3]);
                    }

                    else if (linie.Contains("ANTENNA: DELTA H/E/N"))
                    {
                        linie.Replace("ANTENNA: DELTA H/E/N", "");

                        string[] param = regex.Replace(linie, " ").Split(" ");
                        //ObservationFile.Antenna.Number = Convert.ToDouble(param[1]);
                        //ObservationFile.Antenna.Type = Convert.ToDouble(param[2]);??



                    }

                    else if (linie.Contains("SYS / # / OBS TYPES"))
                    {
                        linie.Replace("SYS / # / OBS TYPES", "");
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        if (param[0].Contains('E'))
                        {//de lucrat


                        }

                    }

                    else if (linie.Contains("SIGNAL STRENGTH UNIT"))
                    {
                        linie.Replace("SIGNAL STRENGTH UNIT", "");
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        ObservationFile.SignalStrengthUnit = param[0];

                    }

                    else if (linie.Contains("INTERVAL"))
                    {
                        linie.Replace("INTERVAL", "");
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        ObservationFile.Interval = Convert.ToDouble(param[1]);

                    }

                    else if (linie.Contains("TIME OF FIRST OBS"))
                    {
                        linie.Replace("TIME OF FIRST OBS", "");

                        linie.Replace("GPS", "");//??

                        string[] param = regex.Replace(linie, " ").Split(" ");
                        // ObservationFile.TimeFirstOrbs = DateTime.SpecifyKind(DateTime.ParseExact(param[1] + param[2] + param[3] + param[4] + param[5] + param[6], "yyyyMMddHHmmss", CultureInfo.InvariantCulture), DateTimeKind.Utc);

                    }

                    else if (linie.Contains("TIME OF LAST OBS"))
                    {
                        linie.Replace("TIME OF LAST OBS", "");

                        linie.Replace("GPS", "");//??

                        string[] param = regex.Replace(linie, " ").Split(" ");
                        // ObservationFile.TimeFirstOrbs = DateTime.SpecifyKind(DateTime.ParseExact(param[1] + param[2] + param[3] + param[4] + param[5] + param[6], "yyyyMMddHHmmss", CultureInfo.InvariantCulture), DateTimeKind.Utc);

                    }

                    else if (linie.Contains("SYS / PHASE SHIFT"))
                    {
                        linie.Replace("SYS / PHASE SHIFT", "");
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        if (linie.Contains('E'))
                        {
                            ObservationFile.PhaseShift = param[2];
                            //SYS??
                        }

                    }

                    else if (linie.Contains("LEAP SECONDS"))
                    {
                        linie.Replace("LEAP SECONDS", "");
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        ObservationFile.LeapSeconds.CurrentNumber = Convert.ToInt32(param[1]);
                        ObservationFile.LeapSeconds.FuturePastLeaps = Convert.ToInt32(param[2]);
                        ObservationFile.LeapSeconds.WeekNumber = Convert.ToInt32(param[3]);


                    }
                }

                string continut = fisier.Split("END OF HEADER")[1];
                List<string> records = continut.Split("> ").ToList();
                records.Remove(records.First());

                foreach (string intrare in records)
                {
                    List<string> satelites = intrare.Split('\n').ToList();
                    string[] dateArray = regex.Replace(satelites[0], " ").Split(" ");
                    satelites.Remove(satelites.First());
                    string dateTime = dateArray[0] + " " + dateArray[1] + " " + dateArray[2] + " " + dateArray[3] + " " + dateArray[4] + " " + dateArray[5];
                    Classes.record toAdd = new record();
                    toAdd.DateOfRecord = DateTime.SpecifyKind(DateTime.ParseExact(dateTime, "yyyy MM dd HH mm s.fffffff", CultureInfo.InvariantCulture), DateTimeKind.Unspecified);

                    foreach (string sat in satelites)
                    {
                        List<string> DATA = regex.Replace(sat, " ").Split(" ").ToList();
                        if (!DATA[0].Contains("E"))
                            continue;
                        string name = DATA[0];
                        List<double> values = new List<double>();
                        DATA.Remove(DATA.First());
                        foreach (string value in DATA)
                        {
                            double temp;
                            Double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out temp);
                            values.Add(temp);
                        }
                        toAdd.Satellites.Add(new entry()
                        {
                            Name = name,
                            Data = values
                        });
                        toAdd.Satellites.Add(new entry()
                        {
                            Name = name,
                            Data = values
                        });
                    }
                    ObservationFile.Entries.Add(toAdd);
                }

            }

            else if (fisier.Contains("NAV DATA"))
            {
                NavigationFile = new Classes.RinexNavigation();
                string header = fisier.Split("END OF HEADER")[0];
                string[] liniiHeader = header.Split('\n');
                foreach (string linie in liniiHeader)
                {
                    if (linie.Contains("RINEX VERSION / TYPE"))
                    {
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        NavigationFile.Version = float.Parse(param[1]);
                        if (param[7].ToLower() == "mixed")
                        {
                            NavigationFile.Type = Enums.Rinex.Types.Mixed;
                        }
                        else if (param[7].ToLower() == "galileo")
                        {
                            NavigationFile.Type = Enums.Rinex.Types.Galileo;
                        }
                        else
                            return false;
                    }
                    else if (linie.Contains("PGM / RUN BY / DATE"))
                    {
                        string[] param = regex.Replace(linie, " ").Split(" ");
                        NavigationFile.PGM = param[0] + " " + param[1];
                        NavigationFile.RunBy = param[2];
                        NavigationFile.Date = DateTime.SpecifyKind(DateTime.ParseExact(param[3] + param[4], "yyyyMMddHHmmss", CultureInfo.InvariantCulture), DateTimeKind.Utc);
                    }
                    else if (linie.Contains("IONOSPHERIC CORR"))
                        NavigationFile.IonosphericCorr = linie.Replace("GAL", "").Replace("IONOSPHERIC CORR", "");

                    else if (linie.Contains("TIME SYSTEM CORR"))
                            NavigationFile.TimeSystemCorr = linie.Replace("GAUT", "").Replace("TIME SYSTEM CORR", "");

                }
            }

            return true;
        }
    }
}
