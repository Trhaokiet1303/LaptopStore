using System.Threading.Tasks;
using LaptopStore.Server.Managers.Preferences;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.Server.Controllers.Utilities
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreferencesController : ControllerBase
    {
        private readonly ServerPreferenceManager _serverPreferenceManager;

        public PreferencesController(ServerPreferenceManager serverPreferenceManager)
        {
            _serverPreferenceManager = serverPreferenceManager;
        }
    }
}