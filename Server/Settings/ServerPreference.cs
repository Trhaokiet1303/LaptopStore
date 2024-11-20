using System.Linq;
using LaptopStore.Shared.Constants.Localization;
using LaptopStore.Shared.Settings;

namespace LaptopStore.Server.Settings
{
    public record ServerPreference : IPreference
    {
        public string LanguageCode { get; set; } = LocalizationConstants.SupportedLanguages.FirstOrDefault()?.Code ?? "en-US";

        //TODO - add server preferences
    }
}