using BraimChallenge.IServices;
using BraimChallenge.Helpers;
using BraimChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BraimChallenge.Controllers
{
    [ApiController]
    public class AnimalsType : Controller
    {
        private IDetecter? _detecter;
        private IValidator _validator;
        private IAnimalTypeService _animalTypeService;

        public AnimalsType(IDetecter detecter, IValidator validator, IAnimalTypeService animalTypeService)
        {
            _detecter = detecter;
            _validator = validator;
            _animalTypeService = animalTypeService;
        }

        // API 1: Получение информации о типе животного
        [HttpGet, Route("animals/types/{typeID?}")]
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

        //API 2: Добавление типа животного 
        [HttpPost, Route("animals/types")]
        public IActionResult AddAnimalType([FromHeader][Required] string Authorize, [FromBody][Required] string type)
        {
            Object lockObject = new Object();

            lock(lockObject)
            {
                if (_animalTypeService.DetectType(type) != (int)Status.success) return StatusCode((int)Status.error);

                if (_detecter?.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

                using AnimalTypeContext animalTypeContext = new();
                List<AnimalType> animalTypeList = animalTypeContext.animaltype.ToList();

                if (animalTypeList.Any(x => x.type.ToLower().Trim() == type)) return StatusCode((int)Status.isDouble);

                AnimalType animalType = new AnimalType()
                {
                    type = type.ToLower().Trim(),
                };

                animalTypeContext.AddAsync(animalType);
                animalTypeContext.SaveChangesAsync();
            }

            lock (lockObject)
            {
                using AnimalTypeContext animalTypeContext = new();
                List<AnimalType> animalTypeList = animalTypeContext.animaltype.ToList();

                AnimalType? animalType = animalTypeList.FirstOrDefault(x => x.type == type);

                return Json(animalType);
            }
        }
    }
}
