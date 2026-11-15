namespace ShoppingPlatformMVC.Viewmodel
{
    public class PhotoDemoProductViewModel
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public bool HasPhoto { get; init; }
    }
}