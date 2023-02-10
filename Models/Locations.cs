using BraimChallenge.Context;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BraimChallenge.Models
{
    public class Locations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class LocationsContext : ApplicationContext
    {
        public DbSet<Locations> locations { get; set; }
    }
}
