namespace AlIssam.API.Dtos.Response
{
    public class DashboardStatisticsResponse
    {
        public int Total_Users { get; set; }
        public int Completed_Orders { get; set; }
        public int Cancelled_Orders { get; set; }
        public  decimal Total_Payment { get; set; }
        public decimal Profit { get; set; }

    }
}
