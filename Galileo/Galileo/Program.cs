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
using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;
using MathNet.Numerics;
using System.Security;
using System.Data.Common;
using MathNet.Numerics.Integration;
using Accord.Math;
using MathNet.Numerics.LinearAlgebra.Complex;
using System.Windows;
using MathNet.Numerics.LinearAlgebra.Complex32;
using System.Diagnostics;
using Syncfusion.XlsIO;
using System.Data;

namespace Galileo
{
    class Program
    {

        /*class Sat
        {
            public entry Observation;
            public EntryNavigation Navigation;
        }*/

        static void Main(string[] args)
        {
            //Constante
            double GM = 3.986005 * Math.Pow(10, 14);
            double omegaE = 7.2921151467 * Math.Pow(10, -5);
            var pi = Math.PI;

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            //Code
            Rinex fisier = new Rinex();
            fisier.ReadFIle(@"D:\GNSS\nav.rnx");
            fisier.ReadFIle(@"D:\GNSS\obs.rnx");

            getPosition(fisier.ObservationFile, fisier.NavigationFile);

            // Conversie din Epoch in JulDate
            // Meeus, Jean (1991) Astronomical Algorithms, Willmann-Bell, Richmond, Virginia, p. 59--62
            double julday(DateTime date)
            {
                double y = date.Year;
                double m = date.Month;
                double d = date.Day;
                double h = date.Hour + (double)date.Minute / 60 + (double)date.Second / 3600;
                //Jan and Feb are considered to be the 13th and 14th month of the preceding year
                if (m <= 2)
                {
                    y -= 1;
                    m += 12;
                }
                //Julian day number 0 assigned to the day starting at noon on Monday, January 1, 4713 BC
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
                //GPS standard epoch, JD = 2,444,244.5 (January 6th, 1980, 00:00 UTC)
                double week = Math.Floor((julday - 2444244.5) / 7);
                // We add + 1 as the GPS week starts at Saturday midnight
                double sec_of_week = Math.Round(((d % 1) + day_of_week + 1) * 86400);
                return new double[2] { week, sec_of_week };
            }

            double check_t(double t)
            {
                //halfWeek = 302400;
                if (t > 302400)
                    t -= 604800;
                if (t < -302400)
                    t += 604800;
                return t;
            }

            //Computation of satellite coordinates (X,Y,Z) at time t
            //Strang G, Borre K (1997) Linear algebra, geodesy, and GPS - pag 482-487
            double[] satPos(double t, EntryNavigation entry)
            {
                double A = entry.Group2.sqrtA * entry.Group2.sqrtA;
                double tk = check_t(t - entry.Group3.Toe);
                double n0 = Math.Sqrt(GM / Math.Pow(A, 3));
                double n = n0 + entry.Group1.deltaN;
                double M = entry.Group1.M0 + n * tk;
                M = (M + 2 * pi) % (2 * pi);
                double E = M;
                for (int j = 0; j < 10; j++)
                {
                    double E_old = E;
                    E = M + entry.Group2.e * Math.Sin(E);
                    double dE = (E - E_old) % (2 * pi);
                    if (Math.Abs(dE) < 1.2E-12)
                        break;
                }
                E = (E + 2 * pi) % (2 * pi);
                double v = Math.Atan2(Math.Sqrt(1 - Math.Pow(entry.Group2.e, 2)) * Math.Sin(E), Math.Cos(E) - entry.Group2.e);
                double phi = v + entry.Group4.omega;
                phi = phi % (2 * pi);
                double u = phi + entry.Group2.Cuc * Math.Cos(2 * phi) + entry.Group2.Cus * Math.Sin(2 * phi);
                double r = A * (1 - entry.Group2.e * Math.Cos(E)) + entry.Group4.Crc * Math.Cos(2 * phi) + entry.Group1.Crs * Math.Sin(2 * phi);
                double i = entry.Group4.i0 + entry.Group5.IDOT * tk + entry.Group3.Cic * Math.Cos(2 * phi) + entry.Group3.Cis * Math.Sin(2 * phi);
                double Omega = entry.Group3.OMEGA + (entry.Group4.OMEGADOT - omegaE) * tk - omegaE * entry.Group3.Toe;
                Omega = (Omega + 2 * pi) % (2 * pi);
                double x1 = Math.Cos(u) * r;
                double y1 = Math.Sin(u) * r;
                double x = x1 * Math.Cos(Omega) - y1 * Math.Cos(i) * Math.Sin(Omega);
                double y = x1 * Math.Sin(Omega) + y1 * Math.Cos(i) * Math.Cos(Omega);
                double z = y1 * Math.Sin(i);
                return new double[3]
                {
                    x, y, z
                };
            }

            //Returns rotated satellite ECEF coordinates due to Earth rotation during signal travel time
            //Strang G, Borre K (1997) Linear algebra, geodesy, and GPS. - pag 492
            double[] e_r_corr(double travelTime, double[] Xsat)
            {
                double omegaTau = omegaE * travelTime;
                double[,] R3 = new double[3, 3] { { Math.Cos(omegaTau), Math.Sin(omegaTau), 0 }, { -Math.Sin(omegaTau), Math.Cos(omegaTau), 0 }, { 0, 0, 1 } };
                double[] XsatRot = { 0, 0, 0 };
                for (int i = 0; i < 3; i++)
                {
                    XsatRot[0] += R3[0, i] * Xsat[i];
                    XsatRot[1] += R3[1, i] * Xsat[i];
                    XsatRot[2] += R3[2, i] * Xsat[i];
                }
                /*
                var R3 = Extreme.Mathematics.Matrix.Create(3, 3, new double[]
                    {
                     Math.Cos(omegaTau), Math.Sin(omegaTau), 0,
                     -Math.Sin(omegaTau), Math.Cos(omegaTau), 0,
                     0, 0 , 1
                }, MatrixElementOrder.ColumnMajor);
                var XsatRot = R3.Multiply(Xsat);
                */
                return new double[3]
                {
                    XsatRot[0], XsatRot[1], XsatRot[2]
                };
            }

            //calculate geodetic coordinates
            double[] toGeod(double a, double finv, double x, double y, double z)
            {
                double h = 0;
                double tolsq = 1E-10;
                double maxIt = 10;
                double rtd = 180 / pi;
                double esq;
                if (finv < 1E-20)
                    esq = 0;
                else
                    esq = (2 - 1 / finv) / finv;
                double oneesq = 1 - esq;
                double P = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
                double dlambda;
                if (P > 1e-20)
                    dlambda = Math.Atan2(y, x) * rtd;
                else
                    dlambda = 0;
                if (dlambda < 0)
                    dlambda += 360;
                double r = Math.Sqrt(Math.Pow(P, 2) + Math.Pow(z, 2));
                double sinPhi;
                if (r > 1e-20)
                    sinPhi = z / r;
                else
                    sinPhi = 0;
                double dPhi = Math.Asin(sinPhi);
                if (r < 1e-20)
                {
                    h = 0;
                    return new double[3]
                    {
                        dPhi, dlambda, h
                    };
                }

                h = r - a * (1 - sinPhi * sinPhi / finv);
                for (int i = 0; i < maxIt; i++)
                {
                    sinPhi = Math.Sin(dPhi);
                    double cosPhi = Math.Cos(dPhi);
                    double nPhi = a / Math.Sqrt(1 - esq * sinPhi * sinPhi);
                    double dP = P - (nPhi + h) * cosPhi;
                    double dZ = z - (nPhi * oneesq + h) * sinPhi;
                    h += sinPhi * dZ + cosPhi * dP;
                    dPhi += (cosPhi * dZ - sinPhi * dP) / (nPhi + h);
                    if (dP * dP + dZ * dZ < tolsq)
                        break;
                }
                dPhi *= rtd;
                return new double[3]
                {
                    dPhi, dlambda, h
                };
            }

            //Transformation of vector dx into topocentric coordinate system with origin at X.
            double[] topocent(double[] x, double[] dx)
            {
                double dtr = pi / 180;
                double[] data = toGeod(6378137, 298.257223563, x[0], x[1], x[2]);
                double phi = data[0];
                double lambda = data[1];
                double c1 = Math.Cos(lambda * dtr);
                double s1 = Math.Sin(lambda * dtr);
                double cb = Math.Cos(phi * dtr);
                double sb = Math.Sin(phi * dtr);
                double[,] F = new double[3, 3];
                F[0, 0] = -s1;
                F[0, 1] = -sb * c1;
                F[0, 2] = cb * c1;
                F[1, 0] = c1;
                F[1, 1] = -sb * s1;
                F[1, 2] = cb * s1;
                F[2, 0] = 0;
                F[2, 1] = cb;
                F[2, 2] = sb;
                double[,] Ftrans = F.Transpose();
                double[] vect = { 0, 0, 0 };
                for (int i = 0; i < 3; i++)
                {
                    vect[0] += Ftrans[0, i] * dx[i];
                    vect[1] += Ftrans[1, i] * dx[i];
                    vect[2] += Ftrans[2, i] * dx[i];
                }
                /*
                                var F = Extreme.Mathematics.Matrix.Create(3, 3, new double[]
                                {
                                   -s1, -sb*c1, cb*c1,
                                   c1, -sb*s1, cb*s1,
                                   0, cb, sb
                                }, MatrixElementOrder.ColumnMajor);
                                var vect = F.GetInverse().Multiply(dx);
                */
                double E = vect[0];
                double N = vect[1];
                double U = vect[2];
                double horDis = Math.Sqrt(Math.Pow(E, 2) + Math.Pow(N, 2));
                double Az;
                double El;
                if (horDis < 1e-20)
                {
                    Az = 0;
                    El = 90;
                }
                else
                {
                    Az = Math.Atan2(E, N) / dtr;
                    El = Math.Atan2(U, horDis) / dtr;
                }
                if (Az < 0)
                    Az += 360;
                double D = Math.Sqrt(Math.Pow(dx[0], 2) + Math.Pow(dx[1], 2) + Math.Pow(dx[2], 2));
                return new double[3]
                {
                    Az, El, D
                };
            }

            //Calculation of tropospheric correction.
            double tropo(double sinel, double hsta, double p, double tkel, double hum, double hp, double htkel, double hhum)
            {
                double ae = 6378137;
                double b0 = 7.839257e-5;
                double tlapse = -6.5;
                double tkhum = tkel + tlapse * (hhum - htkel);
                double atkel = 7.5 * (tkhum - 273.15) / (237.3 + tkhum - 273.15);
                double e0 = 0.0611 * hum * Math.Pow(10, atkel);
                double tksea = tkel - tlapse * htkel;
                double em = -978.77 / (2.8704e6 * tlapse * 1e-5);
                double tkelh = tksea + tlapse * hhum;
                double e0sea = e0 * Math.Pow((tksea / tkelh), 4 * em);
                double tkelp = tksea + tlapse * hp;
                double psea = p * Math.Pow((tksea / tkelp), em);
                if (sinel < 0)
                    sinel = 0;
                double tropo = 0;
                bool done = false;
                double refsea = 77.624e-6 / tksea;
                double htop = 1.1385e-5 / refsea;
                refsea *= psea;
                double Ref = refsea * Math.Pow((htop - hsta) / htop, 4);
                for (; ; )
                {
                    double rtop = Math.Pow((ae + htop), 2) - Math.Pow(ae + hsta, 2) * (1 - Math.Pow(sinel, 2));
                    if (rtop < 0)
                        rtop = 0;
                    rtop = Math.Sqrt(rtop) - (ae + hsta) * sinel;
                    double a = -sinel / (htop - hsta);
                    double b = -b0 * (1 - Math.Pow(sinel, 2)) / (htop - hsta);
                    double[] rn = new double[10];
                    for (int i = 0; i < 8; i++)
                        rn[i] = Math.Pow(rtop, i + 2);
                    double[] alpha = { 2 * a, 2 * Math.Pow(a, 2) + 4 * b / 3, a * (Math.Pow(a, 2) + 3 * b), Math.Pow(a, 4) / 5 + 2.4 * Math.Pow(a, 2) * b + 1.2 * Math.Pow(b, 2), 2 * a * b * (Math.Pow(a, 2) + 3 * b) / 3, Math.Pow(b, 2) * (Math.Pow(a, 2) * 6 + 4 * b) * 1.428571e-1, 0, 0 };
                    if (Math.Pow(b, 2) > 1e-35)
                    {
                        alpha[6] = a * Math.Pow(b, 3) / 2;
                        alpha[7] = Math.Pow(b, 4) / 9;
                    }
                    double dr = rtop;
                    for (int i = 0; i < 8; i++)
                        dr += alpha[i] * rn[i];
                    tropo += dr * Ref * 1000;
                    if (done == true)
                        return tropo;
                    done = true;
                    refsea = (371900e-6 / tksea - 12.92e-6) / tksea;
                    htop = 1.1385e-5 * (1255 / tksea + 0.05) / refsea;
                    Ref = refsea * e0sea * Math.Pow((htop - hsta) / htop, 4);
                }
            }

            //Computation of receiver position from pseudoranges using ordinary least-squares principle
            double[] recpo_ls(record obsData, double time, RinexNavigation eph)
            {
                //obs[] primul pseudorange de la toti satelitii dintr o epoca
                double[] pos = { 0, 0, 0, 0 };
                //double GDOP;
                double v_light = 299792458;
                double dtr = pi / 180;
                int n = 0;
                int[] sat = new int[obsData.Satellites.Count];
                for (int i = 0; i < obsData.Satellites.Count; i++)
                {
                    for (int j = 0; j < eph.Entries.Count; j++)
                        if (gps_time(julday(eph.Entries[j].Toc))[1] == time && obsData.Satellites[i].Name == eph.Entries[j].Name)
                        {
                            sat.SetValue(i, n);
                            n++;
                            break;
                        }
                }
                if (n < 4)
                    return new double[4]
                    {
                       0,0,0,0
                    };
                // Console.WriteLine(obsData.Epoch);
                // Console.WriteLine(n);
                // Console.WriteLine("++++++++++++++");
                double[] El = new double[n];
                //var omc = Extreme.Mathematics.Matrix.Create(n, 1, new double[0], MatrixElementOrder.ColumnMajor);
                //var A = Extreme.Mathematics.Matrix.Create(n, 4, new double[0], MatrixElementOrder.ColumnMajor);
                double[,] A = new double[n, 4];
                double[,] omc = new double[n, 1];
                for (int iter = 0; iter < 6; iter++)
                {
                    // A.Clear();
                    // omc.Clear();
                    for (int i = 0; i < n; i++)
                    {

                        //cautare eph corespunzatoare sat din obs file
                        double travelTime;
                        double rho2;
                        for (int j = 0; j < eph.Entries.Count; j++)
                        {
                            if (gps_time(julday(eph.Entries[j].Toc))[1] == time && obsData.Satellites[sat[i]].Name == eph.Entries[j].Name)
                            {
                                double txRaw = time - obsData.Satellites[sat[i]].Data[0] / v_light;
                                double dt = check_t(txRaw - eph.Entries[j].Group3.Toe);
                                double tcorr = (eph.Entries[j].Group0.a2 * dt + eph.Entries[j].Group0.a1 * dt + eph.Entries[j].Group0.a0);
                                double txGAL = txRaw - tcorr;
                                dt = check_t(txGAL - eph.Entries[j].Group3.Toe);
                                tcorr = (eph.Entries[j].Group0.a2 * dt + eph.Entries[j].Group0.a1 * dt + eph.Entries[j].Group0.a0);
                                txGAL = txRaw - tcorr;
                                double[] X = satPos(txGAL, eph.Entries[j]);
                                double[] rotX;
                                double trop;
                                if (iter == 0)
                                {
                                    travelTime = 0.072;
                                    rotX = X;
                                    trop = 0;
                                }
                                else
                                {
                                    rho2 = Math.Pow(X[0] - pos[0], 2) + Math.Pow(X[1] - pos[1], 2) + Math.Pow(X[2] - pos[2], 2);
                                    travelTime = Math.Sqrt(rho2) / v_light;
                                    rotX = e_r_corr(travelTime, X);
                                    rho2 = Math.Pow(rotX[0] - pos[0], 2) + Math.Pow(rotX[1] - pos[1], 2) + Math.Pow(rotX[2] - pos[2], 2);
                                    double el = topocent(new double[] { pos[0], pos[1], pos[2] }, new double[] { rotX[0] - pos[0], rotX[1] - pos[1], rotX[2] - pos[2] })[1];
                                    if (iter == 5)
                                        El[i] = el;
                                    trop = tropo(Math.Sin(el * dtr), 0.0, 1013.0, 293.0, 50.0, 0.0, 0.0, 0.0);
                                }
                                omc.SetValue(obsData.Satellites[sat[i]].Data[0] - Norm.Frobenius(new double[,] { { rotX[0] - pos[0], rotX[1] - pos[1], rotX[2] - pos[2] }, { 0, 0, 0 } }) - pos[3] + v_light * tcorr - trop, i, 0);
                                //double val =  Math.Sqrt(Math.Pow(rotX[0] - pos[0], 2) + Math.Pow(rotX[1] - pos[1], 2) + Math.Pow(rotX[2] - pos[2], 2)) - pos[4] + v_light * tcorr - trop;
                                //omc.SetValue(val, i, 0);
                                A.SetValue((-(rotX[0] - pos[0])) / obsData.Satellites[sat[i]].Data[0], i, 0);
                                A.SetValue((-(rotX[1] - pos[1])) / obsData.Satellites[sat[i]].Data[0], i, 1);
                                A.SetValue((-(rotX[2] - pos[2])) / obsData.Satellites[sat[i]].Data[0], i, 2);
                                A.SetValue(1, i, 3);
                                // Console.WriteLine("{0} | {1} | {2} | {3}",A.GetValue(i,0),A.GetValue(i,1),A.GetValue(i,2),A.GetValue(i,3));
                            }
                            else
                                continue;
                            break;
                        }
                    }
                    double[] x = { 0, 0, 0, 0 };
                    double[,] Ainv = A.PseudoInverse();
                    for (int i = 0; i < omc.Length; i++)
                    {
                        x[0] += Ainv[0, i] * omc[i, 0];
                        x[1] += Ainv[1, i] * omc[i, 0];
                        x[2] += Ainv[2, i] * omc[i, 0];
                        x[3] += Ainv[3, i] * omc[i, 0];
                    }

                    /*double[,] x = A.PseudoInverse().Multiply(omc);
                     pos[0] += Convert.ToDouble(x.GetValue(0, 0));
                     pos[1] += Convert.ToDouble(x.GetValue(0, 1));
                     pos[2] += Convert.ToDouble(x.GetValue(0, 2));
                     pos[3] += Convert.ToDouble(x.GetValue(0, 3));*/
                    pos[0] += x[0];
                    pos[1] += x[1];
                    pos[2] += x[2];
                    pos[3] += x[3];
                }
                //  Console.WriteLine("----------------");
                return pos;
            }
            //conversion of degrees to degrees, mins, secs
            double[] deg2dms(double deg)
            {
                bool negArg = false;
                if (deg < 0)
                {
                    negArg = true;
                    deg = -deg;
                }
                double intDeg = Math.Floor(deg);
                double Decimal = deg - intDeg;
                double minPart = Decimal * 60;
                double min = Math.Floor(minPart);
                double secPart = minPart - min;
                double sec = secPart * 60;
                if (sec == 60)
                {
                    min++;
                    sec = 0;
                }
                if (min == 60)
                {
                    intDeg++;
                    min = 0;
                }
                if (negArg == true)
                    intDeg = -intDeg;
                return new double[3]
                {
                    intDeg,min,sec
                };
            }

            void cart2geo(double x, double y, double z)
            {
                double a = 6378137;
                double f = 1 / 298.257223563;
                double lambda = Math.Atan2(y, x);
                double ex2 = (2 - f) * f / Math.Pow(1 - f, 2);
                double c = a * Math.Sqrt(1 + ex2);
                double phi = Math.Atan(z / ((Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) * (1 - (2 - f)) * f)));
                double h = 0.1;
                double oldH = 0;
                double N;
                while (Math.Abs(h - oldH) > 1e-13)
                {
                    oldH = h;
                    N = c / Math.Sqrt(1 + ex2 * Math.Pow(Math.Cos(phi), 2));
                    phi = Math.Atan(z / ((Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) * (1 - (2 - f) * f * N / (N + h)))));
                    h = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) / Math.Cos(phi) - N;
                }
                double[] phiRes = deg2dms(phi * 180 / pi);
                double[] lambdaRes = deg2dms(lambda * 180 / pi);
                Console.WriteLine("lat : {0}° {1}' {2}\"", phiRes[0], phiRes[1], phiRes[2]);
                Console.WriteLine("long : {0}° {1}' {2}\"", lambdaRes[0], lambdaRes[1], lambdaRes[2]);
            }

            void getPosition(RinexObservation Obsfile, RinexNavigation eph)
            {
                int noOfEpochs = Obsfile.Entries.Count;
                // Console.WriteLine(noOfEpochs);
                double[,] Pos = new double[4, noOfEpochs];
                double[] pos;

                int i = -1;
                foreach (record ObsData in Obsfile.Entries)
                {

                    double timeOfWeek = gps_time(julday(ObsData.Epoch))[1];
                    pos = recpo_ls(ObsData, timeOfWeek, eph);
                    if (pos[0] != 0)
                    {
                        i++;
                        Pos.SetValue(pos[0], 0, i);
                        Console.WriteLine(pos[0]);
                        Pos.SetValue(pos[1], 1, i);
                        Pos.SetValue(pos[2], 2, i);
                        Pos.SetValue(pos[3], 3, i);
                    }
                }

                for (int j = 0; j < 120; j++)
                {
                   // Console.WriteLine("{0}   |   {1}   |   {2}", Pos.GetRow(0)[j], Pos.GetRow(1)[j], Pos.GetRow(2)[j]);
                }

                List<double> lista = Pos.GetRow(0).ToList();
                lista.RemoveAll(x => x < 4600000);
                lista.RemoveAll(x => x > 4800000);
                double x = lista.Average();

                lista = Pos.GetRow(1).ToList();
                lista.RemoveAll(x => x < 1010000);
                lista.RemoveAll(x => x > 1030000);
                double y = lista.Average();

                lista = Pos.GetRow(2).ToList();
                lista.RemoveAll(x => x < 4200000);
                lista.RemoveAll(x => x > 4500000);
                double z = lista.Average();

                Console.WriteLine("x : {0}", x);
                Console.WriteLine("y : {0}", y);
                Console.WriteLine("z : {0}", z);

                cart2geo(x, y, z);

                File.WriteAllLines(@"D:\GNSS\x.txt", Pos.GetRow(0).Select(d => d.ToString()));
                File.WriteAllLines(@"D:\GNSS\y.txt", Pos.GetRow(1).Select(d => d.ToString()));
                File.WriteAllLines(@"D:\GNSS\z.txt", Pos.GetRow(2).Select(d => d.ToString()));
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
