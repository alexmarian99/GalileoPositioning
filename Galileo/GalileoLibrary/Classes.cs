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
        /// Rinex Version
        /// </value>
        public float Version { get; internal set; }

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
        public Galileo.Enums.Rinex.Types Type { get; internal set; }

        /// <value>
        /// Name of the program that created this file
        /// </value>
        public string PGM { get; internal set; }

        /// <value>
        /// Name of agency that created this file
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
        public receiver Receiver { get; internal set; }

        /// <summary>
        /// Antenna details
        /// </summary>
        public antenna Antenna { get; internal set; }

        /// <value>
        /// Approx. position XYZ
        /// </value>
        public position Position { get; internal set; }

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
        /// Observation interval in seconds
        /// </summary>
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
        public leapseconds LeapSeconds { get; internal set; }

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
        public double x { get; internal set; }
        public double y { get; internal set; }
        public double z { get; internal set; }
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
        public double[] Data { get; internal set; }
    }

    public class record
    {
        /// <summary>
        /// The date the satellites were recorded
        /// </summary>
        public DateTime DateOfRecord { get; internal set; }

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
        public string Version { get; internal set; }

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
        public Galileo.Enums.Rinex.Types Type { get; internal set; }

        /// <value>
        /// Name of the program that created this file
        /// </value>
        public string PGM { get; internal set; }

        /// <value>
        /// Name of agency that created this file
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
        public string IonosphericCorr { get; internal set; }

        /// <summary>
        /// Time System Correction
        /// </summary>
        public string TimeSystemCorr { get; internal set; }

        /// <summary>
        /// Navigation data entries
        /// </summary>
        public List<EntryNavigation> Entries { get; internal set; } = new List<EntryNavigation>();
    }

    public class EntryNavigation
    {
        /// <summary>
        /// Name of satellite
        /// </summary>
        /// <value>E##</value>
        public string Name { get; internal set; }

        /// <summary>
        /// The date the satellite were recorded (T
        /// </summary>
        public DateTime Date { get; internal set; }

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
        /// Issue of data, ephemeris 
        /// </summary>
        public double Iode;

        /// <summary>
        /// Amplitude of the sine harmonic correction term to the orbit radius (m)
        /// </summary>
        public double Crs;

        /// <summary>
        /// Mean motion difference from computed value (semicircles/s)
        /// </summary>
        public double deltaN;

        /// <summary>
        /// Mean anomaly at reference time (semicircles)
        /// </summary>
        public double M0;
    }

    public class GroupData2
    {
        /// <summary>
        /// Amplitude of the cosine harmonic correction term to the argument of latitude (rad)
        /// </summary>
        public double Cuc;

        /// <summary>
        /// Eccentricity (dimensionless)
        /// </summary>
        public double e;

        /// <summary>
        /// Amplitude of the sine harmonic correction term to the argument of latitude (rad)
        /// </summary>
        public double Cus;

        /// <summary>
        /// Square root of the semi-major axis (m1/2)
        /// </summary>
        public double sqrtA;
    }

    public class GroupData3
    {
        /// <summary>
        /// Ephemeris reference time 
        /// </summary>
        public double T0e;

        /// <summary>
        /// Amplitude of the cosine harmonic correction term to the angle of inclination (rad)
        /// </summary>
        public double Cic;

        /// <summary>
        /// Longitude of ascending node at reference time (semicircles)
        /// </summary>
        public double OMEGA;

        /// <summary>
        /// Amplitude of the sine harmonic correction term to the angle of inclination (rad)
        /// </summary>
        public double Cis;
    }

    public class GroupData4
    {
        /// <summary>
        /// Inclination angle at reference time (semicircles)
        /// </summary>
        public double i0;

        /// <summary>
        /// Amplitude of the cosine harmonic correction term to the orbit radius (m)
        /// </summary>
        public double Crc;

        /// <summary>
        /// Argument of perigee (semicircles)
        /// </summary>
        public double omega;

        /// <summary>
        /// Rate of change of right ascension (semicircles/s)
        /// </summary>
        public double OMEGADOT;
    }

    public class GroupData5
    {
        /// <summary>
        /// Rate of change of inclination (semicircles/s)
        /// </summary>
        public double IDOT;

        /// <summary>
        /// Codes on L2 channel
        /// </summary>
        public double CodesL2;

        /// <summary>
        /// To use with T0E
        /// </summary>
        public double GPSWeek;

        /// <summary>
        /// L2 P data flag
        /// </summary>
        public double L2pFlag;
    }

    public class GroupData6
    {
        public double SVaccuracy;

        public double SVhealth;

        public double TGD;

        public double IODC;
    }
}
