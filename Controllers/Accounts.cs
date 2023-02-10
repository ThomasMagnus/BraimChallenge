using Microsoft.AspNetCore.Mvc;
using BraimChallenge.Models;
using System.Text.Json;
using BraimChallenge.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BraimChallenge.Controllers
{
    [ApiController]
    public class Accounts : Controller
    {
        // Регистрация пользователя
        [HttpPost, Route("registration")]
        public IActionResult Registration(AccountBody value)
        {
            Helpers.Validator validator = new Helpers.Validator { value = value };

            // Проверка на пустоту вводимых значений и на повторение email
            if (validator.DataValidator() != 200) return StatusCode(validator.DataValidator());
            object locker = new object();

            lock(locker)
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

            lock(locker)
            {
                using AccountContext baseAccountContext = new();
                List<Account>? baseAccountResult = baseAccountContext?.account.ToList();

                Account? baseAccount = baseAccountResult?.FirstOrDefault(x => x?.email == value.email);

                return Json(baseAccount);
            }
        }

        // Поиск пользователя по ID
        [HttpGet, Route("accounts/{accountId?}")]
        public IActionResult Information([FromHeader][Required] string Authorize, int? accountId)
        {

            if (DetectUserAuth(Authorize) != 200) return StatusCode(DetectUserAuth(Authorize));

            try
            {
                if (accountId <= 0 || accountId is null)
                {
                    return StatusCode((int)Status.error);
                }

                using AccountContext accountContext = new();
                List<Account> accountList = accountContext.account.ToList();

                Account? account = accountList.FirstOrDefault(x => x.id == accountId);

                if (account is null) return StatusCode((int)Status.isNotId);

                return Json(account);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode((int)Status.notValData);
            }
        }

        // Поиск пользоваелей по параметрам
        [HttpGet, Route("accounts/search/{firstName?}/{lastName?}/{email?}/{from?}/{size?}")]
        public IActionResult Search([FromHeader][Required] string Authorize, string firstName, string lastName, string email, int? from = 0, int? size = 10)
        {

            using AccountContext accountContext = new();
            List<Account> accountList = accountContext.account.ToList();

            if (DetectUserAuth(Authorize) != 200) return StatusCode(DetectUserAuth(Authorize));

            if (from is null || from < 0 
                || size is null || size <= 0)
            {
                return StatusCode((int)Status.error);
            };

            Account[]? account = accountList.Where(x => x.firstName.ToLower().Contains(firstName.ToLower()) 
                                                            && x.lastName.ToLower().Contains(lastName.ToLower()) 
                                                            && x.email.ToLower().Contains(email.ToLower())).Skip((int)from).Take((int)size).OrderBy(x => x.id).ToArray();

            if (account.Length == 0) return StatusCode((int)Status.notValData);

            return Json(account);
        }

        // Изменение данных пользователя
        [HttpPut, Route("accounts/{accountId?}")]
        public IActionResult UpdateAccount([FromHeader][Required] string Authorize, int? accountId, AccountBody accountBody)
        {

            Helpers.Validator validator = new Helpers.Validator { value = accountBody };

            if (validator.DataValidator() != 200) return StatusCode(validator.DataValidator());

            using AccountContext accountContext = new();
            List<Account> accountList = accountContext.account.ToList();

            Account? account = accountList.FirstOrDefault(x => x.id == accountId);

            if (account == null) { return StatusCode((int)Status.isAuth); }

            Account? authAccount = accountList.FirstOrDefault(x => x.email == HeaderData(Authorize)[0]);

            if (authAccount?.id != accountId) return StatusCode((int)Status.isAuth);
            
            account.firstName = accountBody.firstName;
            account.lastName = accountBody.lastName;
            account.email = accountBody.email;
            account.password = accountBody.password;

            accountContext.Update(account);
            accountContext.SaveChangesAsync();

            return Json(account);
        }

        // Извелечение данных из заголовка
        [NonAction]
        private string[] HeaderData(string header) => header.Replace("Basic", "").Trim().Split(":");

        // Проверка авторизации
        [NonAction]
        private int DetectUserAuth(string Authorize)
        {
            using AccountContext accountContext = new();
            List<Account> accountList = accountContext.account.ToList();

            string email = HeaderData(Authorize)[0];
            string password = HeaderData(Authorize)[1];

            bool userAuth = accountList.Any(x => x.email == email && x.password == password);

            if (!userAuth) return (int)Status.notValData;

            return (int)Status.success;
        }
    }
}
