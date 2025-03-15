using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace AlIssam.DataAccessLayer
{
    public static class Injection
    {
        public static IServiceCollection AddProjectServices(
                      this IServiceCollection services,
                      IConfiguration config)
        {
            services.AddDbContext<AlIssamDbContext>
                (
                    options => options.UseSqlServer(config.GetConnectionString("TestDb"))
                );

            return services;
        }
    }
}
