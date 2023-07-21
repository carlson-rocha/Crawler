using ExtratoClubeAPI.Crawlers;
using Microsoft.AspNetCore.Builder;

namespace ExtratoClubeAPI
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<ICrawlerService, CrawlerService>();
        }
    }
}
