using Microsoft.AspNetCore.Mvc;

namespace DuelApp.Modules.Users.Api.Controllers
{
    [Route(UsersModule.BasePath)]
    internal class HomeController : BaseController
    {
        [HttpGet]
        public ActionResult<string> Get() => "Users API";
    }
}