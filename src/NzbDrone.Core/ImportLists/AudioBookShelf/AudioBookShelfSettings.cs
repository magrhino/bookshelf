using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.AudioBookShelf
{
    public class AudioBookShelfSettingsValidator : AbstractValidator<AudioBookShelfSettings>
    {
        public AudioBookShelfSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).NotEmpty().WithMessage("AudioBookShelf URL is required");
            RuleFor(c => c.ApiToken).NotEmpty().WithMessage("API Token is required");
            RuleFor(c => c.LibraryId).NotEmpty().WithMessage("Library must be selected");
        }
    }

    public class AudioBookShelfSettings : IImportListSettings
    {
        private static readonly AudioBookShelfSettingsValidator Validator = new AudioBookShelfSettingsValidator();

        [FieldDefinition(1, Label = "AudioBookShelf URL", HelpText = "URL of your AudioBookShelf server (e.g., http://localhost:13378)")]
        public string BaseUrl { get; set; }

        [FieldDefinition(2, Label = "API Token", Type = FieldType.Password, Privacy = PrivacyLevel.Password, HelpText = "API token from AudioBookShelf settings")]
        public string ApiToken { get; set; }

        [FieldDefinition(3, Label = "Library", Type = FieldType.Select, SelectOptionsProviderAction = "getLibraries", HelpText = "AudioBookShelf library to import from")]
        public string LibraryId { get; set; }

        [FieldDefinition(4, Label = "Import Audiobooks Only", Type = FieldType.Checkbox, HelpText = "Only import items with audio files")]
        public bool AudiobooksOnly { get; set; } = true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
