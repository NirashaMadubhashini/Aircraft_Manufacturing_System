namespace Aircraft.Models.ViewModels
{
    public class AirplaneDetailViewModel
    {
        public IEnumerable<Airplane> Airplanes { get; set; }
        public IEnumerable<AirplaneColor> AirplaneColors { get; set; }
    }
}
