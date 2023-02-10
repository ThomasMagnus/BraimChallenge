using BraimChallenge.Helpers;
using BraimChallenge.Models;
using Microsoft.AspNetCore.Mvc;

namespace BraimChallenge.Controllers
{
    [ApiController]
    public class Authentication : Controller
    {
        // Регистрация пользователя
        [HttpPost, Route("registration")]
        public IActionResult Registration(AccountBody value)
        {
            Helpers.Validator validator = new Helpers.Validator { value = value };

            // Проверка на пустоту вводимых значений и на повторение email
            if (validator.DataValidator() != 200) return StatusCode(validator.DataValidator());
            object locker = new object();

            lock (locker)
            {
                using AccountContext accountContext = new();
                List<Account>? accountResult = accountContext.account.ToList();

                // Проверка, авторизован ли уже пользователь
                if (accountResult.Any(x => x.firstName == value.firstName && x.lastName == value.lastName && x.email == value.email)) return StatusCode((int)Status.isAuth);

                Account? account = new Account()
                {
                    firstName = value.firstName,
                    lastName = value.lastName,
                    email = value.email,
                    password = value.password
                };

                accountContext?.AddAsync(account);
                accountContext?.SaveChangesAsync();
            }

            lock (locker)
            {
                using AccountContext baseAccountContext = new();
                List<Account>? baseAccountResult = baseAccountContext?.account.ToList();

                Account? baseAccount = baseAccountResult?.FirstOrDefault(x => x?.email == value.email);

                return Json(baseAccount);
            }
        }
    }
}
