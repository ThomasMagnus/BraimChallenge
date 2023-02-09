using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BraimChallenge.Context;
using BraimChallenge.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BraimChallenge.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }

        //public static List<Account> accountList = new List<Account>();
        //public static Account authAccount { get; set; }

    }

    public class AccountContext : ApplicationContext
    {
        public DbSet<Account> account { get; set; }
    }
}
