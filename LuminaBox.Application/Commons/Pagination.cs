namespace LuminaBox.Application.Commons
{
    public class Pagination<T>
    {
        public List<T> Items { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public Pagination() { }

        public Pagination(List<T> items, int count, int pageIndex, int pageSize)
        {
            Items = items;
            TotalCount = count;
            CurrentPage = pageIndex;
            PageSize = pageSize;
        }
    }
}
