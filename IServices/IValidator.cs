using BraimChallenge.RequestBody;
using Microsoft.AspNetCore.Mvc;

namespace BraimChallenge.IServices
{
    public interface IValidator
    {
        public bool ValidateEmail(AccountBody value);
        public bool ValidateData(AccountBody value);
        public int DataValidator(AccountBody value);
    }
}
