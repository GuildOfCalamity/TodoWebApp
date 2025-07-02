namespace TodoWebApp.Models
{
    public class StatisticsViewModel
    {
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public string? FastestCompletion { get; set; }
        public string? SlowestCompletion { get; set; }
    }
}
