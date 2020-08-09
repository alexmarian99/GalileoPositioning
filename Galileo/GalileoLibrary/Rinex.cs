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

namespace Galileo
{
    public class Rinex
    {
        public Classes.RinexObservation ObservationFile { get; internal set; } = new RinexObservation();

        public Classes.RinexNavigation NavigationFile { get; internal set; } = new RinexNavigation();

        public double floatingToDouble (string floatingNum)
        {
            string[] value = floatingNum.Replace("D", " ").Split(" ", StringSplitOptions.RemoveEmptyEntries).ToArray();
            double doubleNum = double.Parse(value[0], System.Globalization.CultureInfo.InvariantCulture) * Math.Pow(10, double.Parse(value[1], System.Globalization.CultureInfo.InvariantCulture));
            return doubleNum;
        }
        public string addSpaces (string data)
        {
            for (int i = 1; i < data.Length; i++)
            {
                if ((data[i] == '+' || data[i] == '-') && data[i - 1] != 'D')
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

            List<string> linesHeader = file.Split("END OF HEADER",StringSplitOptions.RemoveEmptyEntries)[0].Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            // Load data from observation file
            if (linesHeader[0].Contains("OBSERVATION"))
            {

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
                        if (lineEdited.Count > 5)
                            for (int i=0; i < lineEdited.Count - 5; i++)
                            {
                                lineEdited.Remove(lineEdited.First());
                                NavigationFile.PGM += lineEdited[0];
                            }
                        NavigationFile.RunBy = lineEdited[3];
                        NavigationFile.Date = DateTime.ParseExact(lineEdited[2] + " " + lineEdited[3], "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);
                        if (lineEdited.Last() =="UTC")
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
                        List<string> lineEdited = line.Replace("GAL", "").Replace("IONOSPHERIC CORR", "").Split(" ",StringSplitOptions.RemoveEmptyEntries).ToList();
                        NavigationFile.IonosphericCorr.ai0 = floatingToDouble(lineEdited[0]);
                        NavigationFile.IonosphericCorr.ai1 = floatingToDouble(lineEdited[1]);
                        NavigationFile.IonosphericCorr.ai2 = floatingToDouble(lineEdited[2]);
                    }
                    else if (line.Contains("TIME SYSTEM CORR") && line.Contains("GAUT"))
                    {
                        List<string> lineEdited = line.Replace("GAUT", "").Replace("TIME SYSTEM CORR", "").Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        NavigationFile.TimeSystemCorr.a0 = floatingToDouble(lineEdited[0]);
                        NavigationFile.TimeSystemCorr.a1 = floatingToDouble(lineEdited[1]);
                        NavigationFile.TimeSystemCorr.t = double.Parse(lineEdited[2], System.Globalization.CultureInfo.InvariantCulture);
                        NavigationFile.TimeSystemCorr.w = double.Parse(lineEdited[3], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (line.Contains("LEAP SECONDS"))
                    {
                        List<string> lineEdited = line.Replace("LEAP SECONDS", "").Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        NavigationFile.Leapseconds.CurrentNumber = Convert.ToInt32(lineEdited[0]);
                        NavigationFile.Leapseconds.FuturePastLeaps = Convert.ToInt32(lineEdited[1]);
                        NavigationFile.Leapseconds.WeekNumber = Convert.ToInt64(lineEdited[2]);
                        NavigationFile.Leapseconds.DayNumber = Convert.ToInt32(lineEdited[3]);
                    }
                }
                string content = file.Split("END OF HEADER",StringSplitOptions.RemoveEmptyEntries)[1];
                string pattern = "(?=[GRECS])";
                List<string> satellites = Regex.Split(content, pattern).ToList();
                foreach (string sat in satellites)
                {
                    if (!sat.StartsWith("E"))
                        continue;
                    else
                    {
                        List<string> data = sat.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
                        List<string> dataRow = addSpaces(data[0]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData0 groupData0 = new GroupData0
                        {
                            a0 = floatingToDouble(dataRow[7]),
                            a1 = floatingToDouble(dataRow[8]),
                            a2 = floatingToDouble(dataRow[9])
                        };
                        List<string> DataRow = addSpaces(data[1]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData1 groupData1 = new GroupData1
                        {
                            IODnav = floatingToDouble(DataRow[0]),
                            Crs = floatingToDouble(DataRow[1]),
                            deltaN = floatingToDouble(DataRow[2]),
                            M0 = floatingToDouble(DataRow[3])
                        };
                        DataRow = addSpaces(data[2]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData2 groupData2 = new GroupData2
                        {
                            Cuc = floatingToDouble(DataRow[0]),
                            e = floatingToDouble(DataRow[1]),
                            Cus = floatingToDouble(DataRow[2]),
                            sqrtA = floatingToDouble(DataRow[3])
                        };
                        DataRow = addSpaces(data[3]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData3 groupData3 = new GroupData3
                        {
                            Toe = floatingToDouble(DataRow[0]),
                            Cic = floatingToDouble(DataRow[1]),
                            OMEGA = floatingToDouble(DataRow[2]),
                            Cis = floatingToDouble(DataRow[3])
                        };
                        DataRow = addSpaces(data[4]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData4 groupData4 = new GroupData4
                        {
                            i0 = floatingToDouble(DataRow[0]),
                            Crc = floatingToDouble(DataRow[1]),
                            omega = floatingToDouble(DataRow[2]),
                            OMEGADOT = floatingToDouble(DataRow[3])
                        };
                        DataRow = addSpaces(data[5]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData5 groupData5 = new GroupData5
                        {
                            IDOT = floatingToDouble(DataRow[0]),
                            CodesL2 = floatingToDouble(DataRow[1]),
                            Week = floatingToDouble(DataRow[2]),
                        };
                        DataRow = addSpaces(data[6]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        GroupData6 groupData6 = new GroupData6
                        {
                            SisaSignal = floatingToDouble(DataRow[0]),
                            SVhealth = floatingToDouble(DataRow[1]),
                            BGDa = floatingToDouble(DataRow[2]),
                            BGDb = floatingToDouble(DataRow[3])
                        };
                        DataRow = addSpaces(data[7]).Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                        EntryNavigation entry = new EntryNavigation
                        {
                            Name = dataRow[0],
                            Toc = DateTime.ParseExact(dataRow[1] + " " + dataRow[2] + " " + dataRow[3] + " " + dataRow[4] + " " + dataRow[5] + " " + dataRow[6], "yyyy MM dd HH mm ss", CultureInfo.InvariantCulture),
                            TransmissionTime = floatingToDouble(DataRow[0]),
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
                }
            }

            // TO ADD METEOROLOGICAL

            else
                throw new Exception("Invalid file to load");

            // SATELLITE POSITION 


        }

    }
}
