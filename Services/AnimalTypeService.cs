using BraimChallenge.Helpers;
using BraimChallenge.IServices;
using static System.String;

namespace BraimChallenge.Services
{
    public class AnimalTypeService : IAnimalTypeService
    {
        public int DetectType(string type)
        {
            if (IsNullOrEmpty(type) || IsNullOrWhiteSpace(type)) return (int)Status.error;

            return (int)Status.success;
        }
    }
}
