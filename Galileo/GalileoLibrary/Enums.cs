using System;
using System.Collections.Generic;
using System.Text;

namespace Galileo.Enums
{
    public class Rinex
    {
        public enum SatelliteSystems
        {
            GPS,
            GLONASS,
            Galileo,
            QZSS,
            BDS,
            IRNSS,
            SBAS,
            Mixed
        }

        public enum Types
        {
            Observation,
            Navigation,
            Meteorological
        }

        public enum MarkerTypes
        {
            /// <summary>
            ///  Earth-fixed, high- precision monument
            /// </summary>
            Geodetic,

            /// <summary>
            /// Earth-fixed, lowprecision monument
            /// </summary>
            NonGeodetic,

            /// <summary>
            ///  Generated from network processing
            /// </summary>
            NonPhysicals,

            /// <summary>
            /// Orbiting space vehicle
            /// </summary>
            Spaceborne,

            /// <summary>
            /// Mobile terrestrial vehicle
            /// </summary>
            GroundCraft,

            /// <summary>
            ///  Mobile water craft
            /// </summary>
            WaterCraft,

            /// <summary>
            /// Aircraft, balloon, etc.
            /// </summary>
            Airborne,

            /// <summary>
            /// "Fixed" on water surface
            /// </summary>
            FixedBuoy,

            /// <summary>
            /// Floating on water surface
            /// </summary>
            FloatingBuoy,

            /// <summary>
            /// Floating ice sheet, etc.
            /// </summary>
            Floatingice,

            /// <summary>
            ///  "Fixed" on a glacier
            /// </summary>
            Glacier,

            /// <summary>
            /// Rockets, shells, etc
            /// </summary>
            Ballistic,

            /// <summary>
            /// Animal carrying a receiver
            /// </summary>
            Animal,

            /// <summary>
            /// Human being
            /// </summary>
            Human
        }
    }
}
