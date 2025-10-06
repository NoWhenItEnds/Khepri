using CsvHelper;
using CsvHelper.Configuration;
using Godot;
using Khepri.Types.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Khepri.Models
{
    /// <summary> A piece of data representing a star. </summary>
    public record StarData : IComparable<StarData>
    {
        /// <summary> The primary id of the star in the database. </summary>
        public Int32 Id { get; init; } = -1;

        /// <summary> The star's ID in the Hipparcos catalog, if known. </summary>
        public Int32 HipId { get; init; } = -1;

        /// <summary> The star's ID in the Henry Draper catalog, if known. </summary>
        public Int32 HDId { get; init; } = -1;

        /// <summary> The star's ID in the Harvard Revised catalog, which is the same as its number in the Yale Bright Star Catalog. </summary>
        public Int32 HRId { get; init; } = -1;

        /// <summary> The star's ID in the third edition of the Gliese Catalog of Nearby Stars. </summary>
        public String GLId { get; init; } = String.Empty;

        /// <summary> The Bayer / Flamsteed designation, primarily from the Fifth Edition of the Yale Bright Star Catalog. </summary>
        /// <remarks> This is a combination of the two designations. The Flamsteed number, if present, is given first; then a three-letter abbreviation for the Bayer Greek letter; the Bayer superscript number, if present; and finally, the three-letter constellation abbreviation. Thus Alpha Andromedae has the field value "21Alp And", and Kappa1 Sculptoris (no Flamsteed number) has "Kap1Scl". </remarks>
        public String BayerFlamsteed { get; init; } = String.Empty;

        /// <summary> A common name for the star, such as "Barnard's Star" or "Sirius". These are taken from the International Astronomical Union (https://www.iau.org/public/themes/naming_stars/, specifically, the formatted version from https://github.com/mirandadam/iau-starnames). </summary>
        public String Proper { get; init; } = String.Empty;

        /// <summary> The star's right ascension for epoch and equinox 2000.0. </summary>
        /// <remarks> In radians. </remarks>
        public Double RightAscension { get; init; } = 0;

        /// <summary> The star's declination for epoch and equinox 2000.0. </summary>
        /// <remarks> In radians. </remarks>
        public Double Declination { get; init; } = 0;

        /// <summary> The star's distance in parsecs, the most common unit in astrometry. To convert parsecs to light years, multiply by 3.262. A value >= 100000 indicates missing or dubious (e.g., negative) parallax data in Hipparcos. </summary>
        public Single Distance { get; init; } = 100000;

        /// <summary> The star's proper motion, its 'drift', right ascension in milliarcseconds per year. </summary>
        /// <remarks> In radians. </remarks>
        public Double ProperMotionRightAscension { get; init; } = 0;

        /// <summary> The star's proper motion, its 'drift', declination in milliarcseconds per year. </summary>
        /// <remarks> In radians. </remarks>
        public Double ProperMotionDeclination { get; init; } = 0;

        /// <summary> The star's radial velocity in km/sec, where known. </summary>
        public Single RadialVelocity { get; init; } = 0;

        /// <summary> The visual or apparent magnitude. </summary>
        public Single Magnitude { get; init; } = 0;

        /// <summary> The star's color index (blue magnitude - visual magnitude), where known. </summary>
        public Single ColourIndex { get; init; } = 0;


        /// <summary> An empty or default StarData object. </summary>
        public static StarData None => new StarData();


        /// <inheritdoc/>
        public int CompareTo(StarData other)
        {
            return RightAscension.CompareTo(other.RightAscension);
        }


        /// <summary> Tries to get the formatted id of the star according to one of the catalogues. </summary>
        /// <returns> The formatted id of the star with the catalogue as a prefix. </returns>
        public String GetId()
        {
            String result = "UNKNOWN";
            if (HRId != -1)
            {
                result = $"HR {HRId}";
            }
            else if (HipId != -1)
            {
                result = $"HIP {HipId}";
            }
            else if (HDId != -1)
            {
                result = $"HD {HipId}";
            }
            else if (!String.IsNullOrEmpty(GLId))
            {
                result = $"GJ {GLId}";
            }

            return result;
        }


        /// <summary> Generate the data from the HTG data file. </summary>
        /// <returns> An array of generated data. </returns>
        /// <remarks> https://codeberg.org/astronexus/hyg - v42 @ 2025-08-09 </remarks>
        public static StarData[] LoadFromHYG()
        {
            String csvPath = ProjectSettings.GlobalizePath("res://Data/HYGData/hygdata_v42.csv");
            using (StreamReader reader = new StreamReader(csvPath))
            using (CsvReader csv = new CsvReader(reader))
            {
                csv.Configuration.RegisterClassMap<StarDataMap>();
                return csv.GetRecords<StarData>().ToArray();
            }
        }

    }


    /// <summary> The maps used by CsvHelper to map csv data to StarData. </summary>
    public sealed class StarDataMap : CsvClassMap<StarData>
    {
        public StarDataMap()
        {
            Map(m => m.Id).Name("id").Default(-1);
            Map(m => m.HipId).Name("hip").Default(-1);
            Map(m => m.HDId).Name("hd").Default(-1);
            Map(m => m.HRId).Name("hr").Default(-1);
            Map(m => m.GLId).Name("gl").Default(String.Empty);
            Map(m => m.BayerFlamsteed).Name("bf").Default(String.Empty);
            Map(m => m.Proper).Name("proper").Default(String.Empty);
            Map(m => m.RightAscension).Name("rarad").Default(0);
            Map(m => m.Declination).Name("decrad").Default(0);
            Map(m => m.Distance).Name("dist").Default(100000);
            Map(m => m.ProperMotionRightAscension).Name("pmrarad").Default(0);
            Map(m => m.ProperMotionDeclination).Name("pmdecrad").Default(0);
            Map(m => m.RadialVelocity).Name("rv").Default(0);
            Map(m => m.Magnitude).Name("mag").Default(0);
            Map(m => m.ColourIndex).Name("ci").Default(0);
        }
    }


    /// <summary> Extensions for working with StarData. </summary>
    public static class StarDataExtensions
    {
        public static StarData[][] Load2DArrayFromFile(Single segmentSize)
        {
            // Setup map.
            Int32 length = (Int32)Math.Round(180f / segmentSize); // How many segements are in the declination range.
            List<StarData>[] array = new List<StarData>[length];
            for (Int32 i = 0; i < length; i++)
            {
                array[i] = new List<StarData>();
            }

            // Sort data.
            StarData[] initial = StarData.LoadFromHYG();
            Array.Sort(initial);    // Sort initially by RA.
            Single segmentRad = Mathf.DegToRad(segmentSize);
            foreach (StarData star in initial)
            {
                Int32 pos = (Int32)((star.Declination + 1.5707964) / segmentRad);   // Min is -90, so it needs to be offset so -90 == 0.
                array[pos].Add(star);
            }

            // Format results.
            StarData[][] result = new StarData[length][];
            for (Int32 i = 0; i < length; i++)
            {
                result[i] = array[i].ToArray();
            }
            return result;
        }


        /// <summary> Filter the data to only include the objects nearby the current declination. </summary>
        /// <param name="data"> The original star data object, sorted first by right ascension then declination. </param>
        /// <param name="declination"> The desired declination. </param>
        /// <returns> A list of stars with declinations nearby the provided value. </returns>
        public static StarData[] FilterData(this StarData[][] data, Single declination)
        {
            Single dec = Mathf.DegToRad(declination);
            Double weight = (dec + 1.5707964) / Mathf.Pi;
            Int32 index = (Int32)Mathf.Lerp(0, data.Length - 1, weight);

            StarData[] result = data[index];
            if (index > 0)
            {
                result = result.Union(data[index - 1]).ToArray();
            }
            if (index < data.Length - 1)
            {
                result = result.Union(data[index + 1]).ToArray();
            }

            return result;
        }
    }
}
