using App.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppMvc.Areas.AdminCPControllers{
    [Area("AdminCP")]
    [Authorize(Roles = RoleName.Administrator)]
    public class AdminCP : Controller{
        [Route("/admincp" )]
        public IActionResult Index() => View();
    }
}