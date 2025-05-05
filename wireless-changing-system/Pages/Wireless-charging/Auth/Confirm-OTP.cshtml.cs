using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace wireless_changing_system.Pages.Wireless_charging.Auth
{
    public class Confirm_OTPModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Email { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? ActionType { get; set; } // "register" hoặc "reset-password"
        public void OnGet()
        {
        }
    }
}
