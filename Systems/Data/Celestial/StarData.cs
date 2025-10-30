using CsvHelper;
using CsvHelper.Configuration;
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace Khepri.Data.Celestial
{
    /// <summary> An data object representing distant stars. </summary>
    public class StarData : CelestialData, IComparable<StarData>
    {
        /// <summary> The star's common name. </summary>
        [JsonPropertyName("name"), Required]
        public required String ProperName { get; init; }

        /// <summary> The star's ID in the Hipparcos catalog, if known. </summary>
        [JsonPropertyName("hip"), Required]
        public required Int32 HipId { get; init; }

        /// <summary> The star's ID in the Henry Draper catalog, if known. </summary>
        [JsonPropertyName("hd"), Required]
        public required Int32 HDId { get; init; }

        /// <summary> The star's ID in the Harvard Revised catalog, which is the same as its number in the Yale Bright Star Catalog. </summary>
        [JsonPropertyName("hr"), Required]
        public required Int32 HRId { get; init; }

        /// <summary> The star's ID in the third edition of the Gliese Catalog of Nearby Stars. </summary>
        [JsonPropertyName("gl"), Required]
        public required String GLId { get; init; }

        /// <summary> The Bayer / Flamsteed designation, primarily from the Fifth Edition of the Yale Bright Star Catalog. </summary>
        /// <remarks> This is a combination of the two designations. The Flamsteed number, if present, is given first; then a three-letter abbreviation for the Bayer Greek letter; the Bayer superscript number, if present; and finally, the three-letter constellation abbreviation. Thus Alpha Andromedae has the field value "21Alp And", and Kappa1 Sculptoris (no Flamsteed number) has "Kap1Scl". </remarks>
        [JsonPropertyName("bf"), Required]
        public required String BayerFlamsteed { get; init; }

        /// <summary> The star's right ascension for epoch and equinox 2000.0. </summary>
        /// <remarks> In radians. </remarks>
        [JsonPropertyName("ra"), Required]
        public required Double RightAscension { get; init; }

        /// <summary> The star's declination for epoch and equinox 2000.0. </summary>
        /// <remarks> In radians. </remarks>
        [JsonPropertyName("dec"), Required]
        public required Double Declination { get; init; }

        /// <summary> The star's distance in parsecs, the most common unit in astrometry. To convert parsecs to light years, multiply by 3.262. A value >= 100000 indicates missing or dubious (e.g., negative) parallax data in Hipparcos. </summary>
        [JsonPropertyName("dis"), Required]
        public required Single Distance { get; init; }

        /// <summary> The star's proper motion, its 'drift', right ascension in milliarcseconds per year. </summary>
        /// <remarks> In radians. </remarks>
        [JsonPropertyName("pmra"), Required]
        public required Double ProperMotionRightAscension { get; init; }

        /// <summary> The star's proper motion, its 'drift', declination in milliarcseconds per year. </summary>
        /// <remarks> In radians. </remarks>
        [JsonPropertyName("pmdec"), Required]
        public required Double ProperMotionDeclination { get; init; }

        /// <summary> The star's radial velocity in km/sec, where known. </summary>
        [JsonPropertyName("rv"), Required]
        public required Single RadialVelocity { get; init; }

        /// <summary> The visual or apparent magnitude. </summary>
        [JsonPropertyName("mag"), Required]
        public required Single Magnitude { get; init; }

        /// <summary> The star's color index (blue magnitude - visual magnitude), where known. </summary>
        [JsonPropertyName("ci"), Required]
        public required Single ColourIndex { get; init; }


        /// <summary> Get the most appropriate identifier to represent the object. </summary>
        /// <returns> An identifier that represents the star in a star catalogue. </returns>
        public String GetIdentifier()
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
                result = GLId;
            }

            return result;
        }


        public Color CalculateColour()
        {
            // Clamp B-V to valid range [-0.4, 2.0]
            Single colourIndex = Math.Clamp(ColourIndex, -0.4f, 2f);

            float r = 0f, g = 0f, b = 0f;
            float t;

            // Compute red component
            if (colourIndex >= -0.40f && colourIndex < 0.00f)
            {
                t = (colourIndex + 0.40f) / (0.00f + 0.40f);
                r = 0.61f + (0.11f * t) + (0.1f * t * t);
            }
            else if (colourIndex >= 0.00f && colourIndex < 0.40f)
            {
                t = (colourIndex - 0.00f) / (0.40f - 0.00f);
                r = 0.83f + (0.17f * t);
            }
            else if (colourIndex >= 0.40f && colourIndex < 2.10f)
            {
                r = 1.00f;
            }

            // Compute green component
            if (colourIndex >= -0.40f && colourIndex < 0.00f)
            {
                t = (colourIndex + 0.40f) / (0.00f + 0.40f);
                g = 0.70f + (0.07f * t) + (0.1f * t * t);
            }
            else if (colourIndex >= 0.00f && colourIndex < 0.40f)
            {
                t = (colourIndex - 0.00f) / (0.40f - 0.00f);
                g = 0.87f + (0.11f * t);
            }
            else if (colourIndex >= 0.40f && colourIndex < 1.60f)
            {
                t = (colourIndex - 0.40f) / (1.60f - 0.40f);
                g = 0.98f - (0.16f * t);
            }
            else if (colourIndex >= 1.60f && colourIndex < 2.00f)
            {
                t = (colourIndex - 1.60f) / (2.00f - 1.60f);
                g = 0.82f - (0.5f * t * t);
            }

            // Compute blue component
            if (colourIndex >= -0.40f && colourIndex < 0.40f)
            {
                b = 1.00f;
            }
            else if (colourIndex >= 0.40f && colourIndex < 1.50f)
            {
                t = (colourIndex - 0.40f) / (1.50f - 0.40f);
                b = 1.00f - (0.47f * t) + (0.1f * t * t);
            }
            else if (colourIndex >= 1.50f && colourIndex < 1.94f)
            {
                t = (colourIndex - 1.50f) / (1.94f - 1.50f);
                b = 0.63f - (0.6f * t * t);
            }

            return new Color(r, g, b);
        }


        /// <inheritdoc/>
        public Int32 CompareTo(StarData? other)
        {
            return RightAscension.CompareTo(other?.RightAscension);
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
                StarData[] records = csv.GetRecords<StarData>().ToArray();
                StarData[] resources = new StarData[records.Length];
                for (Int32 i = 0; i < resources.Length; i++)
                {
                    StarData record = records[i];
                    resources[i] = new StarData
                    {
                        Kind = "celestial_star",
                        Descriptions = Array.Empty<String>(),
                        ProperName = record.ProperName,
                        HipId = record.HipId,
                        HDId = record.HDId,
                        HRId = record.HRId,
                        GLId = record.GLId,
                        BayerFlamsteed = record.BayerFlamsteed,
                        RightAscension = record.RightAscension,
                        Declination = record.Declination,
                        Distance = record.Distance,
                        ProperMotionRightAscension = record.ProperMotionRightAscension,
                        ProperMotionDeclination = record.ProperMotionDeclination,
                        RadialVelocity = record.RadialVelocity,
                        Magnitude = record.Magnitude,
                        ColourIndex = record.ColourIndex
                    };
                }
                return resources;
            }
        }
    }


    /// <summary> The maps used by CsvHelper to map csv data to StarData. </summary>
    public sealed class StarDataMap : CsvClassMap<StarData>
    {
        public StarDataMap()
        {
            Map(m => m.ProperName).Name("proper").Default(String.Empty);
            Map(m => m.HipId).Name("hip").Default(-1);
            Map(m => m.HDId).Name("hd").Default(-1);
            Map(m => m.HRId).Name("hr").Default(-1);
            Map(m => m.GLId).Name("gl").Default(String.Empty);
            Map(m => m.BayerFlamsteed).Name("bf").Default(String.Empty);
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
