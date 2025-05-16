namespace OpenSenseMapApiService.Models
{
    public class SenseBoxResponseDto
    {
        public string _id { get; set; }
        public string Name { get; set; }
        public string Exposure { get; set; }
        public string Model { get; set; }
        public LocationResponseDto Location { get; set; }
        public List<SensorDto> Sensors { get; set; }
    }
    public class LocationResponseDto
    {
        public double[] Coordinates { get; set; }
    }
}
