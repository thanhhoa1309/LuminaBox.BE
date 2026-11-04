namespace LuminaBox.Application.Dtos.Geography
{
    public class GeoSyncReport
    {
        public int ProvincesAdded { get; set; }
        public int DistrictsAdded { get; set; }
        public int WardsAdded { get; set; }
        public int GhnProvincesMatched { get; set; }
        public int GhnDistrictsMatched { get; set; }
        public int GhnWardsMatched { get; set; }
        public int GhnDistrictsUnmatched { get; set; }
        public int GhnWardsUnmatched { get; set; }
        public bool GhnMatchingSkipped { get; set; }
    }
}
