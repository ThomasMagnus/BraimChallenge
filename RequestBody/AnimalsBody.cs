namespace BraimChallenge.RequestBody
{
    public class AnimalsBody
    {
        public long[]? animaltypes { get; set; }
        public double weight { get; set; }
        public double length { get; set; }
        public double height { get; set; }
        public string? gender { get; set; }
        public int chipperid { get; set; }
        public long? chippinglocationid { get; set; }
    }

    public class UpdateAnimalBody
    {
        public double weight { get; set; }
        public double length { get; set; }
        public double height { get; set; }
        public string? gender { get; set; }
        public string? lifestatus { get; set; }
        public int chipperid { get; set; }
        public long? chippinglocationid { get; set; }
    }
}
