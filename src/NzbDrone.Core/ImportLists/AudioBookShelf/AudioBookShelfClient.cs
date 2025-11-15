using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists.AudioBookShelf.Resources;

namespace NzbDrone.Core.ImportLists.AudioBookShelf
{
    /// <summary>
    /// Client for interacting with the AudioBookShelf API
    /// </summary>
    public class AudioBookShelfClient
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private readonly string _baseUrl;
        private readonly string _apiToken;

        public AudioBookShelfClient(IHttpClient httpClient, AudioBookShelfSettings settings, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = settings.BaseUrl?.TrimEnd('/');
            _apiToken = settings.ApiToken;
        }

        /// <summary>
        /// Builds an HTTP request with authentication headers
        /// </summary>
        private HttpRequest BuildRequest(string endpoint)
        {
            var request = new HttpRequest($"{_baseUrl}{endpoint}");
            request.Headers.Add("Authorization", $"Bearer {_apiToken}");
            request.Headers.Set("Content-Type", "application/json");
            request.AllowAutoRedirect = true;
            request.SuppressHttpError = false;
            return request;
        }

        /// <summary>
        /// Gets all libraries from AudioBookShelf
        /// </summary>
        public List<AudioBookShelfLibraryResource> GetLibraries()
        {
            try
            {
                _logger.Trace("Fetching libraries from AudioBookShelf");
                var request = BuildRequest("/api/libraries");
                var response = _httpClient.Get<AudioBookShelfLibrariesResponse>(request);

                if (response?.Resource?.Libraries == null)
                {
                    _logger.Warn("AudioBookShelf returned null libraries response");
                    return new List<AudioBookShelfLibraryResource>();
                }

                _logger.Debug($"Retrieved {response.Resource.Libraries.Count} libraries from AudioBookShelf");
                return response.Resource.Libraries;
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, "Failed to fetch libraries from AudioBookShelf");
                throw new AudioBookShelfException("Failed to connect to AudioBookShelf. Please check your URL and API token.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error fetching libraries from AudioBookShelf");
                throw new AudioBookShelfException("An unexpected error occurred while connecting to AudioBookShelf.", ex);
            }
        }

        /// <summary>
        /// Gets all items from a specific library with pagination support
        /// </summary>
        public List<AudioBookShelfLibraryItemResource> GetLibraryItems(string libraryId, int limit = 100)
        {
            var allItems = new List<AudioBookShelfLibraryItemResource>();
            var page = 0;

            try
            {
                _logger.Debug($"Fetching items from AudioBookShelf library {libraryId}");

                while (true)
                {
                    var endpoint = $"/api/libraries/{libraryId}/items?limit={limit}&page={page}";
                    var request = BuildRequest(endpoint);

                    _logger.Trace($"Fetching page {page} with limit {limit}");

                    var response = _httpClient.Get<AudioBookShelfPaginatedResponse<AudioBookShelfLibraryItemResource>>(request);

                    if (response?.Resource == null)
                    {
                        _logger.Warn($"AudioBookShelf returned null response for page {page}");
                        break;
                    }

                    if (response.Resource.Results == null || response.Resource.Results.Count == 0)
                    {
                        _logger.Trace($"No more items found at page {page}");
                        break;
                    }

                    allItems.AddRange(response.Resource.Results);
                    _logger.Trace($"Retrieved {response.Resource.Results.Count} items from page {page}, total so far: {allItems.Count}");

                    page++;

                    // Check if we've retrieved all items
                    if (allItems.Count >= response.Resource.Total)
                    {
                        _logger.Debug($"Retrieved all {allItems.Count} items from library");
                        break;
                    }

                    // Safety check to prevent infinite loops
                    if (page > 1000)
                    {
                        _logger.Warn($"Reached maximum page limit (1000), stopping pagination");
                        break;
                    }
                }

                _logger.Info($"Successfully retrieved {allItems.Count} items from AudioBookShelf library {libraryId}");
                return allItems;
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, $"Failed to fetch library items from AudioBookShelf library {libraryId} at page {page}");
                throw new AudioBookShelfException($"Failed to fetch items from library. Check that the library ID is correct.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Unexpected error fetching library items from AudioBookShelf library {libraryId}");
                throw new AudioBookShelfException("An unexpected error occurred while fetching library items.", ex);
            }
        }

        /// <summary>
        /// Gets a single item by ID (optional, for future use)
        /// </summary>
        public AudioBookShelfLibraryItemResource GetItem(string itemId)
        {
            try
            {
                _logger.Trace($"Fetching item {itemId} from AudioBookShelf");
                var request = BuildRequest($"/api/items/{itemId}");
                var response = _httpClient.Get<AudioBookShelfLibraryItemResource>(request);
                return response.Resource;
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, $"Failed to fetch item {itemId} from AudioBookShelf");
                throw new AudioBookShelfException($"Failed to fetch item {itemId}.", ex);
            }
        }

        /// <summary>
        /// Tests the connection to AudioBookShelf
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                _logger.Debug("Testing connection to AudioBookShelf");
                var libraries = GetLibraries();
                _logger.Info("AudioBookShelf connection test successful");
                return libraries != null;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "AudioBookShelf connection test failed");
                return false;
            }
        }
    }
}
