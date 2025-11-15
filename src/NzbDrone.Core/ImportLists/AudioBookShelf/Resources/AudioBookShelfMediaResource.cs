using System.Collections.Generic;

namespace NzbDrone.Core.ImportLists.AudioBookShelf.Resources
{
    /// <summary>
    /// Media information for an AudioBookShelf library item
    /// </summary>
    public class AudioBookShelfMediaResource
    {
        public List<AudioBookShelfAudioFileResource> AudioFiles { get; set; }
        public string EbookFormat { get; set; }  // "epub", "pdf", etc.
        public int NumAudioFiles { get; set; }
        public int NumTracks { get; set; }
        public double Duration { get; set; }
    }
}
