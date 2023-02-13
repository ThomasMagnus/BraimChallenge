using BraimChallenge.Helpers;
using BraimChallenge.IServices;
using BraimChallenge.Models;
using BraimChallenge.RequestBody;

namespace BraimChallenge.Services
{
    public class LocationDetecter : ILocationDetecter
    {
        // StatusCode 400
        public int DetectPointer(double? latitude, double? longitude)
        {
            if (latitude is null || longitude is null || latitude < -90 || latitude > 90
                || longitude < -180 || longitude >180)
            {
                return (int)Status.error;
            }

            return (int)Status.success;
        }

        // StatusCode 409
        public int DetectDoublePointer(LocationBody locationBody, List<Locations> locationList)
        {
            bool result = locationList.Any(x => x.longitude == locationBody.longitude && x.latitude == locationBody.latitude);

            if (result) return (int)Status.isDouble;

            return (int)Status.success;
        }
    }
}
