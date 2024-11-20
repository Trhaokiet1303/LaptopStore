using LaptopStore.Application.Responses.Audit;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LaptopStore.Client.Infrastructure.Managers.Audit;
using LaptopStore.Shared.Constants.Application;
using LaptopStore.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace LaptopStore.Client.Pages.Admin.Utilities
{
    public partial class AuditTrails
    {
        [Inject] private IAuditManager AuditManager { get; set; }

        public List<RelatedAuditTrail> Trails = new();

        private RelatedAuditTrail _trail = new();
        private string _searchString = "";
        private MudDateRangePicker _dateRangePicker;
        private DateRange _dateRange;

        private ClaimsPrincipal _currentUser;
        private bool _canSearchAuditTrails;
        private bool _loaded;

        private bool Search(AuditResponse response)
        {
            var result = false;

            if (string.IsNullOrWhiteSpace(_searchString)) result = true;
            if (!result)
            {
                if (response.TableName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = true;
                }
                if (response.OldValues?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = true;
                }
                if (response.NewValues?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                {
                    result = true;
                }
            }

            if (_dateRange?.Start == null && _dateRange?.End == null) return result;
            if (_dateRange?.Start != null && response.DateTime < _dateRange.Start)
            {
                result = false;
            }
            if (_dateRange?.End != null && response.DateTime > _dateRange.End + new TimeSpan(0,11, 59, 59, 999))
            {
                result = false;
            }

            return result;
        }

        protected override async Task OnInitializedAsync()
        {
            _currentUser = await _authenticationManager.CurrentUser();
            _canSearchAuditTrails = (await _authorizationService.AuthorizeAsync(_currentUser, Permissions.AuditTrails.Search)).Succeeded;

            await GetDataAsync();
            _loaded = true;
        }

        private async Task GetDataAsync()
        {
            var response = await AuditManager.GetCurrentUserTrailsAsync();
            if (response.Succeeded)
            {
                Trails = response.Data
                    .Select(x => new RelatedAuditTrail
                    {
                        AffectedColumns = x.AffectedColumns,
                        DateTime = x.DateTime,
                        Id = x.Id,
                        NewValues = x.NewValues,
                        OldValues = x.OldValues,
                        PrimaryKey = x.PrimaryKey,
                        TableName = x.TableName,
                        Type = x.Type,
                        UserId = x.UserId,
                        LocalTime = DateTime.SpecifyKind(x.DateTime, DateTimeKind.Utc).ToLocalTime()
                    }).ToList();
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
        }

        private void ShowBtnPress(int id)
        {
            _trail = Trails.First(f => f.Id == id);
            foreach (var trial in Trails.Where(a => a.Id != id))
            {
                trial.ShowDetails = false;
            }
            _trail.ShowDetails = !_trail.ShowDetails;
        }

        public class RelatedAuditTrail : AuditResponse
        {
            public bool ShowDetails { get; set; } = false;
            public DateTime LocalTime { get; set; }
        }
    }
}