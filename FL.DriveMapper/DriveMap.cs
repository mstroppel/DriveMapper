using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace FL.DriveMapper
{
    /// <summary>
    /// Implements the functionality to map a drive using the <c>subst</c> command
    /// and set the label.
    /// </summary>
    public class DriveMap
    {
        private static readonly ILog _logSink = LogManager.GetLogger(typeof(DriveMap));
        private DriveMapInfo _info;

        /// <summary>
        /// Initializes a new instance of the <see cref="DriveMap"/> class.
        /// </summary>
        /// <param name="info">The information to map the drive.</param>
        public DriveMap(DriveMapInfo info)
        {
            _info = info;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DriveMap"/> class.
        /// </summary>
        /// <param name="drive">The drive, e.g. "P:"</param>
        /// <param name="networkPath">The network path, e.g. "\\\\sepp\\projekte".</param>
        /// <param name="label">The label, e.g. "Projekte".</param>
        public DriveMap(string drive, string networkPath, string label)
        {
        }

        /// <summary>
        /// Maps the drive.
        /// </summary>
        /// <returns>The <see cref="Guid"/> in the registry path that was created for this drive mapping.</returns>
        public bool MapDrive()
        {
            if (!Ping())
                return false;

            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\MountPoints2");
            var subKeyAtBeginning = key.GetSubKeyNames();

            LogKeys("zu Beginn", subKeyAtBeginning);

            if (!RunSubst())
                return false;

            var createdKeys = WaitForCreatedRegistryKeys(subKeyAtBeginning);
            if (createdKeys == null)
                return false;

            foreach (var createdKey in createdKeys)
            {
                // Set the label.
                _logSink.Info("Registry Schlüssel: " + createdKey + " wurde erstellt. Setze das Label...");
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\MountPoints2\" + createdKey, "_LabelFromReg", _info.Label, RegistryValueKind.String);
            }

            return true;
        }

        private bool Ping()
        {
            if (!_info.NetworkPath.StartsWith(@"\\"))
            {
                _logSink.Info($"Pinge host {_info.NetworkPath} nicht an, da es kein Netzwerkpfad ist.");
                return true;
            }

            var hostname = _info.NetworkPath.Substring(2);
            var indexOfNextBackslash = hostname.IndexOf('\\');
            if (indexOfNextBackslash != 0)
                hostname = hostname.Substring(0, indexOfNextBackslash);

            var ping = new Ping();
            for (var i = 0; i < 1000; i++)
            {
                _logSink.Warn($"Ping den host '{hostname}' an...");

                var reply = ping.Send(hostname);
                if (reply.Status == IPStatus.Success)
                {
                    _logSink.Info("Ping erfolgreich :-)");
                    return true;
                }

                _logSink.Warn($"Ping an host '{hostname}' schlug fehl: {reply.Status}");
                Thread.Sleep(250);
            }

            _logSink.Error("Ping schlug fehl. Mapping wird nicht durchgeführt!");
            return false;
        }

        private static void LogKeys(string message, string[] subKeyAtBeginning)
        {
            _logSink.Info("Anzahl Schlüssel " + message + ": " + subKeyAtBeginning.Length);
            if (_logSink.IsDebugEnabled)
            {
                foreach (var key in subKeyAtBeginning)
                {
                    _logSink.Debug("  " + key);
                }
            }
        }

        /// <summary>
        /// Runs the subst application.
        /// </summary>
        /// <returns><c>true</c> if successfuly done, <c>false</c> otherwise.</returns>
        private bool RunSubst()
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = "subst.exe";
                process.StartInfo.Arguments = _info.Drive + " " + _info.NetworkPath;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    _logSink.Error("Fehler beim Ausführuen von subst.exe. Existiert das Laufwer schon? ExitCode = " + process.ExitCode);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logSink.Error("Fehler beim Starten von subst.exe", e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Waits for created registry key.
        /// </summary>
        /// <param name="existingKeys">The existing keys.</param>
        /// <returns>The created registry key or <c>null</c> if not key was created.</returns>
        private IEnumerable<string> WaitForCreatedRegistryKeys(string[] existingKeys)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\MountPoints2");
            var subKeysAfterDone = key.GetSubKeyNames().Distinct().ToArray();
            var breakCount = 0;
            var expectedLenght = existingKeys.Length + 2;
            while (subKeysAfterDone.Length < expectedLenght)
            {
                Task.Delay(500).Wait();
                key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\MountPoints2");
                subKeysAfterDone = key.GetSubKeyNames().Distinct().ToArray();
                ++breakCount;
                if (breakCount > 10)
                {
                    break;
                }
            }
            var created = subKeysAfterDone.Except(existingKeys).ToArray();

            if (created.Length == 0)
                _logSink.Warn("Fehler beim setzen des Labels von " + _info + "! Es wurde kein registry Eintrag erstellt!");
            else
                _logSink.Info("Folgende Schlüssel wurden erstellt: " + string.Join(", ", created));

            return created;
        }
    }
}
