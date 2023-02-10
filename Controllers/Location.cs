using Microsoft.AspNetCore.Mvc;
using BraimChallenge.Helpers;
using BraimChallenge.Models;
using System.ComponentModel.DataAnnotations;

namespace BraimChallenge.Controllers
{
    [ApiController]
    public class Location : Controller
    {
        private Detecters _detecters;

        public Location()
        {
            _detecters = new Detecters();
        }

        [HttpGet, Route("locations/{pointId?}")]
        public IActionResult Information([FromHeader][Required] string Authotize, int? pointId)
        {
            if (_detecters.DetectAccountId(pointId) != 200) return StatusCode((int)Status.error);

            using LocationsContext locationsContext = new();
            List<Locations> locationsList = locationsContext.locations.ToList();

            Locations? location = locationsList.FirstOrDefault(x => x.id == pointId);
            if (location is null) return StatusCode((int)Status.isNotId);
            if (_detecters.DetectUserAuth(Authotize) != 200) { return StatusCode((int)Status.notValData); }


            return Json(location);
        }
    }
}
