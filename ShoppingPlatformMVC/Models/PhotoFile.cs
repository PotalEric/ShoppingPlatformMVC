namespace ShoppingPlatformMVC.Models
{
    public static class PhotoFile
    {
        public const int MaxBytes = 5 * 1024 * 1024;

        public static string? GetContentType(byte[] data) => data switch
        {
            [0xFF, 0xD8, 0xFF, ..] => "image/jpeg",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, ..] => "image/png",
            _ => null
        };

        public static async Task<(byte[]? Data, string? ContentType, string? Error)> ReadAsync(IFormFile photo)
        {
            if (photo.Length == 0 || photo.Length > MaxBytes)
            {
                return (null, null, "Choose a JPEG or PNG photo no larger than 5 MB.");
            }

            using var stream = new MemoryStream();
            await photo.CopyToAsync(stream);
            var data = stream.ToArray();
            var contentType = GetContentType(data);
            return contentType is null
                ? (null, null, "The file must be a JPEG or PNG photo.")
                : (data, contentType, null);
        }
    }
}