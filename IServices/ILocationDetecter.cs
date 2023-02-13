using BraimChallenge.Models;
using BraimChallenge.RequestBody;

namespace BraimChallenge.IServices
{
    public interface ILocationDetecter
    {
        public int DetectPointer(double? latitude, double? longitude);
        public int DetectDoublePointer(LocationBody locationBody, List<Locations> locationList);
    }
}
