using Galileo.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Galileo
{
    public class Rinex
    {
        public Classes.RinexObservation ObservationFile { get; internal set; } = new RinexObservation();

        public Classes.RinexNavigation NavigationFile { get; internal set; } = new RinexNavigation();

        public void ReadFIle(string path)
        {
            string file = File.ReadAllText(path);

            List<string> linesHeader = file.Split("END OF HEADER", StringSplitOptions.RemoveEmptyEntries)[0].Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            // Load data from observation file
            if (linesHeader[0].Contains("OBSERVATION"))
            {
                ObservationFile.Type = Enums.Rinex.Types.Observation;
                string ObsTypes = null;

                foreach (string line in linesHeader)
                {
                    if (line.Contains("RINEX VERSION / TYPE"))
                    {
                        List<string> lineEdited = line.Replace("RINEX VERSION / TYPE", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                        ObservationFile.Version = float.Parse(lineEdited[0]);

                        if (lineEdited.Last().ToLower() == "galileo")
                            ObservationFile.SatelliteSystem = Enums.Rinex.SatelliteSystems.Galileo;

                        else if (lineEdited.Last().ToLower() == "mixed")
                            ObservationFile.SatelliteSystem = Enums.Rinex.SatelliteSystems.Mixed;

                        else
                            throw new Exception("No Galileo satellites in Rinex Observation");
                    }

                    else if (line.Contains("PGM / RUN BY / DATE"))
                    {
                        List<string> lineEdited = line.Replace("PGM / RUN BY / DATE", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                        ObservationFile.PGM = lineEdited[0];

                        if (lineEdited.Count > 5)
                            for (int i = 0; i < lineEdited.Count - 5; i++)
                            {
                                lineEdited.Remove(lineEdited.First());
                                ObservationFile.PGM += " " + lineEdited[0];
                            }

                        ObservationFile.RunBy = lineEdited[1];

                        ObservationFile.Date = DateTime.ParseExact(lineEdited[2] + " " + lineEdited[3], "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);
                        if (lineEdited.Last() == "UTC")
                            ObservationFile.Date = DateTime.SpecifyKind(ObservationFile.Date, DateTimeKind.Utc);
                        else
                            ObservationFile.Date = DateTime.SpecifyKind(ObservationFile.Date, DateTimeKind.Local);
                    }

                    else if (line.Contains("COMMENT"))
                    {
                        if (ObservationFile.Comments == null)
                            ObservationFile.Comments = line.Replace("COMMENT", "");
                        else
                            ObservationFile.Comments += "\n" + line.Replace("COMMENT", "");
                    }

                    else if (line.Contains("MARKER NAME"))
                    {
                        List<string> lineEdited = line.Replace("MARKER NAME", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        ObservationFile.Marker.Name = string.Join(' ', lineEdited);
                    }

                    else if (line.Contains("MARKER NUMBER"))
                    {
                        List<string> lineEdited = line.Replace("MARKER NUMBER", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        ObservationFile.Marker.Number = string.Join(' ', lineEdited);
                    }

                    else if (line.Contains("OBSERVER / AGENCY"))
                    {
                        List<string> lineEdited = line.Replace("OBSERVER / AGENCY", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        ObservationFile.Agency = lineEdited.Last();
                        lineEdited.Remove(lineEdited.Last());
                        ObservationFile.Observer = string.Join(' ', lineEdited);
                    }

                    // To add Marker Type

                    else if (line.Contains("REC # / TYPE / VERS"))
                    {
                        List<string> lineEdited = line.Replace("REC # / TYPE / VERS", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        ObservationFile.Receiver.Number = Convert.ToInt64(lineEdited[0]);
                        lineEdited.Remove(lineEdited.First());
                        ObservationFile.Receiver.Version = lineEdited.Last();
                        lineEdited.Remove(lineEdited.Last());
                        ObservationFile.Receiver.Type = string.Join(' ', lineEdited);
                    }

                    else if (line.Contains("ANT # / TYPE"))
                    {
                        List<string> lineEdited = line.Replace("ANT # / TYPE", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        ObservationFile.Antenna.Number = Convert.ToInt64(lineEdited[0]);
                        lineEdited.Remove(lineEdited.First());
                        ObservationFile.Antenna.Type = string.Join(' ', lineEdited);
                    }

                    else if (line.Contains("APPROX POSITION XYZ"))
                    {
                        List<string> lineEdited = line.Replace("APPROX POSITION XYZ", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        ObservationFile.Position.x = Convert.ToDouble(lineEdited[0]);
                        ObservationFile.Position.y = Convert.ToDouble(lineEdited[1]);
                        ObservationFile.Position.z = Convert.ToDouble(lineEdited[2]);
                    }

                    else if (line.Contains("ANTENNA: DELTA H/E/N"))
                    {
                        List<string> lineEdited = line.Replace("ANTENNA: DELTA H/E/N", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        ObservationFile.AntennaDeltaHEN.H = Convert.ToDouble(lineEdited[0]);
                        ObservationFile.AntennaDeltaHEN.E = Convert.ToDouble(lineEdited[1]);
                        ObservationFile.AntennaDeltaHEN.N = Convert.ToDouble(lineEdited[2]);
                    }

                    else if (line.Contains("SYS / # / OBS TYPES"))
                    {
                        if (ObsTypes == null)
                            ObsTypes = line.Replace("SYS / # / OBS TYPES", "");
                        else
                            ObsTypes += '\n' + line.Replace("SYS / # / OBS TYPES", "");
                    }

                    else if (line.Contains("SIGNAL STRENGTH UNIT"))
                        ObservationFile.SignalStrengthUnit = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

                    else if (line.Contains("INTERVAL"))
                        ObservationFile.Interval = Convert.ToDouble(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]);

                    else if (line.Contains("TIME OF FIRST OBS"))
                    {
                        List<string> lineEdited = line.Replace("TIME OF FIRST OBS", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        lineEdited.Remove(lineEdited.Last());
                        ObservationFile.TimeFirstOrbs = DateTime.ParseExact(string.Join(' ', lineEdited), "yyyy MM dd HH mm s.fffffff", CultureInfo.InvariantCulture);
                        ObservationFile.TimeFirstOrbs = DateTime.SpecifyKind(ObservationFile.TimeFirstOrbs, DateTimeKind.Unspecified);
                    }

                    else if (line.Contains("TIME OF LAST OBS"))
                    {
                        List<string> lineEdited = line.Replace("TIME OF LAST OBS", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        lineEdited.Remove(lineEdited.Last());
                        ObservationFile.TimeLastOrbs = DateTime.ParseExact(string.Join(' ', lineEdited), "yyyy MM dd HH mm s.fffffff", CultureInfo.InvariantCulture);
                        ObservationFile.TimeLastOrbs = DateTime.SpecifyKind(ObservationFile.TimeFirstOrbs, DateTimeKind.Unspecified);
                    }

                    else if (line.Contains("RCV CLOCK OFFS APPL"))
                    {
                        List<string> lineEdited = line.Replace("RCV CLOCK OFFS APPL", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        ObservationFile.RCVClock = Convert.ToBoolean(Convert.ToInt32(lineEdited[0]));
                    }

                    else if (line.Contains("SYS / PHASE SHIFT") && line.StartsWith("E"))
                        ObservationFile.PhaseShift = line.Replace("SYS / PHASE SHIFT", "");

                    else if (line.Contains("LEAP SECONDS"))
                    {
                        List<string> lineEdited = line.Replace("LEAP SECONDS", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        ObservationFile.LeapSeconds.CurrentNumber = Convert.ToInt32(lineEdited[0]);
                        ObservationFile.LeapSeconds.FuturePastLeaps = Convert.ToInt32(lineEdited[1]);
                        ObservationFile.LeapSeconds.WeekNumber = Convert.ToInt64(lineEdited[2]);
                        ObservationFile.LeapSeconds.DayNumber = Convert.ToInt32(lineEdited[3]);
                    }
                }

                string content = file.Split("END OF HEADER", StringSplitOptions.RemoveEmptyEntries)[1];

                List<string> records = content.Split('>', StringSplitOptions.RemoveEmptyEntries).ToList();
                records.Remove(records.First());

                foreach (string record in records)
                {
                    string dateLine = record.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0];
                    record ListRecord = new record();
                    List<string> date = dateLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                    ListRecord.EpochFlag = Convert.ToInt16(date[6]);
                    DateTime dataRef;
                    DateTime.TryParseExact(date[0] + " " + date[1] + " " + date[2] + " " + date [3] + " " + date[4] + " " + date[5], "yyyy MM dd HH mm s.fffffff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dataRef);
                    ListRecord.DateOfRecord = dataRef;
                    ListRecord.DateOfRecord = DateTime.SpecifyKind(ListRecord.DateOfRecord, DateTimeKind.Unspecified);

                    List<string> entries = record.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                    entries.Remove(entries.First());

                    foreach (string entry in entries)
                    {
                        if (entry.StartsWith("E"))
                        {
                            entry ListEntry = new entry();
                            List<string> stringDatas = entry.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                            ListEntry.Name = stringDatas[0];
                            stringDatas.Remove(stringDatas.First());

                            foreach (string stringData in stringDatas)
                            {
                                if (stringData.StartsWith("-."))
                                    ListEntry.Data.Add(Convert.ToDouble("-" + 0 + stringData.Substring(1)));
                                else if (stringData.StartsWith("."))
                                    ListEntry.Data.Add(Convert.ToDouble(0 + stringData.Substring(0)));
                                else
                                    ListEntry.Data.Add(Convert.ToDouble(stringData));
                            }
                            ListRecord.Satellites.Add(ListEntry);
                        }
                    }
                    ObservationFile.Entries.Add(ListRecord);
                }
            }

            // Load data from navigation file
            else if (linesHeader[0].Contains("NAV"))
            {

            }

            // TO ADD METEOROLOGICAL

            else
                throw new Exception("Invalid file to load");
        }

    }
}
