namespace NzbDrone.Core.ImportLists.AudioBookShelf.Resources
{
    /// <summary>
    /// Represents an audio file in an AudioBookShelf library item
    /// </summary>
    public class AudioBookShelfAudioFileResource
    {
        public string Filename { get; set; }
        public string Format { get; set; }  // "mp3", "m4b", etc.
        public double Duration { get; set; }
        public string Path { get; set; }
    }
}
