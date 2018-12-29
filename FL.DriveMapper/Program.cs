using log4net;
using log4net.Config;
using System;
using System.Security.Principal;

namespace FL.DriveMapper
{
    /// <summary>
    /// Contains the main entry point for the application.
    /// </summary>
    public class Program
    {
        private static readonly ILog _logSink = LogManager.GetLogger(typeof(Program));

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
            foreach (var info in _driveMappings)
            {
                var mapper = new DriveMap(info);
                mapper.MapDrive();
            }
            
            return (int)ReturnCodes.Successful;
        }
    }
}
