using System.Collections.Generic;

namespace NzbDrone.Core.ImportLists.AudioBookShelf.Resources
{
    /// <summary>
    /// Response from GET /api/libraries endpoint
    /// </summary>
    public class AudioBookShelfLibrariesResponse
    {
        public List<AudioBookShelfLibraryResource> Libraries { get; set; }
    }
}
