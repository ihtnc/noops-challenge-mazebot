using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MazebotCrawler
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
        {
            return services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "Mazebot Crawler API",
                    Version = "v1",
                    Description = "Mazebot API Solver",
                    TermsOfService = "None",
                    Contact = new Swashbuckle.AspNetCore.Swagger.Contact
                    {
                        Name = "ihopethisnamecounts",
                        Email = string.Empty,
                        Url = "https://github.com/ihtnc/noops-challenge-mazebot"
                    }
                });
            });
        }

        public static IApplicationBuilder UseApiDocumentation(this IApplicationBuilder app)
        {
            return app.UseSwagger()
               .UseSwaggerUI(c => { c.SwaggerEndpoint($"/swagger/v1/swagger.json", $"MazebotCrawler API V1"); });
        }
    }
} 