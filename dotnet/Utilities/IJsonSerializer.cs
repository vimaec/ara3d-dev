using System;
using System.IO;
using System.IO.Compression;

namespace Ara3D.DotNetUtilities
{
    /// <summary>
    /// A minimal subset of the Newtonsoft.Json.JsonSerializer.
    /// </summary>
    public interface IJsonSerializer
    {
        object Deserialize(TextReader reader, Type objectType);
        void Serialize(TextWriter textWriter, object value);
    }

    public static class IJSonSerializerExtensions
    {
        /// <summary>
        /// Writes the object to the given filepath.
        /// </summary>
        public static void ToJsonFile(this IJsonSerializer serializer, object o, string filePath)
        {
            using (var tw = File.CreateText(filePath))
                serializer.Serialize(tw, o);
        }

        /// <summary>
        /// Loads the object from the given stream reader.
        /// </summary>
        public static T LoadJsonFromStream<T>(this IJsonSerializer serializer, StreamReader streamReader)
            => (T)(serializer).Deserialize(streamReader, typeof(T));

        /// <summary>
        /// Loads the object from the given stream.
        /// </summary>
        public static T LoadJsonFromStream<T>(this IJsonSerializer serializer, Stream stream)
        {
            using (var reader = new StreamReader(stream))
                return serializer.LoadJsonFromStream<T>(reader);
        }

        /// <summary>
        /// Loads the object from the given filepath.
        /// </summary>
        public static T LoadJsonFromFile<T>(this IJsonSerializer serializer, string filePath)
        {
            using (var file = File.OpenText(filePath))
                return serializer.LoadJsonFromStream<T>(file);
        }

        /// <summary>
        /// Loads the object from the given folder and file.
        /// </summary>
        public static T LoadJson<T>(this IJsonSerializer serializer, string folder, string file, ILogger logger) where T : class
        {
            var inputFile = Path.Combine(folder, file);
            return logger.Log($"Loading file {inputFile} which is {Util.FileSizeAsString(inputFile)}", () =>
                serializer.LoadJsonFromFile<T>(inputFile));
        }

        /// <summary>
        /// Loads the object from the given folder and file. The first parameter acts as a type inference helper for code ergonomics.
        /// </summary>
        public static T LoadJson<T>(this IJsonSerializer serializer, T x, string folder, string file, ILogger logger) where T : class
            => serializer.LoadJson<T>(folder, file, logger);

        /// <summary>
        /// Loads the object from the given stream.
        /// </summary>
        public static T LoadJson<T>(this IJsonSerializer serializer, Stream stream, ILogger logger) where T : class
            => serializer.LoadJsonFromStream<T>(stream);

        /// <summary>
        /// Loads the object from the given stream. The first parameter acts as a type inference helper for code ergonomics.
        /// </summary>
        public static T LoadJson<T>(this IJsonSerializer serializer, T x, Stream stream, ILogger logger) where T : class
            => serializer.LoadJson<T>(stream, logger);

        /// <summary>
        /// Loads the object from the given zip archive.
        /// </summary>
        public static T LoadJson<T>(this IJsonSerializer serializer, ZipArchive archive, string entryName, ILogger logger) where T : class
            => serializer.LoadJson<T>(archive.GetEntry(entryName).Open(), logger);

        /// <summary>
        /// Loads the object from the given zip archive. The first parameter acts as a type inference helper for code ergonomics.
        /// </summary>
        public static T LoadJson<T>(this IJsonSerializer serializer, T x, ZipArchive archive, string entryName, ILogger logger) where T : class
            => serializer.LoadJson<T>(archive, entryName, logger);
    }
}
