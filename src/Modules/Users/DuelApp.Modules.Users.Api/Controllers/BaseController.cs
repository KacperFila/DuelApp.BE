using Microsoft.AspNetCore.Mvc;

namespace DuelApp.Modules.Users.Api.Controllers
{
    [ApiController]
    [Route(UsersModule.BasePath + "/[controller]")]
    internal abstract class BaseController : ControllerBase
    {
        protected ActionResult<T> OkOrNotFound<T>(T model)
        {
            if (model is not null)
            {
                return Ok(model);
            }

            return NotFound();
        }
    }
}