using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists.AudioBookShelf.Resources;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.AudioBookShelf
{
    /// <summary>
    /// AudioBookShelf Library import list provider for hardcover
    /// Imports audiobooks from an AudioBookShelf server library
    /// </summary>
    public class AudioBookShelfImport : AudioBookShelfImportListBase<AudioBookShelfSettings>
    {
        public AudioBookShelfImport(
            IImportListStatusService importListStatusService,
            IConfigService configService,
            IParsingService parsingService,
            IHttpClient httpClient,
            Logger logger)
            : base(importListStatusService, configService, parsingService, httpClient, logger)
        {
        }

        public override string Name => "AudioBookShelf Library";
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        /// <summary>
        /// Fetches items from the AudioBookShelf library
        /// </summary>
        public override IList<ImportListItemInfo> Fetch()
        {
            _logger.Info("Fetching books from AudioBookShelf library {0}", Settings.LibraryId);

            try
            {
                var client = GetClient();
                var items = client.GetLibraryItems(Settings.LibraryId);

                _logger.Debug("Retrieved {0} items from AudioBookShelf", items.Count);

                // Filter audiobooks if configured
                if (Settings.AudiobooksOnly)
                {
                    var originalCount = items.Count;
                    items = items.Where(IsAudiobook).ToList();
                    _logger.Debug("Filtered to {0} audiobook items (from {1} total)", items.Count, originalCount);
                }

                var result = items.Select(MapToImportListItem).Where(x => x != null).ToList();

                _logger.Info("Mapped {0} AudioBookShelf items to import list items", result.Count);

                return CleanupListItems(result);
            }
            catch (AudioBookShelfException ex)
            {
                _logger.Error(ex, "Failed to fetch items from AudioBookShelf");
                return new List<ImportListItemInfo>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error fetching items from AudioBookShelf");
                return new List<ImportListItemInfo>();
            }
        }

        /// <summary>
        /// Determines if an item is an audiobook (has audio files)
        /// </summary>
        private bool IsAudiobook(AudioBookShelfLibraryItemResource item)
        {
            return item.MediaType == "book" &&
                   item.Media?.AudioFiles != null &&
                   item.Media.AudioFiles.Any();
        }

        /// <summary>
        /// Maps an AudioBookShelf library item to ImportListItemInfo
        /// This is the critical method that enables hardcover metadata matching via ISBN/ASIN
        /// </summary>
        private ImportListItemInfo MapToImportListItem(AudioBookShelfLibraryItemResource absItem)
        {
            try
            {
                if (absItem.Metadata == null)
                {
                    _logger.Warn("AudioBookShelf item {0} has no metadata, skipping", absItem.Id);
                    return null;
                }

                var metadata = absItem.Metadata;

                // Get primary author (ABS can have multiple authors)
                var authorName = metadata.Authors?.FirstOrDefault()?.Name
                                ?? metadata.Author
                                ?? "Unknown Author";

                var bookTitle = metadata.Title;
                if (string.IsNullOrWhiteSpace(bookTitle))
                {
                    _logger.Warn("AudioBookShelf item {0} has no title, skipping", absItem.Id);
                    return null;
                }

                // Clean up ISBN (remove hyphens/spaces)
                var isbn = metadata.Isbn?.Replace("-", "").Replace(" ", "").Trim();
                var asin = metadata.Asin?.Trim();

                _logger.Trace("Mapping AudioBookShelf item: {0} by {1} (ISBN: {2}, ASIN: {3})",
                    bookTitle, authorName, isbn ?? "none", asin ?? "none");

                var importItem = new ImportListItemInfo
                {
                    Author = authorName.CleanSpaces(),
                    Book = bookTitle.CleanSpaces(),

                    // Leave Goodreads IDs null - will be looked up by metadata service
                    // The name "GoodreadsId" is misleading - these work with hardcover/bookinfo.pro too
                    AuthorGoodreadsId = null,
                    BookGoodreadsId = null,
                    EditionGoodreadsId = null,

                    // Parse release date if available
                    ReleaseDate = ParseReleaseDate(metadata.PublishedYear),

                    // NEW: Enhanced matching for hardcover via ISBN/ASIN
                    // These fields will be used by ImportListSyncService for more accurate matching
                    Isbn = isbn.IsNotNullOrWhiteSpace() ? isbn : null,
                    Asin = asin.IsNotNullOrWhiteSpace() ? asin : null
                };

                return importItem;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to map AudioBookShelf item {0}", absItem.Id);
                return null;
            }
        }

        /// <summary>
        /// Parses the release date from AudioBookShelf metadata
        /// </summary>
        private DateTime ParseReleaseDate(string publishedYear)
        {
            if (string.IsNullOrWhiteSpace(publishedYear))
            {
                return DateTime.MinValue;
            }

            // Try to parse as year only (most common)
            if (int.TryParse(publishedYear, out var year) && year > 1000 && year < 3000)
            {
                return new DateTime(year, 1, 1);
            }

            // Try to parse as full date
            if (DateTime.TryParse(publishedYear, out var date))
            {
                return date;
            }

            _logger.Trace("Could not parse release date: {0}", publishedYear);
            return DateTime.MinValue;
        }
    }
}
