namespace Doppler.CloverAPI.DopplerSecurity;

public static class DopplerSecurityDefaults
{
    public const string PublicKeysFolderConfigKey = "DopplerSecurity:PublicKeysFolder";
    public const string PublicKeysFolderDefaultConfigValue = "public-keys";
    public const string PublicKeysFilenameConfigKey = @"DopplerSecurity:PublicKeysFilenameRegex";
    public const string PublicKeysFilenameRegexDefaultConfigValue = "\\.xml$";
    public const string SuperuserJwtKey = "isSU";
}
