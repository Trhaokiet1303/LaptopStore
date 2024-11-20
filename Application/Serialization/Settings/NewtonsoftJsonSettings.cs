
using LaptopStore.Application.Interfaces.Serialization.Settings;
using Newtonsoft.Json;

namespace LaptopStore.Application.Serialization.Settings
{
    public class NewtonsoftJsonSettings : IJsonSerializerSettings
    {
        public JsonSerializerSettings JsonSerializerSettings { get; } = new();
    }
}