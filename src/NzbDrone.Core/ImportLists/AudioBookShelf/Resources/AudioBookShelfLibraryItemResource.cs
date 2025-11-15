namespace NzbDrone.Core.ImportLists.AudioBookShelf.Resources
{
    /// <summary>
    /// Represents a library item from AudioBookShelf GET /api/libraries/{id}/items
    /// </summary>
    public class AudioBookShelfLibraryItemResource
    {
        public string Id { get; set; }
        public string MediaType { get; set; }
        public AudioBookShelfMediaResource Media { get; set; }
        public AudioBookShelfMetadataResource Metadata { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
    }
}
