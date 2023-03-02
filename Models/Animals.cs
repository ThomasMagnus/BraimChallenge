using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BraimChallenge.Context;
using Microsoft.EntityFrameworkCore;

namespace BraimChallenge.Models
{
    public class Animals
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long? id { get; set; }
        public long[]? animaltypes { get; set; }
        public double weight { get; set; }
        public double length { get; set; }
        public double height { get; set; }
        public string? gender { get; set; }
        public int chipperid { get; set; }
        public long? chippinglocationid { get; set; }
        public string? lifestatus { get; set; } = "ALIVE";
        public long[]? visitedlocations { get; set; }
        public DateTime? chippingdatetime { get; set; } = DateTime.UtcNow;
        public DateTime? deathdatetime { get; set; } = null;
    }

    public class AnimalsContext : ApplicationContext
    {
        public DbSet<Animals> animal { get; set; }
    }
}
