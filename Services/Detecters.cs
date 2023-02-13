using BraimChallenge.Helpers;
using BraimChallenge.IServices;
using BraimChallenge.Models;
using Microsoft.AspNetCore.Mvc;

namespace BraimChallenge.Services
{
    public class Detecters : IDetecter
    {
        public string[] HeaderData(string header) => header.Replace("Basic", "").Trim().Split(":");

        // Проверка авторизации
        public int DetectUserAuth(string Authorize)
        {
            using AccountContext accountContext = new();
            List<Account> accountList = accountContext.account.ToList();

            string email = HeaderData(Authorize)[0];
            string password = HeaderData(Authorize)[1];

            bool userAuth = accountList.Any(x => x.email == email && x.password == password);

            if (!userAuth) return (int)Status.notValData;

            return (int)Status.success;
        }

        // Проверка accountId
        public int DetectId(int? accountId)
        {
            if (accountId <= 0 || accountId is null)
            {
                return (int)Status.error;
            }

            return (int)Status.success;
        }

        // Проверка, тот ли аккаунт
        public int DetectAccount(int? accountId, string Authorize, List<Account> accountList)
        {
            Account? authAccount = accountList.FirstOrDefault(x => x.email == HeaderData(Authorize)[0]);

            if (authAccount?.id != accountId) return (int)Status.isAuth;

            return (int)Status.success;
        }
    }
}
