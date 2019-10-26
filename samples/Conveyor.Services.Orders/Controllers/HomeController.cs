using Microsoft.AspNetCore.Mvc;

namespace Conveyor.Services.Orders.Controllers
{
    [Route("")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        public ActionResult<string> Get() => "Orders Service";
    }
}