using log4net;
using log4net.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Xml.Serialization;

namespace FL.DriveMapper
{
    /// <summary>
    /// Contains the main entry point for the application.
    /// </summary>
    public class Program
    {
        private static readonly ILog _logSink = LogManager.GetLogger(typeof(Program));
        private static readonly XmlSerializer _driveMappingSerializer = new XmlSerializer(typeof(DriveMappings));

        private static readonly DriveMapInfo[] _driveMappings = new []
        {
            new DriveMapInfo("P:", @"\\rst-server\shared", "Test02")
        };

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args"></param>
        public static int Main(string[] args)
        {
            var mappings = ReadConfiguration();
            foreach (var info in mappings)
            {
                _logSink.Info("Starte mapping von: " + info);
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
                var directory = new FileInfo(location).Directory.FullName;
                if (directory == null)
                {
                    _logSink.Error("Konnte das Verzeichnis der Applikation nicht bestimment!");
                    return Enumerable.Empty<DriveMapInfo>();
                }

                var fullPath = Path.Combine(directory, "DriveMappings.xml");
                if (!File.Exists(fullPath))
                {
                    _logSink.Error("Konnte die Konfigurationsdatei \"" + fullPath + "\" nicht finden!");
                    return Enumerable.Empty<DriveMapInfo>();
                }
                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    var parsed = (DriveMappings)_driveMappingSerializer.Deserialize(stream);
                    return parsed.Mappings;
                }
            }
            catch (Exception e)
            {
                _logSink.Error("Fehler beim Lesen der Konfigurationsdatei!", e);
            }
            return Enumerable.Empty<DriveMapInfo>();
        }
    }
}
