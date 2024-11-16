using log4net;
using System.Reflection;
using System.Xml.Serialization;

namespace FL.DriveMapper
{
    /// <summary>
    /// Contains the main entry point for the application.
    /// </summary>
    public class Program
    {
        private static readonly ILog LogSink = LogManager.GetLogger(typeof(Program));
        private static readonly XmlSerializer DriveMappingSerializer = new XmlSerializer(typeof(DriveMappings));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args"></param>
        public static int Main(string[] args)
        {
            var mappings = ReadConfiguration();
            foreach (var info in mappings)
            {
                LogSink.Info("Starte mapping von: " + info);
                var mapper = new DriveMap(info);
                mapper.MapDrive();
            }
            
            return (int)ReturnCodes.Successful;
        }

        private static IEnumerable<DriveMapInfo> ReadConfiguration()
        {
            try
            {
                var location = Assembly.GetCallingAssembly().Location;
                var directory = new FileInfo(location).Directory?.FullName;
                if (directory == null)
                {
                    LogSink.Error("Konnte das Verzeichnis der Applikation nicht bestimment!");
                    return Enumerable.Empty<DriveMapInfo>();
                }

                var fullPath = Path.Combine(directory, "DriveMappings.xml");
                if (!File.Exists(fullPath))
                {
                    LogSink.Error("Konnte die Konfigurationsdatei \"" + fullPath + "\" nicht finden!");
                    return Enumerable.Empty<DriveMapInfo>();
                }

                using var stream = new FileStream(fullPath, FileMode.Open);
                var parsed = (DriveMappings)DriveMappingSerializer.Deserialize(stream);
                return parsed?.Mappings;
            }
            catch (Exception e)
            {
                LogSink.Error("Fehler beim Lesen der Konfigurationsdatei!", e);
            }
            return Enumerable.Empty<DriveMapInfo>();
        }
    }
}
