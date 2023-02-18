using BraimChallenge.Models;
using BraimChallenge.RequestBody;

namespace BraimChallenge.IServices
{
    public interface IAnimalService
    {
        public int CheckingForZero(IDetecter _detecters, AnimalsBody animalBody);
    }
}
