using Microsoft.AspNetCore.Mvc;

namespace BraimChallenge.Models
{
    public class UserHeaders
    {
        [FromHeader]
        public string UserParams { get; set; }
    }
}
