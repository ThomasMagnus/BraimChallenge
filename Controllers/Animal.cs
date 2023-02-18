using BraimChallenge.Helpers;
using BraimChallenge.IServices;
using BraimChallenge.Services;
using BraimChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BraimChallenge.RequestBody;
using Microsoft.EntityFrameworkCore;

namespace BraimChallenge.Controllers
{
    [ApiController]
    public class Animal : Controller
    {

        private IDetecter? _detecters;
        private IValidator? _validator;
        private IAnimalService _animalService;

        // API 1: Получение информации о животном
        public Animal(IDetecter detecters, IValidator validator, IAnimalService animalService)
        {
            _detecters = detecters;
            _validator = validator;
            _animalService = animalService;
        }

        [HttpGet, Route("animals/{animalId?}")]
        public IActionResult Information([FromHeader][Required] string Authorize, long? animalId)
        {
            if (_detecters?.DetectId(animalId) != 200) return StatusCode((int)Status.error);
            if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            using AnimalsContext animalsContext = new();
            List<Animals> animalsList = animalsContext.animal.ToList();

            Animals? animals = animalsList.FirstOrDefault(x => x.id == animalId);

            if (animals is null) return StatusCode((int)Status.isNotId);

            return Json(animals);
        }

        [HttpGet, Route("animals/search/")]
        public JsonResult Search([FromHeader][Required] string Authorize, [FromQuery] DateTime startDateTime, [FromQuery] DateTime endDateTime,
                                    [FromQuery] int? chipperId, [FromQuery] long? chippingLocationId, [FromQuery] string lifeStatus, [FromQuery] string gender,
                                    [FromQuery] int? from, [FromQuery] int? size)
        {

            return Json("");
        }

        // API 3: Добавление нового животного 
        [HttpPost, Route("animals")]
        public async Task<IActionResult> AddAnimal([FromHeader][Required] string Authorize, AnimalsBody animalBody)
        {
            if (_animalService.CheckingForZero(_detecters, animalBody) != 200) return StatusCode((int)Status.error);
            if (animalBody.gender is null || !new string[3] { "MALE", "FEMALE", "OTHER" }.Contains(animalBody.gender)) return StatusCode((int)Status.error);
            if (animalBody.animaltypes is null || animalBody.animaltypes.Length <= 0) return StatusCode((int)Status.error);
            if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);


            using AnimalsContext animalsContext = new();
            Animals animals = new Animals()
            {
                animaltypes = animalBody.animaltypes,
                weight = animalBody.weight,
                height = animalBody.height,
                length = animalBody.length,
                gender = animalBody.gender,
                chipperid = animalBody.chipperid,
                chippinglocationid = animalBody.chippinglocationid,
            };
            
            try
            {
                await animalsContext.AddAsync(animals);
                await animalsContext.SaveChangesAsync();

                Animals? animal = animalsContext.animal.ToList().FirstOrDefault(x => x.length == animalBody.length && x.weight == animalBody.weight && x.height == animalBody.height);

                return Json(animal);
            }
            catch (DbUpdateException ex) when (ex.Message == "An error occurred while saving the entity changes. See the inner exception for details.")
            {
                return StatusCode((int)Status.error);
            }
        }
    }
}
