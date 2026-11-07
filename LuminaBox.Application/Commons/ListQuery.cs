namespace LuminaBox.Application.Commons
{
    public class ListQuery
    {
        private const int MaxPageSize = 50;
        private int _page = 1;
        private int _pageSize = 10;

        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = true;

        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
        }
    }
}
