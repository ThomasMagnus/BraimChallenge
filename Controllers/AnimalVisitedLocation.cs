﻿using BraimChallenge.IServices;
using BraimChallenge.Helpers;
using BraimChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Npgsql;
using BraimChallenge.Context;

namespace BraimChallenge.Controllers
{
    [ApiController]
    [Route("animals")]
    public class AnimalVisitedLocation : Controller
    {
        private IDetecter _detecter;
        
        public AnimalVisitedLocation(IDetecter detecter)
        {
            _detecter = detecter;
        }
        // API 1: Просмотр точек локации, посещенных животным
        [HttpGet, Route("{animalId:int}/locations")]
        public async Task<IActionResult> Information([FromHeader][Required] string Authorize, long? animalId, [FromQuery] string? startDateTime, [FromQuery] string? endDateTime,
            [FromQuery][Required] int? size, [FromQuery][Required] int from = 0)
        {
            if (_detecter.DetectId(animalId) != 200) return StatusCode((int)Status.error);
            if (from < 0 || size <= 0) return StatusCode((int)Status.error);
            if (_detecter.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            using AnimalsContext animalsContext = new();

            List<Animals> animalsList = animalsContext.animal.ToList();
            Animals? animals = animalsList.FirstOrDefault(x => x.id == animalId);

            if (animals is null) return StatusCode((int)Status.isNotId);

            DateTime startDate, endDate;
            List<AnimalVisited> animalVisiteds = new List<AnimalVisited>();

            string commandString = $"SELECT * FROM animalvisitedlocation WHERE id={animalId}";

            using (NpgsqlConnection connection = new NpgsqlConnection(ApplicationContext.connectionString))
            {
                connection.Open();
                List <KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> requestQuery = Request.Query.ToList();

                for (int i = 0; i < requestQuery.Count; i++)
                {
                    if (requestQuery[i].Key == "startDateTime")
                    {
                        if (!DateTime.TryParseExact(startDateTime, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out startDate))
                            return StatusCode((int)Status.error);
                        commandString += $" AND datetimeofvisitlocationpoint>={startDateTime}";
                        continue;
                    } 
                    else if (requestQuery[i].Key == "endDateTime")
                    {
                        if (!DateTime.TryParseExact(endDateTime, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out endDate)) 
                            return StatusCode((int)Status.error);
                        commandString += $" AND datetimeofvisitlocationpoint<={endDateTime}";
                        continue;
                    }

                    if (i >= requestQuery.Count - 3) break;
                }

                commandString += $" ORDER BY id LIMIT {size} OFFSET {from}";

                using (NpgsqlCommand command = new NpgsqlCommand(commandString, connection))
                {
                    try
                    {
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            animalVisiteds.Add(new AnimalVisited()
                            {
                                id = long.Parse(reader["id"].ToString()),
                                animalid = long.Parse(reader["animalid"].ToString()),
                                locationpointid = long.Parse(reader["locationpointid"].ToString()),
                                datetimeofvisitlocationpoint = DateTime.Parse(reader["datetimeofvisitlocationpoint"].ToString())
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            return Json(animalVisiteds);
        }

        // API 2: Добавление точки локации, посещенной животным
        [HttpPost, Route("/{animalId}/locations/{pointId}")]
        public IActionResult AddPointAnimalLocation([FromHeader][Required] string Authorize, long animalId, long pointId)
        {
            if (_detecter.DetectId(animalId) != 200 || _detecter.DetectId(pointId) != 200) return StatusCode((int)Status.error);
            if (_detecter.DetectUserAuth(Authorize) != 200) return StatusCode((int)Status.notValData);

            using (AnimalVisitedContext animalVisitedContext = new())
            {
                using (AnimalsContext animalContext = new())
                {
                    List<Animals> animalsList = animalContext.animal.ToList();
                    Animals? animals = animalsList.FirstOrDefault(x => x.id == animalId);

                    using (LocationsContext locationContext = new())
                    {

                        List<Locations> locationsList = locationContext.locations.ToList();
                        Locations? locations = locationContext.locations.FirstOrDefault(x => x.id == pointId);

                        if (animals is null || locations is null) return StatusCode((int)Status.isNotId);
                        if (animals.lifestatus == "DEAD") return StatusCode((int)Status.error);

                        List<AnimalVisited> animalsVisitedCollection = animalVisitedContext.animalvisited
                            .Where(x => x.animalid == animalId)
                            .OrderBy(x => x.id)
                            .ToList();

                        if (animalsVisitedCollection.Count != 0 && locations.id == animalsVisitedCollection[animalsVisitedCollection.Count - 1].locationpointid) 
                            return StatusCode((int)Status.error);

                    }
                }

                animalVisitedContext.Add(new AnimalVisited()
                {
                    animalid = animalId,
                    locationpointid = pointId,
                });

                animalVisitedContext.SaveChanges();

                List<AnimalVisited> animalsVisitedList = animalVisitedContext.animalvisited.ToList();
                AnimalVisited? animalVisited = animalsVisitedList.LastOrDefault(x => x.animalid == animalId && x.locationpointid == pointId);

                return Json(animalVisited);

            }
        }
    }
}