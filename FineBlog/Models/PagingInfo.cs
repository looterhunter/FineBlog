namespace FineBlog.Models
{
    public class PagingInfo
    {
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public int MaxDisplayedPages { get; set; } = 3; //顯示分頁數量最多3個
        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);

        public int[] GetDisplayedPages()
        {
            var pages = new List<int>();

            var startPage = Math.Max(1, CurrentPage - MaxDisplayedPages / 2);
            var endPage = Math.Min(TotalPages, startPage + MaxDisplayedPages - 1);

            for (var i = startPage; i <= endPage; i++)
            {
                pages.Add(i);
            }

            return pages.ToArray();
        }
    }
}
