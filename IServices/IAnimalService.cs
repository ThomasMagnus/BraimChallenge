using BraimChallenge.Models;
using BraimChallenge.RequestBody;

namespace BraimChallenge.IServices
{
    public interface IAnimalService
    {
        public int CheckingForZero(IDetecter _detecters, double? weight, double? height, double? length, long? chipperid, long? chippinglocationid);
        public int CheckGender(string value);
        public int CheckLifeStatus(string value);
    }
}
