namespace Aircraft.Models.ViewModels;

public class ProductDetailViewModel
{
    public Airplane Airplane { get; set; }
    public AirplaneColor? AirplaneColor { get; set; }
    public List<AirplaneColor> RelatedAirplaneColors { get; set; }
    public IEnumerable<Image>? AirplaneImages { get; set; }
    public IEnumerable<AirplaneSize>? AirplaneSizes { get; set; }
    public IEnumerable<AirplaneColor>? RelatedProduct { get; set; }
}