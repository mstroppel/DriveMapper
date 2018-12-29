namespace FL.DriveMapper
{
    /// <summary>
    /// Defines the different return codes of the application.
    /// </summary>
    public enum ReturnCodes
    {
        /// <summary>
        /// Successful completion.
        /// </summary>
        Successful = 0,

                /// <summary>
        /// Application was not started with elevated rights.
        /// </summary>
        NotElevated = -2,

        /// <summary>
        /// Unkown error.
        /// </summary>
        Failed = -1

    }
}
