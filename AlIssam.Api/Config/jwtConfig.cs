namespace AlIssam.API.Config
{
    public class JwtConfig
    {
        public string Secret { get; set; }
        public string ExpireTime { get; set; }
        public string Auidiance { get; set; } 
        public string ValidAudience { get; set; }
        public  string ValidIssuer { get; set; }
        public string Issuer { get; set; } 
        public int ClockSkewMinutes { get; set; }
    }
}
