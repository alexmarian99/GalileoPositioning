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
        static void Main(string[] args)
        {

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            //Constante
            double GM = 3.986005 * Math.Pow(10, 14);
            double omegaE = 7.292115 * Math.Pow(10, -5);
            //var pi = Math.PI;


            //Code
            Rinex fisier = new Rinex();
            fisier.ReadFIle(@"D:\navData.rnx");
            fisier.ReadFIle(@"D:\M0SE00ITA_R_20201970000_01D_30S_MO.rnx");
            DateTime reper = DateTime.SpecifyKind(DateTime.ParseExact("06.01.1980 00:00:00", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture), DateTimeKind.Unspecified);

            Console.WriteLine();

            // 6 Jan 1980 00:00:00
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


            /*var sat1 = CalculateSatPosition(fisier.NavigationFile.Entries[84]); var pseudo1 = fisier.ObservationFile.Entries[60].Satellites[0].Data[1];
            var sat2 = CalculateSatPosition(fisier.NavigationFile.Entries[83]); var pseudo2 = fisier.ObservationFile.Entries[60].Satellites[1].Data[1];
            var sat3 = CalculateSatPosition(fisier.NavigationFile.Entries[82]); var pseudo3 = fisier.ObservationFile.Entries[60].Satellites[2].Data[1];
            var sat4 = CalculateSatPosition(fisier.NavigationFile.Entries[81]); var pseudo4 = fisier.ObservationFile.Entries[60].Satellites[3].Data[1];
            var sat5 = CalculateSatPosition(fisier.NavigationFile.Entries[90]); var pseudo5 = fisier.ObservationFile.Entries[60].Satellites[5].Data[1];


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
            Console.WriteLine(Convert.ToDecimal(X[0, 2]));*/

            // Exemplu din documentatie

            double[] sat1 = new double[3] { 28573624.909, 176258.719, 475886.493 }; double pseudo1 = 25573786.094;
            double[] sat2 = new double[3] { 20534972.474, 3620869.695, 20821515.054 }; double pseudo2 = 23269991.712;
            double[] sat3 = new double[3] { 13834909.426, 9331764.237, 24705373.313 }; double pseudo3 = 23527045.278;
            double[] sat4 = new double[3] { -18325015.195, 12831313.778, 20831862.073 }; double pseudo4 = 29205487.559;
            double[] sat5 = new double[3] { -11441576.697, 19817392.158, 15998439.113 }; double pseudo5 = 26129807.790;

            Console.WriteLine(CalculateTi(sat1, pseudo1));
            Console.WriteLine("159654526737584.50");

        }
    }
}
