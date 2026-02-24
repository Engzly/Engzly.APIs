namespace Engzly.Domain.Entities.Identity
{
    public sealed class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        private Location() { }

        public Location(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("Invalid Latitude");

            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("Invalid Longitude");

            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
