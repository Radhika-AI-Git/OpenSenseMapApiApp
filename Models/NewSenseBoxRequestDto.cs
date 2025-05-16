namespace OpenSenseMapApiService.Models
{
    public class NewSenseBoxRequestDto
    {
        public string Name { get; set; }
        public string Exposure { get; set; } 
        public LocationDto Location { get; set; }
        public List<SensorDto> Sensors { get; set; }
        public string Model { get; set; } 
    }
    public class LocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Height { get; set; }
    }

    public class SensorDto
    {
        public string Title { get; set; }
        public string Unit { get; set; }
        public string SensorType { get; set; }
        public string Icon { get; set; }
    }
}
