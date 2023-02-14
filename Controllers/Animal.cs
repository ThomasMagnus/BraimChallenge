using Microsoft.AspNetCore.Mvc;

namespace BraimChallenge.Controllers
{
    public class Animal : Controller
    {
        [HttpGet, Route("animals/{animalId?}")]
        public JsonResult Information()
        {
            return Json("");
        }

        [HttpGet, Route("animals/search")]
        public JsonResult Search()
        {
            return Json("");
        }
    }
}
