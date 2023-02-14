using Microsoft.AspNetCore.Mvc;
using BraimChallenge.Models;
using System.Text.Json;
using BraimChallenge.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BraimChallenge.RequestBody;
using BraimChallenge.IServices;

namespace BraimChallenge.Controllers
{
    [ApiController]
    public class Accounts : Controller
    {
        private IDetecter? _detecters;
        private IValidator? _validator;

        public Accounts(IDetecter detecter, IValidator? validator)
        {
            _detecters = detecter;
            _validator = validator;
        }

        // API 1: Получение информации об аккаунте пользователя
        [HttpGet, Route("accounts/{accountId?}")]
        public IActionResult Information([FromHeader][Required] string Authorize, long? accountId)
        {

            if (_detecters?.DetectUserAuth(Authorize) != 200) return StatusCode(_detecters.DetectUserAuth(Authorize));

            try
            {
                if (_detecters.DetectId(accountId) != 200) return StatusCode(_detecters.DetectId(accountId));

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

        // API 2: Поиск аккаунтов пользователей по параметрам
        [HttpGet, Route("accounts/search/{firstName?}/{lastName?}/{email?}/{from?}/{size?}")]
        public IActionResult Search([FromHeader][Required] string Authorize, string firstName, string lastName, string email, int? from = 0, int? size = 10)
        {

            using AccountContext accountContext = new();
            List<Account> accountList = accountContext.account.ToList();

            if (_detecters.DetectUserAuth(Authorize) != 200) return StatusCode(_detecters.DetectUserAuth(Authorize));

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

        // API 3: Обновление данных аккаунта пользователя
        [HttpPut, Route("accounts/{accountId?}")]
        public IActionResult UpdateAccount([FromHeader][Required] string Authorize, long? accountId, AccountBody accountBody)
        {
            if (_validator.DataValidator() != 200) return StatusCode(_validator.DataValidator());

            using AccountContext accountContext = new();
            List<Account> accountList = accountContext.account.ToList();

            Account? account = accountList.FirstOrDefault(x => x.id == accountId);

            if (account == null) { return StatusCode((int)Status.isAuth); }

            if (_detecters.DetectAccount(accountId, Authorize, accountList) != 200) { return StatusCode((int)Status.isAuth); }
            
            account.firstName = accountBody.firstName;
            account.lastName = accountBody.lastName;
            account.email = accountBody.email;
            account.password = accountBody.password;

            accountContext.Update(account);
            accountContext.SaveChangesAsync();

            return Json(account);
        }

        // API 4: Удаление аккаунта пользователя
        [HttpDelete, Route("accounts/{accountId?}")]
        public IActionResult DeleteAccount([FromHeader][Required] string Authorize, long? accountId)
        {
            if (_detecters?.DetectId(accountId) != 200) return StatusCode(_detecters.DetectId(accountId));
            if (_detecters?.DetectUserAuth(Authorize) != 200) return StatusCode(_detecters.DetectUserAuth(Authorize));

            using AccountContext accountContext = new();
            List<Account> accountList = accountContext.account.ToList();
            Account? account = accountList.FirstOrDefault(x => x.id == accountId);

            if (account == null) { return StatusCode((int)Status.isAuth); }
            if (_detecters?.DetectAccount(accountId, Authorize, accountList) != 200) { return StatusCode((int)Status.isAuth); }

            accountContext.Remove(account);
            accountContext.SaveChangesAsync();

            return StatusCode((int)Status.success);
        }

    }
}
