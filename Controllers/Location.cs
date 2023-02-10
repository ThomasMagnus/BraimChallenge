using Microsoft.AspNetCore.Mvc;
using BraimChallenge.Helpers;
using BraimChallenge.Models;

namespace BraimChallenge.Controllers
{
    public class Location : Controller
    {
        [HttpGet, Route("locations/{pointId?}")]
        public IActionResult Information(int? pointId)
        {
            if (pointId is null || pointId < 0) return StatusCode((int)Status.error);

            using LocationsContext locationsContext = new();
            List<Locations> locationsList = locationsContext.locarions.ToList();

            Locations? locations = locationsList.FirstOrDefault(x => x.id == pointId);
            if (locations != null) return StatusCode((int)Status.isNotId);



            return Json("");
        }
    }
}
