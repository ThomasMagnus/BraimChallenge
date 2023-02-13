using Microsoft.AspNetCore.Mvc;

namespace BraimChallenge.IServices
{
    public interface IValidator
    {
        public bool ValidateEmail();
        public bool ValidateData();
        public int DataValidator();
    }
}
