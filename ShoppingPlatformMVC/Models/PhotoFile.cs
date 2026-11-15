namespace ShoppingPlatformMVC.Models
{
    public static class PhotoFile
    {
        public static string? GetContentType(byte[] data) => data switch
        {
            [0xFF, 0xD8, 0xFF, ..] => "image/jpeg",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, ..] => "image/png",
            _ => null
        };
    }
}