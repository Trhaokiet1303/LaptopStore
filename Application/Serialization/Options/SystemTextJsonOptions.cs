using System.Text.Json;
using LaptopStore.Application.Interfaces.Serialization.Options;

namespace LaptopStore.Application.Serialization.Options
{
    public class SystemTextJsonOptions : IJsonSerializerOptions
    {
        public JsonSerializerOptions JsonSerializerOptions { get; } = new();
    }
}