using Godot;
using System;

namespace Khepri.Resources.Celestial
{
    /// <summary> The data component of a star entity. </summary>
    [GlobalClass]
    public partial class StarResource : EntityResource, IComparable<StarResource>
    {
        /// <summary> A common name for the star, such as "Barnard's Star" or "Sirius". These are taken from the International Astronomical Union (https://www.iau.org/public/themes/naming_stars/, specifically, the formatted version from https://github.com/mirandadam/iau-starnames). </summary>
        [ExportGroup("Statistics")]
        [Export] public String ProperName { get; private set; }

        /// <summary> The star's right ascension for epoch and equinox 2000.0. </summary>
        /// <remarks> In radians. </remarks>
        [Export] public Double RightAscension { get; private set; }

        /// <summary> The star's declination for epoch and equinox 2000.0. </summary>
        /// <remarks> In radians. </remarks>
        [Export] public Double Declination { get; private set; }

        /// <summary> The star's distance in parsecs, the most common unit in astrometry. To convert parsecs to light years, multiply by 3.262. A value >= 100000 indicates missing or dubious (e.g., negative) parallax data in Hipparcos. </summary>
        [Export] public Single Distance { get; private set; }

        /// <summary> The star's proper motion, its 'drift', right ascension in milliarcseconds per year. </summary>
        /// <remarks> In radians. </remarks>
        [Export] public Double ProperMotionRightAscension { get; private set; }

        /// <summary> The star's proper motion, its 'drift', declination in milliarcseconds per year. </summary>
        /// <remarks> In radians. </remarks>
        [Export] public Double ProperMotionDeclination { get; private set; }

        /// <summary> The star's radial velocity in km/sec, where known. </summary>
        [Export] public Single RadialVelocity { get; private set; }

        /// <summary> The visual or apparent magnitude. </summary>
        [Export] public Single Magnitude { get; private set; }

        /// <summary> The star's color index (blue magnitude - visual magnitude), where known. </summary>
        [Export] public Single ColourIndex { get; private set; }


        /// <summary> The data component of a star entity. </summary>
        public StarResource() { }


        /// <inheritdoc/>
        public int CompareTo(StarResource? other)
        {
            return RightAscension.CompareTo(other?.RightAscension);
        }
    }
}
