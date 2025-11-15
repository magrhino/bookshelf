using System.Collections.Generic;

namespace NzbDrone.Core.ImportLists.AudioBookShelf.Resources
{
    /// <summary>
    /// Book metadata from AudioBookShelf
    /// </summary>
    public class AudioBookShelfMetadataResource
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Author { get; set; }  // May be comma-separated
        public List<AudioBookShelfAuthorResource> Authors { get; set; }
        public string Narrator { get; set; }
        public string Publisher { get; set; }
        public string PublishedYear { get; set; }
        public string Isbn { get; set; }
        public string Asin { get; set; }
        public string Description { get; set; }
        public List<string> Genres { get; set; }
    }
}
