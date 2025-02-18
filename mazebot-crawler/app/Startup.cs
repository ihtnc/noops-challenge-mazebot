﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MazebotCrawler.Crawlies;
using MazebotCrawler.Middlewares;
using MazebotCrawler.Repositories;
using MazebotCrawler.Services;

namespace MazebotCrawler
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddOptions()
                .Configure<NoOpsChallengeOptions>(Configuration)

                .AddHttpClient()

                .AddSingleton<IMazebotSolverStatusRepository, InMemoryStatusRepository>()

                .AddTransient<IApiRequestProvider, ApiRequestProvider>()
                .AddTransient<IApiClient, ApiClient>()
                .AddTransient<INoOpsApiClient, NoOpsApiClient>()
                .AddTransient<IMazebotSolver, MazebotSolver>()
                .AddTransient<IMazeImager, MazeImager>()

                .AddTransient<IMazeCrawlerQueen, MazeCrawlerQueen>()
                .AddTransient<IMazeCrawlerSpawner, MazeCrawlerSpawner>()

                .AddApiDocumentation()

                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseApiDocumentation();
            app.UseMiddleware<CorrelationIdHeaderMiddleware>();

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
