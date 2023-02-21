using BraimChallenge.Helpers;
using BraimChallenge.IServices;
using BraimChallenge.Services;
using BraimChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BraimChallenge.RequestBody;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using BraimChallenge.Context;
using System.Collections;
using System.Data.SqlTypes;

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

            if (!String.IsNullOrEmpty(lifeStatus?.Trim())) if (!new string[2] { "ALIVE", "DEAD" }.Contains(lifeStatus.ToUpper())) return StatusCode((int)Status.error);
            if (!String.IsNullOrEmpty(gender?.Trim())) if (!new string[3] { "MALE", "FEMALE", "OTHER" }.Contains(gender.ToUpper())) return StatusCode((int)Status.error);

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
