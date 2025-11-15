using System.Collections.Generic;

namespace NzbDrone.Core.ImportLists.AudioBookShelf.Resources
{
    /// <summary>
    /// Paginated response wrapper for AudioBookShelf API
    /// </summary>
    public class AudioBookShelfPaginatedResponse<T>
    {
        public List<T> Results { get; set; }
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Page { get; set; }
    }
}
