namespace TrailingWhitespace
{
    using System;
    
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class PackageGuids
    {
        public const string guidVSPackageString = "043286da-34d9-44a7-93eb-16a775cfcf61";
        public const string guidVSPackageCmdSetString = "3ff85586-1fd8-4018-a861-5012505d6f3a";
        public static Guid guidVSPackage = new Guid(guidVSPackageString);
        public static Guid guidVSPackageCmdSet = new Guid(guidVSPackageCmdSetString);
    }
    /// <summary>
    /// Helper class that encapsulates all CommandIDs uses across VS Package.
    /// </summary>
    internal sealed partial class PackageIds
    {
    }
}
