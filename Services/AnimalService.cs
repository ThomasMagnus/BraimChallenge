using BraimChallenge.Helpers;
using BraimChallenge.IServices;
using BraimChallenge.RequestBody;

namespace BraimChallenge.Services
{
    public class AnimalService : IAnimalService
    {
        public int CheckingForZero(IDetecter _detecters, AnimalsBody animalBody)
        {
            if (_detecters?.DetectId((long?)animalBody.weight) != 200 || _detecters.DetectId((long?)animalBody.height) != 200 ||
                _detecters?.DetectId((long?)animalBody.length) != 200 || _detecters.DetectId((long?)animalBody.chipperid) != 200 ||
                _detecters?.DetectId((long?)animalBody.chippinglocationid) != 200) return (int)Status.error;

            return (int)Status.success;
        }
    }
}
