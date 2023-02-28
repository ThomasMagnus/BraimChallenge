using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BraimChallenge.Models
{
    public class AnimalVisited
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long animalid { get; set; }
        public long locationpointid { get; set; }
        public DateTime datetimeofvisitlocationpoint { get; set; }
    }
}
