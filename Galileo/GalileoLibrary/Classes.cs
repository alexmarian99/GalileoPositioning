using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace Galileo.Classes
{
    /// <summary>
    /// The structure of a <b>Rinex Observation</b> file
    /// </summary>
    public class RinexObservation
    {
        /// <value>
        /// Format version
        /// </value>
        public float Version { get; internal set; }

        /// <summary>
        /// File type
        /// </summary>
        /// <value><b>O</b> for Observation Data</value>
        public Galileo.Enums.Rinex.Types Type { get; internal set; }

        /// <summary>
        /// Satellite System:
        /// </summary>
        /// <value>
        /// <list type="bullet">
        /// <item>GPS</item>
        /// <item>GLONASS</item>
        /// <item>Galileo</item>
        /// <item>QZSS</item>
        /// <item>BDS</item>
        /// <item>IRNSS</item>
        /// <item>SBAS</item>
        /// <item>Mixed (Multiple types of satellites used)</item>
        /// </list>
        /// </value>
        public Galileo.Enums.Rinex.SatelliteSystems SatelliteSystem { get; internal set; }

        /// <value>
        /// Name of program creating current file
        /// </value>
        public string PGM { get; internal set; }

        /// <value>
        /// Name of agency creating current file
        /// </value>
        public string RunBy { get; internal set; }

        /// <summary>
        /// Date and time of file creation
        /// </summary>
        /// <remarks>
        /// Zone = UTC / LCL (Local Time)
        /// </remarks>
        /// <value>
        /// yyyymmdd hhmmss zone
        /// </value>
        public DateTime Date { get; internal set; }

        /// <value>
        /// Comment lines
        /// </value>
        public string Comments { get; internal set; } = null;

        /// <summary>
        /// Antenna marker details
        /// </summary>
        public marker Marker { get; internal set; } = new marker();

        /// <value>
        /// Name of observer
        /// </value>
        public string Observer { get; internal set; }

        /// <value>
        /// Name of agency
        /// </value>
        public string Agency { get; internal set; }

        /// <summary>
        /// Receiver details
        /// </summary>
        public receiver Receiver { get; internal set; } = new receiver();

        /// <summary>
        /// Antenna details
        /// </summary>
        public antenna Antenna { get; internal set; } = new antenna();

        /// <value>
        /// Approx. position XYZ
        /// </value>
        public position Position { get; internal set; } = new position();

        /// <summary>
        /// Antenna Height and Eccentricity (All units in meters)
        /// </summary>
        public AntennaDeltaHENs AntennaDeltaHEN { get; internal set; } = new AntennaDeltaHENs();

        /// <summary>
        /// Number of columns and type of data recorded
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader>
        /// <term>Variable</term>
        /// <term>Value</term>
        /// </listheader>
        /// <item>
        /// <term>Observation type</term>
        /// <term>C = pseudorange</term>
        /// <term>L = carrier phase</term>
        /// <term>D = doppler</term>
        /// <term>S = signal strength</term>
        /// </item>
        /// <item>
        /// <term>Band / Frequency</term>
        /// <term>1, 2, ..., 8</term>
        /// </item>
        /// <item>
        /// <term>Attribute</term>
        /// <term>tracking mode or channel, e.g., I, Q, etc.</term>
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>C1C</example>
        public string ObsType { get; internal set; }

        /// <summary>
        /// Unit of the carrier to noise ratio observables 
        /// </summary>
        /// <value>Snn (if present) DBHZ : S/N given in dbHz</value>
        public string SignalStrengthUnit { get; internal set; }

        /// <summary>
        /// Observation interval
        /// </summary>
        /// <value>seconds</value>
        public double Interval { get; internal set; }

        /// <summary>
        /// Time of first observation record
        /// </summary>
        /// <value>yyyy mm  dd  hh  mm  ss  Type</value>
        public string TimeFirstOrbs { get; internal set; }

        /// <summary>
        /// Time of last observation
        /// </summary>
        /// <value>yyyy mm  dd  hh  mm  ss  Type</value>
        public string TimeLastOrbs { get; internal set; }

        /// <summary>
        /// Epoch, code, and phase are corrected by applying the realtime-derived receiver clock offset:
        /// </summary>
        /// <value>true=yes, false=no; default: false</value>
        public bool RCVClock { get; internal set; } = false;

        /// <summary>
        /// Phase shift correction used to generate phases consistent w/r to cycle shifts
        /// <list type="bullet">
        /// <item>Carrier phase observation code: Type Band Attribute</item>
        /// <item>Correction applied (cycles)</item>
        /// <item>Number of satellites involved 0 or blank: All satellites of system</item>
        /// <item>List of satellites</item>
        /// </list>
        /// </summary>
        public string PhaseShift { get; internal set; }

        /// <summary>
        /// Leap seconds details
        /// </summary>
        public leapseconds LeapSeconds { get; internal set; } = new leapseconds();

        /// <summary>
        /// Records from satellites
        /// </summary>
        public List<record> Entries { get; internal set; } = new List<record>();

    }

    public class marker
    {
        /// <value>
        /// Name of antenna marker
        /// </value>
        public string Name { get; internal set; }

        /// <value>
        /// Number of antenna marker
        /// </value>
        public string Number { get; internal set; }

        /// <summary>
        /// Type of the marker
        /// </summary>
        public Galileo.Enums.Rinex.MarkerTypes Type { get; internal set; }
    }

    public class receiver
    {
        /// <value>
        /// Receiver number
        /// </value>
        public long Number { get; internal set; }

        /// <value>
        /// Receiver type
        /// </value>
        public string Type { get; internal set; }

        /// <value>
        /// Receiver version
        /// </value>
        public string Version { get; internal set; }
    }

    public class antenna
    {
        /// <value>
        /// Antenna number
        /// </value>
        public long Number { get; internal set; }

        /// <value>
        /// Antenna type
        /// </value>
        public string Type { get; internal set; }
    }

    public class position
    {
        /// <value>
        /// Units: Meters, System: ITRS recommended
        /// </value>
        public double x { get; internal set; }

        /// <summary>
        /// Units: Meters, System: ITRS recommended
        /// </summary>
        public double y { get; internal set; }

        /// <summary>
        /// Units: Meters, System: ITRS recommended
        /// </summary>
        public double z { get; internal set; }
    }

    public class AntennaDeltaHENs
    {
        /// <summary>
        /// Antenna height: Height of the antenna reference point(ARP) above the marker
        /// </summary>
        /// <value>Meters</value>
        public double H { get; internal set; }

        /// <summary>
        /// Horizontal eccentricity of ARP relative to the marker(east)
        /// </summary>
        /// <value>Meters</value>
        public double E { get; internal set; }

        /// <summary>
        /// Horizontal eccentricity of ARP relative to the marker(north)
        /// </summary>
        /// <value>Meters</value>
        public double N { get; internal set; }
    }

    public class leapseconds
    {
        /// <summary>
        /// Current number of leap seconds
        /// </summary>
        public int CurrentNumber { get; internal set; }

        /// <summary>
        /// Future or past leap seconds ΔtLSF(BNK) , i.e. future leap second if the week and day number are in the future.
        /// </summary>
        public int FuturePastLeaps { get; internal set; }

        /// <summary>
        /// weeks since 6-Jan-1980 
        /// </summary>
        public long WeekNumber { get; internal set; }

        /// <summary>
        /// The day number is the day before the leap second
        /// </summary>
        public int DayNumber { get; internal set; }
    }

    public class entry
    {
        /// <summary>
        /// Name of satellite
        /// </summary>
        /// <value>E##</value>
        public string Name { get; internal set; }

        /// <summary>
        /// Data array
        /// </summary>
        public List<double> Data { get; internal set; }
    }

    public class record
    {
        /// <summary>
        /// The date the satellites were recorded
        /// </summary>
        public DateTime DateOfRecord { get; internal set; }

        /// <value>
        /// <para>0: OK</para>
        /// <para>1: power failure between previous and current epoch</para>
        /// <para>>1: Special event</para>
        /// </value>
        public short EpochFlag { get; internal set; }

        /// <summary>
        /// Satellites recorded at that time
        /// </summary>
        public List<entry> Satellites { get; internal set; } = new List<entry>();

    }

    /// <summary>
    /// The structure of a <b>Rinex Navigation</b> file
    /// </summary>
    public class RinexNavigation
    {
        /// <summary>
        /// Rinex Version
        /// </summary>
        public float Version { get; internal set; }


        /// <summary>
        /// File type
        /// </summary>
        /// <value><b>N</b> for Navigation Data</value>
        public Galileo.Enums.Rinex.Types Type { get; internal set; }

        /// <summary>
        /// Satellite System:
        /// </summary>
        /// <value>
        /// <list type="bullet">
        /// <item>GPS</item>
        /// <item>GLONASS</item>
        /// <item>Galileo</item>
        /// <item>QZSS</item>
        /// <item>BDS</item>
        /// <item>IRNSS</item>
        /// <item>SBAS</item>
        /// <item>Mixed (Multiple types of satellites used)</item>
        /// </list>
        /// </value>
        public Galileo.Enums.Rinex.SatelliteSystems SateliteSystem { get; internal set; }

        /// <value>
        /// Name of program creating current file
        /// </value>
        public string PGM { get; internal set; }

        /// <value>
        /// Name of agency creating current file
        /// </value>
        public string RunBy { get; internal set; }

        /// <summary>
        /// Date and time of file creation
        /// </summary>
        /// <remarks>
        /// Zone = UTC / LCL (Local Time)
        /// </remarks>
        /// <value>
        /// yyyymmdd hhmmss zone
        /// </value>
        public DateTime Date { get; internal set; }

        /// <summary>
        /// Ionospheric Correction
        /// </summary>
        public IonosphericCorrs IonosphericCorr { get; internal set; } = new IonosphericCorrs();

        /// <summary>
        /// Time System Correction
        /// </summary>
        public TimeSystemCorrs TimeSystemCorr { get; internal set; } = new TimeSystemCorrs();

        public leapseconds Leapseconds { get; internal set; } = new leapseconds();

        /// <summary>
        /// Navigation data entries
        /// </summary>
        public List<EntryNavigation> Entries { get; internal set; } = new List<EntryNavigation>();
    }

    public class IonosphericCorrs
    {
        /// <summary>
        /// 
        /// </summary>
        public double ai0;

        /// <summary>
        /// 
        /// </summary>
        public double ai1;

        /// <summary>
        /// 
        /// </summary>
        public double ai2;
    }

    public class TimeSystemCorrs
    {
        /// <summary>
        /// Coefficients of 1-deg polynomial
        /// </summary>
        /// <value>seconds</value>
        public double a0;

        /// <summary>
        /// Coefficients of 1-deg polynomial
        /// </summary>
        /// <value>sec/sec</value>
        public double a1;

        /// <summary>
        /// Reference time for polynomial
        /// </summary>
        /// <value>Seconds into GAL Week</value>
        public double t;

        /// <summary>
        /// Reference week number
        /// </summary>
        public double w;
    }
    public class EntryNavigation
    {
        /// <summary>
        /// Name of satellite
        /// </summary>
        /// <value>E##</value>
        public string Name { get; internal set; }

        /// <summary>
        /// Time of clock (GAL)
        /// </summary>
        public DateTime Toc { get; internal set; }

        /// <summary>
        /// Polynomial coefficients for clock correction
        /// </summary>
        public GroupData0 Group0 { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public GroupData1 Group1 { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public GroupData2 Group2 { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public GroupData3 Group3 { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public GroupData4 Group4 { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public GroupData5 Group5 { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public GroupData6 Group6 { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public double TransmissionTime { get; internal set; }
    }

    public class GroupData0
    {
        /// <summary>
        /// CLock bias (s)
        /// </summary>
        public double a0;

        /// <summary>
        /// Clock drift (s/s)
        /// </summary>
        public double a1;

        /// <summary>
        /// drift rate (aging) (s/s2))
        /// </summary>
        public double a2;
    }
    public class GroupData1
    {
        /// <summary>
        /// Issue of data, ephemeris (IODE)
        /// </summary>
        public double IODnav;

        /// <summary>
        /// Amplitude of the sine harmonic correction term to the orbit radius (m)
        /// </summary>
        /// <value>meters</value>
        public double Crs;

        /// <summary>
        /// Mean motion difference from computed value (semicircles/s)
        /// </summary>
        /// <value>radians/sec</value>
        public double deltaN;

        /// <summary>
        /// Mean anomaly at reference time (semicircles)
        /// </summary>
        /// <value>radians</value>
        public double M0;
    }

    public class GroupData2
    {
        /// <summary>
        /// Amplitude of the cosine harmonic correction term to the argument of latitude (rad)
        /// </summary>
        /// <value>radians</value>
        public double Cuc;

        /// <summary>
        /// Eccentricity (dimensionless)
        /// </summary>
        public double e;

        /// <summary>
        /// Amplitude of the sine harmonic correction term to the argument of latitude (rad)
        /// </summary>
        /// <value>radians</value>
        public double Cus;

        /// <summary>
        /// Square root of the semi-major axis (m1/2)
        /// </summary>
        public double sqrtA;
    }

    public class GroupData3
    {
        /// <summary>
        /// Toe Time of Ephemeris
        /// </summary>
        /// <value>sec of GAL week</value>
        public double Toe;

        /// <summary>
        /// Amplitude of the cosine harmonic correction term to the angle of inclination (rad)
        /// </summary>
        /// <value>radians</value>
        public double Cic;

        /// <summary>
        /// Longitude of ascending node at reference time (semicircles)
        /// </summary>
        /// <value>radians</value>
        public double OMEGA;

        /// <summary>
        /// Amplitude of the sine harmonic correction term to the angle of inclination (rad)
        /// </summary>
        /// <value>radians</value>
        public double Cis;
    }

    public class GroupData4
    {
        /// <summary>
        /// Inclination angle at reference time (semicircles)
        /// </summary>
        /// <value>radians</value>
        public double i0;

        /// <summary>
        /// Amplitude of the cosine harmonic correction term to the orbit radius (m)
        /// </summary>
        /// <value>meters</value>
        public double Crc;

        /// <summary>
        /// Argument of perigee (semicircles)
        /// </summary>
        /// <value>radians</value>
        public double omega;

        /// <summary>
        /// Rate of change of right ascension (semicircles/s)
        /// </summary>
        /// <value>radians/sec</value>
        public double OMEGADOT;
    }

    public class GroupData5
    {
        /// <summary>
        /// Rate of change of inclination (semicircles/s)
        /// </summary>
        /// <value>radians/sec</value>
        public double IDOT;

        /// <summary>
        /// Biti de verificat pentru validarea datelor
        /// </summary>
        public double CodesL2;

        /// <summary>
        /// To use with T0E
        /// </summary>
        public double Week;

    }

    public class GroupData6
    {
        /// <summary>
        /// SISA Signal in space accuracy
        /// Undefined/Unknown: -1
        /// </summary>
        /// <value>meters</value>
        public double SisaSignal;

        /// <summary>
        /// See Galileo ICD Section 5.1.9.3 (Rinex303.pdf) p75
        /// </summary>
        public double SVhealth;

        /// <summary>
        /// BGD E5a/E1
        /// </summary>
        /// <value>Seconds</value>
        public double BGDa;

        /// <summary>
        /// BGD E5b/E1
        /// </summary>
        /// <value>Seconds</value>
        public double BGDb;
    }
}
