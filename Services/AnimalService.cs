using BraimChallenge.Helpers;
using BraimChallenge.IServices;
using BraimChallenge.RequestBody;

namespace BraimChallenge.Services
{
    public class AnimalService : IAnimalService
    {
        public int CheckingForZero(IDetecter _detecters, double? weight, double? height, double? length, long? chipperid, long? chippinglocationid)
        {
            if (_detecters?.DetectId((long?)weight) != 200 || _detecters.DetectId((long?)height) != 200 ||
                _detecters?.DetectId((long?)length) != 200 || _detecters.DetectId(chipperid) != 200 ||
                _detecters?.DetectId(chippinglocationid) != 200) return (int)Status.error;

            return (int)Status.success;
        }

        public int CheckGender(string value)
        {
            if (value is null || !new string[3] { "MALE", "FEMALE", "OTHER" }.Contains(value)) return (int)Status.error;

            return (int)Status.success;
        }

        public int CheckLifeStatus(string value)
        {
            if (value is null || !new string[2] { "ALIVE", "DEAD" }.Contains(value)) return (int)Status.error;

            return (int)Status.success;
        }
    }
}
