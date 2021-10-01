using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Net.Http;
using DAL;
using DAL.Context;
using DAL.Service;
using DAL.SqlServer;
using Domain;
using MathCore.EF7.Interfaces.Repositories;
using MathCore.EF7.Repositories.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ApiServer
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiServer", Version = "v1" });
            });


            //ToDo When Use it, don't forget add-migration and update-database
            services.AddTransient<TestContextInitializer>();
            var db_type = Configuration["Database"];
            switch (db_type)
            {
                default: throw new NotSupportedException($"Тип подключения БД {db_type} не поддерживается");

                case "SqlServer":
                    services.AddTestDbContextSqlServer(Configuration.GetConnectionString(db_type));
                    break;

                case "Sqlite":
                    services.AddTestDbContextSqlite(Configuration.GetConnectionString(db_type));
                    break;

                case "InMemory":
                    services.AddDbContext<TestContext>(opt => opt.UseInMemoryDatabase("TestMathCore"));
                    break;
            }
            services.AddTestRepositories();
            //services.AddDbContext<TestContext>(o =>
            //{
            //    o.UseLazyLoadingProxies();
            //    o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            //});
            services.AddCors(
                o =>
                {
                    o.AddPolicy("CorsPolicy", policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    });
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<HttpClient>();

            services.AddMvc().AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.Formatting = Formatting.Indented;
                })
               .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.WriteIndented = true;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TestContextInitializer context)
        {
            app.UseCors("CorsPolicy"); 
            context.InitializeAsync().Wait();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiServer v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
