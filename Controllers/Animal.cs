using BraimChallenge.Helpers;
using BraimChallenge.IServices;
using BraimChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using BraimChallenge.RequestBody;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using BraimChallenge.Context;
using System;

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

        // API 2: Поиск животных по параметрам
        [HttpGet, Route("animals/search/")]
        public IActionResult Search([FromHeader][Required] string Authorize, [FromQuery] DateTime startDateTime, [FromQuery] DateTime endDateTime,
                                    [FromQuery] int? chipperId, [FromQuery] long? chippingLocationId, [FromQuery] string? lifeStatus, [FromQuery] string? gender,
                                    [FromQuery] int? from, [FromQuery] int? size)
        {
            if (from < 0 || size <= 0) return StatusCode((int)Status.error);
            if (_detecters.DetectId(chipperId) != 200 || _detecters.DetectId(chipperId) != 200) return StatusCode((int)Status.error);

            if (!String.IsNullOrEmpty(lifeStatus?.Trim())) if (_animalService.CheckGender(gender) != 200) return StatusCode((int)Status.error);
            if (!String.IsNullOrEmpty(gender?.Trim())) if (_animalService.CheckLifeStatus(lifeStatus) != 200) return StatusCode((int)Status.error);

            if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            List<Animals> animalsArray = new List<Animals>();

            using (NpgsqlConnection connection = new NpgsqlConnection(ApplicationContext.connectionString))
            {
                string commandString = "SELECT * FROM animal WHERE ";
                try
                {
                    connection.Open();
                    List<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> requestQuery = Request.Query.ToList();

                    for (int i = 0; i < requestQuery.Count; i++)
                    {
                        if (requestQuery[i].Key == "startDateTime")
                        {
                            commandString += $"chippingdatetime>='{requestQuery[i].Value}' AND ";
                            continue;
                        }
                        else if (requestQuery[i].Key == "endDateTime")
                        {
                            commandString += $"chippingdatetime<='{requestQuery[i].Value}' AND ";
                            continue;
                        }

                        if (i == Request.Query.Count - 3)
                        {
                            commandString += $"{requestQuery[i].Key}='{requestQuery[i].Value}'";
                            break;
                        }

                        commandString += $"{requestQuery[i].Key}='{requestQuery[i].Value}' AND ";
                    }

                    commandString += $" ORDER BY id LIMIT {size} OFFSET {from}";
                } catch (Exception ex) { Console.WriteLine(ex.Message); }

                using (NpgsqlCommand command = new NpgsqlCommand(commandString, connection))
                {
                    try
                    {
                        var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            animalsArray.Add(new Animals
                            {
                                id = int.Parse(reader["id"].ToString()),
                                animaltypes = (long[])reader["animaltypes"],
                                weight = int.Parse(reader["weight"].ToString()),
                                length = int.Parse(reader["length"].ToString()),
                                height = int.Parse(reader["height"].ToString()),
                                gender = reader["gender"].ToString(),
                                chipperid = int.Parse(reader["height"].ToString()),
                                chippinglocationid = int.Parse(reader["chippinglocationid"].ToString()),
                                lifestatus = reader["lifestatus"].ToString(),
                                chippingdatetime = (DateTime)reader["chippingdatetime"],
                            });

                        }
                        reader.Close();
                    } catch (Exception ex) { Console.WriteLine(ex.Message); }

                }

                return Json(animalsArray);
            }
        }

        // API 3: Добавление нового животного 
        [HttpPost, Route("animals")]
        public async Task<IActionResult> AddAnimal([FromHeader][Required] string Authorize, AnimalsBody animalBody)
        {
            if (_animalService.CheckingForZero(_detecters, animalBody.weight, animalBody.height, animalBody.length, animalBody.chipperid, animalBody.chippinglocationid) != 200) 
                return StatusCode((int)Status.error);
            if (_animalService.CheckGender(animalBody.gender) != 200) return StatusCode((int)Status.error);
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

        // API 4: Обновление информации о животном 
        [HttpPut, Route("animals/{animalId?}")]
        public async Task<IActionResult> UpdateAnimal([FromHeader][Required] string Authorize, long? animalId, UpdateAnimalBody updateAnimalBody)
        {
            if (_animalService.CheckingForZero(_detecters, updateAnimalBody.weight, updateAnimalBody.height,
                updateAnimalBody.length, updateAnimalBody.chipperid, updateAnimalBody.chippinglocationid) != 200)
            {
                return StatusCode((int)Status.error);
            }

            if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            if (_animalService.CheckGender(updateAnimalBody.gender) != 200) return StatusCode((int)Status.error);
            if (_animalService.CheckLifeStatus(updateAnimalBody.lifestatus) != 200) return StatusCode((int)Status.error);

            using AnimalsContext animalsContext = new();
            using AccountContext accountContext = new AccountContext();
            using LocationsContext locationContext = new LocationsContext();

            List<Animals> animalsList = animalsContext.animal.ToList();
            List<Account> accountList = accountContext.account.ToList();
            List<Locations> locationsList = locationContext.locations.ToList();

            Animals? animal = animalsList.FirstOrDefault(x => x.id == animalId);
            Account? account = accountList.FirstOrDefault(x => x.id == updateAnimalBody.chipperid);
            Locations? locations = locationsList.FirstOrDefault(x => x.id == updateAnimalBody.chippinglocationid);

            if (animal is null || account is null || locations is null) return StatusCode((int)Status.isNotId);
            if (animal.lifestatus == "DEAD" && updateAnimalBody.lifestatus == "ALIVE") return StatusCode((int)Status.error);
            //if (locations.id == updateAnimalBody.chippinglocationid) return StatusCode((int)Status.error);

            animal.weight = updateAnimalBody.weight;
            animal.length = updateAnimalBody.length;
            animal.height = updateAnimalBody.height;
            animal.gender = updateAnimalBody.gender;
            animal.lifestatus = updateAnimalBody.lifestatus;
            animal.chipperid = updateAnimalBody.chipperid;
            animal.chippinglocationid = updateAnimalBody.chippinglocationid;
            animal.deathdatetime = updateAnimalBody.lifestatus == "DEAD" ? DateTime.UtcNow : null;

            animalsContext.Update(animal);
            await animalsContext.SaveChangesAsync();

            return Json(animal);
        }

        // API 5: Удаление животного
        [HttpDelete, Route("animals/{animalId?}")]
        public async Task<IActionResult> DeleteAnimal([FromHeader][Required] string Authorize, long? animalId)
        {
            if (_detecters?.DetectId(animalId) != 200) return StatusCode((int)Status.error);
            if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            using AnimalsContext animalsContext = new AnimalsContext();
            List<Animals> animalsList = animalsContext.animal.ToList();

            Animals? animal = animalsList.FirstOrDefault(x => x.id == animalId);

            if (animal is null) return StatusCode((int)Status.isNotId);

            animalsContext.Remove(animal);
            await animalsContext.SaveChangesAsync();

            return StatusCode((int)Status.success);
            
        }

        // API 6: Добавление типа животного к животному
        [HttpGet, Route("animals/{animalId?}/types/{typeId?}")]
        public async Task<IActionResult> AddTypeAnimal([FromHeader][Required] string Authorize, long? animalId, long typeId)
        {
            if (_detecters?.DetectId(animalId) != 200 || _detecters.DetectId(typeId) != 200) return StatusCode((int)Status.error);
            if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            using AnimalsContext animalsContext = new AnimalsContext();
            using AnimalTypeContext animalTypeContext = new();

            List<Animals> animalsList = animalsContext.animal.ToList();
            List<AnimalType> animalTypeList = animalTypeContext.animaltype.ToList();

            Animals? animal = animalsList.FirstOrDefault(x => x.id == animalId);
            AnimalType? animalType = animalTypeList.FirstOrDefault(x => x.id == typeId);

            if (animal is null || animalType is null) return StatusCode((int)Status.isNotId);
            long[] animalTypesArray = new long[] { };

            Array.Resize<long>(ref animalTypesArray, animal.animaltypes!.Length + 1);

            Array.Copy(animal.animaltypes, animalTypesArray, animal.animaltypes.Length);

            animalTypesArray[animalTypesArray.Length - 1] = typeId;

            animal.animaltypes = animalTypesArray;

            animalsContext.Update(animal);
            await animalsContext.SaveChangesAsync();

            return Json(animal);
        }

        // API 7: Изменение типа животного у животного
        [HttpPut, Route("animals/{animalId?}/types")]
        public async Task<IActionResult> UpdateAnimalType([FromHeader][Required] string Authorize, long? animalId, UpdateAnimalType updateAnimalType)
        {
            if (_detecters?.DetectId(animalId) != 200 || _detecters.DetectId(updateAnimalType.oldTypeId) != 200 || _detecters.DetectId(updateAnimalType.newTypeId) != 200)
                return StatusCode((int)Status.error);

            if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            using AnimalsContext animalsContext = new();
            using AnimalTypeContext animalTypeContext = new();

            List<Animals> animalsList = animalsContext.animal.ToList();
            List<AnimalType> animalTypeList = animalTypeContext.animaltype.ToList();

            Animals? animal = animalsList.FirstOrDefault(x => x.id == animalId);
            AnimalType? animalTypeOld = animalTypeList.FirstOrDefault(x => x.id == updateAnimalType.oldTypeId);
            AnimalType? animalTypeNew = animalTypeList.FirstOrDefault(x => x.id == updateAnimalType.newTypeId);

            if (animal is null || animalTypeOld is null || animalTypeNew is null) return StatusCode((int)Status.isNotId);
            if (!animal.animaltypes!.Contains(updateAnimalType.oldTypeId)) return StatusCode((int)Status.isNotId);
            if (animal.animaltypes!.Contains(updateAnimalType.newTypeId)) return StatusCode((int)Status.isContains);

            int index = Array.IndexOf(animal.animaltypes, updateAnimalType.oldTypeId);
            animal.animaltypes[index] = updateAnimalType.newTypeId;
            animalsContext.Update(animal);
            await animalsContext.SaveChangesAsync();

            return Json(animal);
        }

        // API 8: Удаление типа животного у животного
        [HttpDelete, Route("animals/{animalId?}/types/{typeId?}")]
        public async Task<IActionResult> DeleteAnimalType([FromHeader][Required] string Authorize, long? animalId, long typeId)
        {
            if (_detecters.DetectId(animalId) != 200 || _detecters.DetectId(typeId) != 200) return StatusCode((int)Status.error);
            if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            using AnimalsContext animalsContext = new();
            using AnimalTypeContext animalTypeContext = new();

            List<Animals> animalsList = animalsContext.animal.ToList();
            List<AnimalType> animalTypeList = animalTypeContext.animaltype.ToList();

            Animals? animal = animalsList.FirstOrDefault(x => x.id == animalId);
            AnimalType? animalType = animalTypeList.FirstOrDefault(x => x.id == typeId);

            if (animal is null || animalType is null) return StatusCode((int)Status.isNotId);
            if (!animal.animaltypes.Contains(typeId)) return StatusCode((int)Status.isNotId);

            if (animal.animaltypes.Length == 1) return StatusCode((int)Status.error);

            long[] longs = new long[] {};
            Array.Resize(ref longs, animal.animaltypes.Length - 1);

            longs = animal.animaltypes.Where(x => x != typeId).ToArray();
            animal.animaltypes = longs;
            animalsContext.Update(animal);
            await animalsContext.SaveChangesAsync();

            return Json(animal);
        }
    }
}
