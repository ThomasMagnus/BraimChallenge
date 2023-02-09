using Microsoft.AspNetCore.Mvc;

namespace BraimChallenge.Controllers
{
    public class Location : Controller
    {
        [HttpGet, Route("locations/{pointId?}")]
        public JsonResult Information()
        {
            return Json("");
        }
    }
}
