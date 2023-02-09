using Microsoft.AspNetCore.Mvc;

namespace BraimChallenge.IHelpers
{
    public interface IValidator
    {
        public bool ValidateEmail();
        public bool ValidateData();
        public int DataValidator();
    }
}
