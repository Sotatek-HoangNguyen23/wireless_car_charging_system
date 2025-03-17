using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace wireless_changing_system.Pages.Wireless_charging.Auth
{
    public class Send_OTPModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string FullName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }
        public void OnGet()
        {
        }
    }
}
