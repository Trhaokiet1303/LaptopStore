namespace LaptopStore.Application.Requests.Identity
{
    public class ToggleUserStatusRequest
    {
        public bool ActivateUser { get; set; }
        public bool EmailConfirm { get; set; }
        public string UserId { get; set; }
    }
}