using Galileo.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Threading;

namespace Galileo
{
    class Program
    {

        class Sat
        {
            public entry Observation;
            public EntryNavigation Navigation;
        }

        static void Main(string[] args)
        {

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            //Code
            Rinex fisier = new Rinex();
            fisier.ReadFIle(@"D:\navData.rnx");
            fisier.ReadFIle(@"D:\M0SE00ITA_R_20201970000_01D_30S_MO.rnx");
            DateTime reper = DateTime.SpecifyKind(DateTime.ParseExact("06.01.1980 00:00:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture), DateTimeKind.Unspecified);

            Sat sat1 = new Sat()
            {
                Observation = fisier.ObservationFile.Entries[60].Satellites[0],
                Navigation = fisier.NavigationFile.Entries[84]
            };

            Sat sat2 = new Sat()
            {
                Observation = fisier.ObservationFile.Entries[60].Satellites[1],
                Navigation = fisier.NavigationFile.Entries[83]
            };

            Sat sat3 = new Sat()
            {
                Observation = fisier.ObservationFile.Entries[60].Satellites[2],
                Navigation = fisier.NavigationFile.Entries[82]
            };

            Sat sat4 = new Sat()
            {
                Observation = fisier.ObservationFile.Entries[60].Satellites[3],
                Navigation = fisier.NavigationFile.Entries[81]
            };

            Sat sat5 = new Sat()
            {
                Observation = fisier.ObservationFile.Entries[60].Satellites[5],
                Navigation = fisier.NavigationFile.Entries[90]
            };

            List<Sat> Satellites = new List<Sat>();
            Satellites.Add(sat1);
            Satellites.Add(sat2);
            Satellites.Add(sat3);
            Satellites.Add(sat4);
            Satellites.Add(sat5);


            var julcalc = julday(fisier.ObservationFile.Entries[60].Epoch);
            Console.WriteLine(julcalc);
            Console.WriteLine(gps_time(julcalc)[1]);
            
            // Conversie din Epoch in JulDate
            double julday(DateTime date)
            {
                double y = date.Year;
                double m = date.Month;
                double d = date.Day;
                double h = date.Hour + (double)date.Minute / 60 + (double)date.Second / 3600;
                
                if (m <= 2)
                {
                    y -= 1;
                    m += 12;
                }

                return Math.Floor(365.25 * (y + 4716)) + Math.Floor(30.6001 * (m + 1)) + d + h / 24 - 1537.5;
            }
            
            //Conversie in GPS Time ( return double[2] = { week,  sec_of_week  }  )
            double[] gps_time(double julday)
            {
                double a = Math.Floor(julday + .5);
                double b = a + 1537;
                double c = Math.Floor((b - 122.1) / 365.25);
                double e = Math.Floor(365.25 * c);
                double f = Math.Floor((b - e) / 30.6001);
                double d = b - e - Math.Floor(30.6001 * f) + (julday + .5) % 1;
                double day_of_week = (Math.Floor(julday + .5)) % 7;
                double week = Math.Floor((julday - 2444244.5) / 7);

                // We add + 1 as the GPS week starts at Saturday midnight
                double sec_of_week = ((d % 1) + day_of_week + 1) * 86400;

                return new double[2] { week, sec_of_week };
            }

            #region codeVechi

            /*// 6 Jan 1980 00:00:00
             * //Constante
            double GM = 3.986005 * Math.Pow(10, 14);
            double omegaE = 7.292115 * Math.Pow(10, -5);
            //var pi = Math.PI;
            double[] CalculateSatPosition(EntryNavigation entry)
            {
                var totalDaysFrom1980 = (entry.Toc - reper).TotalDays;

                var dayOfWeek = totalDaysFrom1980 % 7;

                var secondsInTheWeek = dayOfWeek * 86400;


                // Time from current ephemeris epoch
                double tk = secondsInTheWeek - entry.Group3.Toe;
                if (tk > 302400)
                    tk -= 604800;
                else if (tk < -302400)
                    tk += 604800;
                //Console.WriteLine(tk);
                //Console.WriteLine(Convert.ToDecimal(tk));





                //EccentricAnomally
                // Computed mean motion
                double a = Math.Pow(entry.Group2.sqrtA, 2);
                double n0 = Math.Sqrt(GM / Math.Pow(a, 3));

                // Corrected mean motion
                double n = n0 + entry.Group1.deltaN;

                // Mean anomally
                double Mk = entry.Group1.M0 + n * tk;

                // Solve Kepler's Equation for Eccentric Anomally
                double Ek = Mk;
                for (; ; )
                {
                    double temp = Ek;
                    Ek = Mk + entry.Group2.e * Math.Sin(Ek);
                    if (Math.Abs(Ek - temp) < 1e-10) break;
                }
                // return Ek



                //True anomally
                double vk = Math.Atan2(Math.Sqrt(1 - entry.Group2.e * entry.Group2.e) * Math.Sin(Ek), Math.Cos(Ek) - entry.Group2.e);

                // Argument of lattitude
                double AOL = vk * entry.Group4.omega;

                // Second Harmonic Perturbation
                double duk = entry.Group2.Cus * Math.Sin(2 * AOL) + entry.Group2.Cuc * Math.Cos(2 * AOL); // Argument of lattitude correction
                double drk = entry.Group1.Crs * Math.Sin(2 * AOL) + entry.Group4.Crc * Math.Cos(2 * AOL); // Radius Correction
                double dik = entry.Group3.Cis * Math.Sin(2 * AOL) + entry.Group3.Cic * Math.Cos(2 * AOL); // Inclination Correction

                // Corrected Argument of lattitude , Radius & Inclination
                double uk = AOL + duk;
                double rk = a * (1 - entry.Group2.e * Math.Cos(Ek)) + drk;
                double ik = entry.Group4.i0 + dik + entry.Group5.IDOT * tk;

                // Position in orbital plane
                double x_kp = rk * Math.Cos(uk);
                double y_kp = rk * Math.Sin(uk);

                // Corrected longitude of ascending mode
                double OmegaK = entry.Group3.OMEGA + (entry.Group4.OMEGADOT - omegaE) * tk - omegaE * entry.Group3.Toe;

                // Earth fixed coordinates
                double x = x_kp * Math.Cos(OmegaK) - y_kp * Math.Cos(ik) * Math.Sin(OmegaK);
                double y = x_kp * Math.Sin(OmegaK) - y_kp * Math.Cos(ik) * Math.Cos(OmegaK);
                double z = y_kp * Math.Sin(ik);

                return new double[3]
                {
                    x, y, z
                };
            }

            static double CalculateTi(double[] sat, double pseudorange)
            {
                return (Math.Pow(sat[0], 2) + Math.Pow(sat[1], 2) + Math.Pow(sat[2], 2)) - Math.Pow(pseudorange, 2);
            }


            var sat1 = CalculateSatPosition(fisier.NavigationFile.Entries[84]); var pseudo1 = fisier.ObservationFile.Entries[60].Satellites[0].Data[0];
            var sat2 = CalculateSatPosition(fisier.NavigationFile.Entries[83]); var pseudo2 = fisier.ObservationFile.Entries[60].Satellites[1].Data[0];
            var sat3 = CalculateSatPosition(fisier.NavigationFile.Entries[82]); var pseudo3 = fisier.ObservationFile.Entries[60].Satellites[2].Data[0];
            var sat4 = CalculateSatPosition(fisier.NavigationFile.Entries[81]); var pseudo4 = fisier.ObservationFile.Entries[60].Satellites[3].Data[0];
            var sat5 = CalculateSatPosition(fisier.NavigationFile.Entries[90]); var pseudo5 = fisier.ObservationFile.Entries[60].Satellites[5].Data[0];


            Matrix<double> B = DenseMatrix.OfArray(new double[,]
            {
                { CalculateTi(sat2, pseudo2) - CalculateTi(sat1, pseudo1) },
                { CalculateTi(sat3, pseudo3) - CalculateTi(sat1, pseudo1) },
                { CalculateTi(sat4, pseudo4) - CalculateTi(sat1, pseudo1) },
                { CalculateTi(sat5, pseudo5) - CalculateTi(sat1, pseudo1) }
            });

            Matrix<double> A = DenseMatrix.OfArray(new double[,]
            {
                { sat2[0] - sat1[0], sat2[1] - sat1[1], sat2[2] - sat1[2], pseudo1 - pseudo2 },
                { sat3[0] - sat1[0], sat3[1] - sat1[1], sat3[2] - sat1[2], pseudo1 - pseudo3  },
                { sat4[0] - sat1[0], sat4[1] - sat1[1], sat4[2] - sat1[2], pseudo1 - pseudo4  },
                { sat5[0] - sat1[0], sat5[1] - sat1[1], sat5[2] - sat1[2], pseudo1 - pseudo5  }
            });
            Console.WriteLine(Convert.ToDecimal(sat2[0] - sat1[0]));
            var X = B.Transpose().Multiply(0.5).Multiply(A.Inverse());
            Console.WriteLine(A.Inverse());
            Console.WriteLine(B.Transpose());
            Console.WriteLine(B.Transpose().Multiply(0.5));
            Console.WriteLine(X);
            Console.WriteLine(Convert.ToDecimal(X[0, 0]));
            Console.WriteLine(Convert.ToDecimal(X[0, 1]));
            Console.WriteLine(Convert.ToDecimal(X[0, 2]));
            Console.WriteLine(Convert.ToDecimal(X[0, 3])); // 59316429.1916321

            Console.WriteLine();
            Console.WriteLine(ecef2lla(X[0, 0], X[0, 1], X[0, 2])[0]);
            Console.WriteLine(ecef2lla(X[0, 0], X[0, 1], X[0, 2])[1]);
            Console.WriteLine(ecef2lla(X[0, 0], X[0, 1], X[0, 2])[2]);
        }

            // Exemplu din documentatie

            /*double[] sat1 = new double[3] { 28573624.909, 176258.719, 475886.493 }; double pseudo1 = 25573786.094;
            double[] sat2 = new double[3] { 20534972.474, 3620869.695, 20821515.054 }; double pseudo2 = 23269991.712;
            double[] sat3 = new double[3] { 13834909.426, 9331764.237, 24705373.313 }; double pseudo3 = 23527045.278;
            double[] sat4 = new double[3] { -18325015.195, 12831313.778, 20831862.073 }; double pseudo4 = 29205487.559;
            double[] sat5 = new double[3] { -11441576.697, 19817392.158, 15998439.113 }; double pseudo5 = 26129807.790;

            Console.WriteLine(CalculateTi(sat1, pseudo1, 0));
            Console.WriteLine("159654526737584.50");*/
            #endregion

        }
    }
}
