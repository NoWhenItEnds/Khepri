using System;
using System.Collections.Generic;
using Godot;

namespace Jaypen.Utilities.Extensions
{
    /// <summary> Helper methods for working with CSVs. </summary>
    public static class CsvExtensions
    {
        /// <summary> Load the given CSV file as a concrete data object. </summary>
        /// <typeparam name="T"> The type of data object to attempt to serialise. </typeparam>
        /// <param name="relativeDirectoryPath"> The Godot-relative file path to load. </param>
        /// <returns> An array of the objects serialised from the loaded CSV. </returns>
        /// <exception cref="System.IO.FileNotFoundException"> Thrown when Godot cannot open the file at <paramref name="relativeDirectoryPath"/>. </exception>
        /// <exception cref="FormatException"> Thrown when <see cref="IParseable{T}.Parse"/> fails for a data row. </exception>
        public static T[] LoadData<T>(String relativeDirectoryPath) where T : IParseable<T>
        {
            using FileAccess? file = FileAccess.Open(relativeDirectoryPath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                Error openError = FileAccess.GetOpenError();
                throw new System.IO.FileNotFoundException(
                    $"Could not open CSV file '{relativeDirectoryPath}': Godot error {openError}.",
                    relativeDirectoryPath);
            }

            List<T> result = new List<T>();
            String[] header = file.GetCsvLine();

            // Row numbering starts at 2 to match spreadsheet convention (row 1 is the header).
            Int32 rowNumber = 1;
            while (!file.EofReached())
            {
                rowNumber++;
                String[] currentLine = file.GetCsvLine();
                Boolean rowIsEmpty = currentLine.Length == 0 || String.IsNullOrWhiteSpace(currentLine[0]);
                if (!rowIsEmpty)
                {
                    try
                    {
                        result.Add(T.Parse(header, currentLine));
                    }
                    catch (Exception inner)
                    {
                        throw new FormatException(
                            $"Failed to parse row {rowNumber} of '{relativeDirectoryPath}': [{String.Join(", ", currentLine)}]",
                            inner);
                    }
                }
            }

            return result.ToArray();
        }


        /// <summary> Indicates that the object is parseable from a CSV. </summary>
        /// <typeparam name="T"> The type of the parseable model. </typeparam>
        public interface IParseable<T> where T : IParseable<T>
        {
            /// <summary> Attempt to parse the loaded CSV data into a concrete model. </summary>
            /// <param name="header"> An ordered list of the file's headers. </param>
            /// <param name="data"> The data ordered into the same format as the header. </param>
            /// <returns> A constructed object. </returns>
            /// <exception cref="FormatException"> Thrown by implementations when the data cannot be mapped to a valid object. </exception>
            public static abstract T Parse(String[] header, String[] data);
        }
    }
}
