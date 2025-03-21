using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace wireless_changing_system.Pages.Wireless_charging.Auth
{
    public class New_PasswordModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Token { get; set; }
        public void OnGet()
        {
        }
    }
}
