using BraimChallenge.Models;
using Microsoft.AspNetCore.Mvc;
using BraimChallenge.Context;
using BraimChallenge.RequestBody;
using BraimChallenge.IServices;
using BraimChallenge.Helpers;

namespace BraimChallenge.Services
{
    public class Validator : IValidator
    {
        public AccountBody value { get; set; }

        // Проверка на значения, которые вводит пользователь
        public bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(value.firstName) || string.IsNullOrWhiteSpace(value.lastName) ||
                string.IsNullOrWhiteSpace(value.password) || string.IsNullOrWhiteSpace(value.email))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // Проверка, зарегестрирован ли пользователь с таким email
        public bool ValidateEmail()
        {
            using AccountContext accountContext = new();
            List<Account> accountList = accountContext.account.ToList();

            foreach (Account item in accountList)
            {
                if (item.email == value.email)
                {
                    return false;
                }
            }

            return true;
        }

        // Объединение проверок
        public int DataValidator()
        {
            if (!ValidateData()) return (int)Status.error;

            using AccountContext accountContext = new();
            List<Account> accountList = accountContext.account.ToList();

            if (accountList.Count != 0)
            {
                if (!ValidateEmail()) return (int)Status.isDouble;
            }

            return (int)Status.success;
        }
    }
}
