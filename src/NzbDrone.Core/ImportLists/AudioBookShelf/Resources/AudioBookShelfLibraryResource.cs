namespace NzbDrone.Core.ImportLists.AudioBookShelf.Resources
{
    /// <summary>
    /// Represents a library from the AudioBookShelf GET /api/libraries response
    /// </summary>
    public class AudioBookShelfLibraryResource
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MediaType { get; set; }  // "book" or "podcast"
        public string FolderPath { get; set; }
        public int NumBooks { get; set; }
    }
}
