using System;

namespace NzbDrone.Core.ImportLists.AudioBookShelf
{
    /// <summary>
    /// Exception thrown when AudioBookShelf API operations fail
    /// </summary>
    public class AudioBookShelfException : Exception
    {
        public AudioBookShelfException(string message)
            : base(message)
        {
        }

        public AudioBookShelfException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
