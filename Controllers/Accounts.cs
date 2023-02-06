using Microsoft.AspNetCore.Mvc;
using BraimChallenge.Models;
using System.Text.Json;
using BraimChallenge.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace BraimChallenge.Controllers
{
    [ApiController]
    public class Accounts : Controller
    {
        private static int id { get; set; } = 1;

        [HttpPost, Route("registration")]
        public IActionResult Registration(AccountBody value)
        {
            if (!ValidateData(value))
            {
                return StatusCode((int)Status.error);
            }

            //if (String.IsNullOrWhiteSpace(value.firstName) || String.IsNullOrWhiteSpace(value.lastName) ||
            //    String.IsNullOrWhiteSpace(value.password) || String.IsNullOrWhiteSpace(value.email))
            //{
            //    return StatusCode((int)Status.error);
            //}

            if (Account.accountList.Count != 0)
            {
                foreach(Account item in Account.accountList)
                {
                    if (item.email == value.email)
                    {
                        return StatusCode((int)Status.isEmail);
                    }
                }
            }

            if (Account.authAccount is not null && Account.authAccount.firstName == value.firstName
                && Account.authAccount.lastName == value.lastName && Account.authAccount.email == value.email)
            {
                Account.authAccount = new();
                return StatusCode((int)Status.isAuth);
            }

            Account? account = new Account()
            {
                id = id++,
                firstName = value.firstName,
                lastName = value.lastName,
                email = value.email,
                password = value.password
            };

            Account.authAccount = account;

            Account.accountList.Add(account);

            return Json(account);
        }

        [HttpGet, Route("accounts/{accountId?}")]
        public IActionResult Information(int? accountId)
        {
            try
            {
                if (accountId <= 0 || accountId is null)
                {
                    return StatusCode((int)Status.error);
                }

                Account? account = Account.accountList.FirstOrDefault(x => x.id == accountId);

                if (account is null) return StatusCode((int)Status.isNotId);

                return Json(account);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode((int)Status.notValData);
            }
        }

        [HttpGet, Route("accounts/search/{firstName?}/{lastName?}/{email?}/{from?}/{size?}")]
        public IActionResult Search(string firstName, string lastName, string email, int? from = 0, int? size = 10)
        {

            if (from is null || from < 0 
                || size is null || size <= 0)
            {
                return StatusCode((int)Status.error);
            };

            Account[]? account = Account.accountList.Where(x => x.firstName.ToLower().Contains(firstName.ToLower()) 
                                                            && x.lastName.ToLower().Contains(lastName.ToLower()) 
                                                            && x.email.ToLower().Contains(email.ToLower())).Skip((int)from).Take((int)size).ToArray();

            if (account.Length == 0) return StatusCode((int)Status.notValData);

            return Json(account);
        }

        [HttpPut, Route("accounts/{accountId?}")]
        public IActionResult UpdateAccount(int? accountId, AccountBody accountBody)
        {

            if (accountId is null || accountId <= 0) return StatusCode((int)Status.error);

            if (!ValidateData(accountBody))
            {
                return StatusCode((int)Status.error);
            }

            Account? account = Account.accountList.FirstOrDefault(x => x.id == accountId);

            account.firstName = accountBody.firstName;
            account.lastName = accountBody.lastName;
            account.email = accountBody.email;
            account.password = accountBody.password;

            return Json(account);
        }

        [NonAction]
        public bool ValidateData(AccountBody value)
        {
            if (String.IsNullOrWhiteSpace(value.firstName) || String.IsNullOrWhiteSpace(value.lastName) ||
                String.IsNullOrWhiteSpace(value.password) || String.IsNullOrWhiteSpace(value.email))
            {
                return false;
            } else
            {
                return true;
            }
        }
    }
}
