using BraimChallenge.IServices;
using BraimChallenge.Helpers;
using BraimChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BraimChallenge.Controllers
{
    public class AnimalsType : Controller
    {
        private IDetecter? _detecter;
        private IValidator _validator;

        public AnimalsType(IDetecter detecter, IValidator validator)
        {
            _detecter = detecter;
            _validator = validator;
        }

        // API 1: Получение информации о типе животного
        [HttpGet, Route("animals/types/{typeId}")]
        public IActionResult Information([FromHeader][Required] string Authorize, long? typeID)
        {
            if (_detecter?.DetectId(typeID) != 200) return StatusCode((int)Status.error);

            if (_detecter.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            using AnimalTypeContext animalTypeContext = new();
            List<AnimalType> animalTypeList = animalTypeContext.animaltype.ToList();
            AnimalType? animalType = animalTypeList.FirstOrDefault(x => x.id == typeID);

            if (animalType is null) return StatusCode((int)Status.isNotId);

            return Json(animalType);
        }
    }
}
