namespace pr9ASP.Services
{
    internal static class Paging
    {
        public const int MaxPageSize = 100;

        // Защита от page=0, отрицательных и слишком больших значений
        public static (int Page, int PageSize) Normalize(int page, int pageSize) =>
            (Math.Max(page, 1), Math.Clamp(pageSize, 1, MaxPageSize));
    }
}
