using Microsoft.AspNetCore.Mvc;
using BraimChallenge.IServices;
using BraimChallenge.Helpers;
using BraimChallenge.Models;
using System.ComponentModel.DataAnnotations;
using BraimChallenge.RequestBody;
using BraimChallenge.Services;

namespace BraimChallenge.Controllers
{
    [ApiController]
    public class Location : Controller
    {
        private IDetecter _detecters;
        private IValidator _validator;
        private ILocationDetecter _locationDetecter;

        public Location(IDetecter detecter, IValidator validator, ILocationDetecter locationDetecter)
        {
            _detecters = detecter;
            _validator = validator;
            _locationDetecter = locationDetecter;
        }

        // API 1: Получение информации о точке локации животных 
        [HttpGet, Route("locations/{pointId?}")]
        public IActionResult Information([FromHeader][Required] string Authotize, int? pointId)
        {
            if (_detecters.DetectId(pointId) != 200) return StatusCode((int)Status.error);

            using LocationsContext locationsContext = new();
            List<Locations> locationsList = locationsContext.locations.ToList();

            Locations? location = locationsList.FirstOrDefault(x => x.id == pointId);
            if (location is null) return StatusCode((int)Status.isNotId);
            if (_detecters.DetectUserAuth(Authotize) != 200) { return StatusCode((int)Status.notValData); }

            return Json(location);
        }

        // API 2: Добавление точки локации животных
        [HttpPost, Route("locations")]
        public IActionResult LocationPost([FromHeader][Required] string Authorize, LocationBody locationBody)
        {
            object lockObject = new Object();

            lock(lockObject)
            {
                if (_locationDetecter.DetectPointer(locationBody.latitude, locationBody.longitude) != 200) return StatusCode((int)Status.error);
                if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

                using LocationsContext locationsContext = new();
                List<Locations> locationsList = locationsContext.locations.ToList();

                if (_locationDetecter.DetectDoublePointer(locationBody, locationsList) != 200) return StatusCode((int)Status.isDouble);

                Locations location = new Locations()
                {
                    latitude = locationBody.latitude,
                    longitude = locationBody.longitude,
                };

                locationsContext.AddAsync(location);
                locationsContext.SaveChangesAsync();
            }

            lock(lockObject)
            {
                using LocationsContext locationsContext = new();
                List<Locations> locationsList = locationsContext.locations.ToList();
                Locations? locationsResult = locationsList.FirstOrDefault(x => x.latitude == locationBody.latitude && x.longitude == locationBody.longitude);
                return Json(locationsResult);
            }
        }

        // API 3: Изменение точки локации животных 
        [HttpPut, Route("locations/{pointId?}")]
        public IActionResult UpdateLocationPoint([FromHeader][Required] string Authorize, int? pointId, LocationBody locationBody)
        {
            if (_detecters.DetectId(pointId) != 200) return StatusCode((int)Status.error);
            if (_locationDetecter.DetectPointer(locationBody.latitude, locationBody.longitude) != 200) return StatusCode((int)Status.error);
            if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            using LocationsContext locationsContext = new();
            List<Locations> locationsList = locationsContext.locations.ToList();

            if (_locationDetecter.DetectDoublePointer(locationBody, locationsList) != 200) return StatusCode((int)Status.isDouble);

            Locations? locations = locationsList.FirstOrDefault(x => x.id == pointId);

            if (locations is null) return StatusCode((int)Status.isNotId);

            locations.latitude = locationBody.latitude;
            locations.longitude = locationBody.longitude;

            locationsContext.Update(locations);
            locationsContext.SaveChangesAsync();

            return Json(locations);
        }

        // API 4: Удаление точки локации животных 
    }
}
