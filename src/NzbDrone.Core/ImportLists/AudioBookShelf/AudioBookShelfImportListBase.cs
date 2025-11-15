using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.ImportLists.AudioBookShelf
{
    /// <summary>
    /// Base class for AudioBookShelf import list providers
    /// </summary>
    public abstract class AudioBookShelfImportListBase<TSettings> : ImportListBase<TSettings>
        where TSettings : AudioBookShelfSettings, new()
    {
        protected readonly IHttpClient _httpClient;

        protected AudioBookShelfImportListBase(
            IImportListStatusService importListStatusService,
            IConfigService configService,
            IParsingService parsingService,
            IHttpClient httpClient,
            Logger logger)
            : base(importListStatusService, configService, parsingService, logger)
        {
            _httpClient = httpClient;
        }

        public override ImportListType ListType => ImportListType.Other;

        /// <summary>
        /// Creates an AudioBookShelf API client with current settings
        /// </summary>
        protected AudioBookShelfClient GetClient()
        {
            return new AudioBookShelfClient(_httpClient, Settings, _logger);
        }

        /// <summary>
        /// Tests the connection to AudioBookShelf
        /// </summary>
        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var client = GetClient();
                if (!client.TestConnection())
                {
                    return new ValidationFailure(string.Empty, "Failed to connect to AudioBookShelf. Please check your URL and API token.");
                }

                return null;
            }
            catch (AudioBookShelfException ex)
            {
                _logger.Error(ex, "AudioBookShelf connection test failed");
                return new ValidationFailure(string.Empty, $"Connection failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "AudioBookShelf connection test failed unexpectedly");
                return new ValidationFailure(string.Empty, $"Connection failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the getLibraries action to populate the library dropdown
        /// </summary>
        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "getLibraries")
            {
                try
                {
                    var client = GetClient();
                    var libraries = client.GetLibraries();

                    return new
                    {
                        options = libraries
                            .FindAll(l => l.MediaType == "book")
                            .ConvertAll(l => new { value = l.Id, name = l.Name })
                    };
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to fetch libraries for dropdown");
                    return new { options = new List<object>() };
                }
            }

            return base.RequestAction(action, query);
        }
    }
}
