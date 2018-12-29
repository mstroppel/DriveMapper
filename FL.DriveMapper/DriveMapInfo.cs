namespace FL.DriveMapper
{
    /// <summary>
    /// Contains the information to map one drive.
    /// </summary>
    public class DriveMapInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DriveMapInfo"/> class.
        /// </summary>
        /// <param name="drive">The drive.</param>
        /// <param name="networkPath">The network path.</param>
        /// <param name="label">The label.</param>
        public DriveMapInfo(string drive, string networkPath, string label)
        {
            Drive = drive;
            NetworkPath = networkPath;
            Label = label;
        }

        /// <summary>
        /// Gets the drive.
        /// </summary>
        public string Drive { get; private set; }

        /// <summary>
        /// Gets the network path.
        /// </summary>
        public string NetworkPath { get; private set; }

        /// <summary>
        /// Gets the label.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return typeof(DriveMapInfo).Name + ": Drive=" + Drive + " NetworkPath= " + NetworkPath + " Label=" + Label;
        }
    }
}
