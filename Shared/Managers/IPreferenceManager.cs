using LaptopStore.Shared.Settings;
using System.Threading.Tasks;
using LaptopStore.Shared.Wrapper;

namespace LaptopStore.Shared.Managers
{
    public interface IPreferenceManager
    {
        Task SetPreference(IPreference preference);

        Task<IPreference> GetPreference();

    }
}