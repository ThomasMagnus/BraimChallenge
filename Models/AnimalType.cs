using BraimChallenge.Context;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BraimChallenge.Models
{
    public class AnimalType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public string? type { get; set; }
    }

    public class AnimalTypeContext : ApplicationContext
    {
        public DbSet<AnimalType> animaltype { get; set; }
    }
}
