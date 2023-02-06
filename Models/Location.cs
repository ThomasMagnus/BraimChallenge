using Microsoft.AspNetCore.Mvc;

namespace BraimChallenge.Models
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
