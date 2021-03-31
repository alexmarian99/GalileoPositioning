using Galileo.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Galileo
{
    public class Rinex
    {
        public Classes.RinexObservation ObservationFile { get; internal set; } = new RinexObservation();

        public Classes.RinexNavigation NavigationFile { get; internal set; } = new RinexNavigation();

        public string addSpaces (string data)
        {
            for (int i = 1; i < data.Length; i++)
            {
                if ((data[i] == '+' || data[i] == '-') && data[i - 1] != 'E')
                {
                    data = data.Insert(i, " ");
                    i++;
                }
            }
            return data; 
        }
        public void ReadFIle(string path)
        {
            string file = File.ReadAllText(path);

            List<string> linesHeader = file.Split("END OF HEADER", StringSplitOptions.RemoveEmptyEntries)[0].Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();

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

                        else if (lineEdited.Last().ToLower() == "mixed" || lineEdited.Last().ToLower() == "m")
                            ObservationFile.SatelliteSystem = Enums.Rinex.SatelliteSystems.Mixed;

                        else
                            throw new Exception("No Galileo satellites in Rinex Observation");
                    }

                    else if (line.Contains("PGM / RUN BY / DATE"))
                    {
                        List<string> lineEdited = line.Replace("PGM / RUN BY / DATE", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                        ObservationFile.PGM = lineEdited[0];

                        //ObservationFile.RunBy = lineEdited[1];

                        ObservationFile.Date = DateTime.ParseExact(lineEdited[1] + " " + lineEdited[2], "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);
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
                        ObservationFile.Antenna.Number = Convert.ToInt64(lineEdited[0] == "Unknown" ? "0" : lineEdited[0]);
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
                        ObservationFile.TimeFirstOrbs = DateTime.ParseExact(string.Join(' ', lineEdited), "yyyy M dd H mm ss.fffffff", CultureInfo.InvariantCulture);
                        ObservationFile.TimeFirstOrbs = DateTime.SpecifyKind(ObservationFile.TimeFirstOrbs, DateTimeKind.Unspecified);
                    }

                    else if (line.Contains("TIME OF LAST OBS"))
                    {
                        List<string> lineEdited = line.Replace("TIME OF LAST OBS", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        lineEdited.Remove(lineEdited.Last());
                        ObservationFile.TimeLastOrbs = DateTime.ParseExact(string.Join(' ', lineEdited), "yyyy M dd H mm s.fffffff", CultureInfo.InvariantCulture);
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
                    if (!record.Contains("COMMENT"))
                    {
                        string dateLine = record.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)[0];
                        record ListRecord = new record();
                        List<string> date = dateLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        ListRecord.EpochFlag = Convert.ToInt16(date[6]);
                        DateTime dataRef;
                        DateTime.TryParseExact(date[0] + " " + date[1] + " " + date[2] + " " + date[3] + " " + date[4] + " " + date[5], "yyyy MM dd HH mm ss.fffffff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dataRef);
                        ListRecord.Epoch = dataRef;
                        ListRecord.Epoch = DateTime.SpecifyKind(ListRecord.Epoch, DateTimeKind.Unspecified);

                        List<string> entries = record.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
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
                                        ListEntry.Data.Add(Convert.ToDouble("-" + 0 + stringData.Substring(1), CultureInfo.InvariantCulture));
                                    else if (stringData.StartsWith("."))
                                        ListEntry.Data.Add(Convert.ToDouble(0 + stringData.Substring(0), CultureInfo.InvariantCulture));
                                    else
                                        ListEntry.Data.Add(Convert.ToDouble(stringData, CultureInfo.InvariantCulture));
                                }
                                ListRecord.Satellites.Add(ListEntry);
                            }
                        }
                        ObservationFile.Entries.Add(ListRecord);
                    }
                }
            }

            // Load data from navigation file
            else if (linesHeader[0].Contains("NAV"))
            {
                NavigationFile.Type = Enums.Rinex.Types.Navigation;
                foreach(string line in linesHeader)
                {
                    if (line.Contains("RINEX VERSION / TYPE"))
                    {
                        List<string> lineEdited = line.Replace("RINEX VERSION / TYPE", "").Split(" ",StringSplitOptions.RemoveEmptyEntries).ToList();
                        NavigationFile.Version = float.Parse(lineEdited[0]);
                        if (lineEdited.Last().ToLower() == "galileo")
                        {
                            NavigationFile.SateliteSystem = Enums.Rinex.SatelliteSystems.Galileo;
                        }
                        else if (lineEdited.Last().ToLower() == "mixed")
                        {
                            NavigationFile.SateliteSystem = Enums.Rinex.SatelliteSystems.Mixed;
                        }
                    }
                    else if (line.Contains("PGM / RUN BY / DATE"))
                    {
                        List<string> lineEdited = line.Replace("PGM / RUN BY / DATE", "").Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        NavigationFile.PGM = lineEdited[0];
                        /*
                        if (lineEdited.Count > 5)
                            for (int i=0; i < lineEdited.Count - 5; i++)
                            {
                                lineEdited.Remove(lineEdited.First());
                                NavigationFile.PGM += lineEdited[0];
                            }
                        */
                        //NavigationFile.RunBy = lineEdited[3];
                        NavigationFile.Date = DateTime.ParseExact(lineEdited[1] + " " + lineEdited[2], "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);
                        if (lineEdited.Contains("UTC"))
                        {
                            NavigationFile.Date = DateTime.SpecifyKind(NavigationFile.Date, DateTimeKind.Utc);
                        }
                        else
                        {
                            NavigationFile.Date = DateTime.SpecifyKind(NavigationFile.Date, DateTimeKind.Local);
                        }
                    }
                    else if (line.Contains("IONOSPHERIC CORR") && line.Contains("GAL"))
                    {
                        string spacedLINE = addSpaces(line);
                        List<string> lineEdited = spacedLINE.Replace("GAL", "").Replace("IONOSPHERIC CORR", "").Split(" ",StringSplitOptions.RemoveEmptyEntries).ToList();
                        NavigationFile.IonosphericCorr.ai0 = Convert.ToDouble(lineEdited[0]);
                        NavigationFile.IonosphericCorr.ai1 = Convert.ToDouble(lineEdited[1]);
                        NavigationFile.IonosphericCorr.ai2 = Convert.ToDouble(lineEdited[2]);
                    }
                    else if (line.Contains("TIME SYSTEM CORR") && line.Contains("GAUT"))
                    {
                        string spacedLine = addSpaces(line);
                        List<string> lineEdited = spacedLine.Replace("GAUT", "").Replace("TIME SYSTEM CORR", "").Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        NavigationFile.TimeSystemCorr.a0 = Convert.ToDouble(lineEdited[0]);
                        NavigationFile.TimeSystemCorr.a1 = Convert.ToDouble(lineEdited[1]);
                        NavigationFile.TimeSystemCorr.t = double.Parse(lineEdited[2], System.Globalization.CultureInfo.InvariantCulture);
                        NavigationFile.TimeSystemCorr.w = double.Parse(lineEdited[3], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (line.Contains("LEAP SECONDS"))
                    {
                        string spacedLine = addSpaces(line);
                        List<string> lineEdited = spacedLine.Replace("LEAP SECONDS", "").Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        NavigationFile.Leapseconds.CurrentNumber = Convert.ToInt32(lineEdited[0]);
                        //NavigationFile.Leapseconds.FuturePastLeaps = Convert.ToInt32(lineEdited[1]);
                        //NavigationFile.Leapseconds.WeekNumber = Convert.ToInt64(lineEdited[2]);
                        //NavigationFile.Leapseconds.DayNumber = Convert.ToInt32(lineEdited[3]);
                    }
                }
                string content = file.Split("END OF HEADER",StringSplitOptions.RemoveEmptyEntries)[1];
                List<string> satellites = Regex.Split(content, "\r\nE").ToList();
                satellites.RemoveAt(0);
                foreach (string sat in satellites)
                {
                        List<string> data = sat.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
                        List<string> dataRow = addSpaces(data[0]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData0 groupData0 = new GroupData0
                        {
                            a0 = Convert.ToDouble(dataRow[7]),
                            a1 = Convert.ToDouble(dataRow[8]),
                            a2 = Convert.ToDouble(dataRow[9])
                        };
                        List<string> DataRow = addSpaces(data[1]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData1 groupData1 = new GroupData1
                        {
                            IODnav = Convert.ToDouble(DataRow[0]),
                            Crs = Convert.ToDouble(DataRow[1]),
                            deltaN = Convert.ToDouble(DataRow[2]),
                            M0 = Convert.ToDouble(DataRow[3])
                        };
                        DataRow = addSpaces(data[2]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData2 groupData2 = new GroupData2
                        {
                            Cuc = Convert.ToDouble(DataRow[0]),
                            e = Convert.ToDouble(DataRow[1]),
                            Cus = Convert.ToDouble(DataRow[2]),
                            sqrtA = Convert.ToDouble(DataRow[3])
                        };
                        DataRow = addSpaces(data[3]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData3 groupData3 = new GroupData3
                        {
                            Toe = Convert.ToDouble(DataRow[0]),
                            Cic = Convert.ToDouble(DataRow[1]),
                            OMEGA = Convert.ToDouble(DataRow[2]),
                            Cis = Convert.ToDouble(DataRow[3])
                        };
                        DataRow = addSpaces(data[4]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData4 groupData4 = new GroupData4
                        {
                            i0 = Convert.ToDouble(DataRow[0]),
                            Crc = Convert.ToDouble(DataRow[1]),
                            omega = Convert.ToDouble(DataRow[2]),
                            OMEGADOT = Convert.ToDouble(DataRow[3])
                        };
                        DataRow = addSpaces(data[5]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData5 groupData5 = new GroupData5
                        {
                            IDOT = Convert.ToDouble(DataRow[0]),
                            CodesL2 = Convert.ToDouble(DataRow[1]),
                            Week = Convert.ToDouble(DataRow[2]),
                        };
                        DataRow = addSpaces(data[6]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData6 groupData6 = new GroupData6
                        {
                            SisaSignal = Convert.ToDouble(DataRow[0]),
                            SVhealth = Convert.ToDouble(DataRow[1]),
                            BGDa = Convert.ToDouble(DataRow[2]),
                            BGDb = Convert.ToDouble(DataRow[3])
                        };
                        DataRow = addSpaces(data[7]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        EntryNavigation entry = new EntryNavigation
                        {
                            Name = "E" + dataRow[0],
                            Toc = DateTime.ParseExact(dataRow[1] + " " + dataRow[2] + " " + dataRow[3] + " " + dataRow[4] + " " + dataRow[5] + " " + dataRow[6], "yyyy MM dd HH mm ss", CultureInfo.InvariantCulture),
                            TransmissionTime = Convert.ToDouble(DataRow[0]),
                            Group0 = groupData0,
                            Group1 = groupData1,
                            Group2 = groupData2,
                            Group3 = groupData3,
                            Group4 = groupData4,
                            Group5 = groupData5,
                            Group6 = groupData6
                        };
                        NavigationFile.Entries.Add(entry);
                }
                Console.WriteLine("ok");
            }

            // TO ADD METEOROLOGICAL

            else
                throw new Exception("Invalid file to load");

            // SATELLITE POSITION 


        }

    }
}
