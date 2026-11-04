namespace LuminaBox.Application.Dtos.Geography
{
    public class ProvinceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int? GhnProvinceId { get; set; }
    }

    public class DistrictResponse
    {
        public Guid Id { get; set; }
        public Guid ProvinceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int? GhnDistrictId { get; set; }
    }

    public class WardResponse
    {
        public Guid Id { get; set; }
        public Guid DistrictId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? GhnWardCode { get; set; }
    }

    public class ResolvedLocation
    {
        public Guid ProvinceId { get; set; }
        public Guid DistrictId { get; set; }
        public Guid WardId { get; set; }
    }

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
