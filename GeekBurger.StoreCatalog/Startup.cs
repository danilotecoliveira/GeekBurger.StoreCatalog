﻿using System;
using System.IO;
using GeekBurger.StoreCatalog.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.Configuration;
using GeekBurger.StoreCatalog.ServiceBus;
using GeekBurger.StoreCatalog.Infra.Services;
using GeekBurger.StoreCatalog.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using GeekBurger.StoreCatalog.Infra.Interfaces;
using GeekBurger.StoreCatalog.Infra.Repositories;

namespace GeekBurger.StoreCatalog
{
    /// <summary>
    /// Startup project
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Startup constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Startup global configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Startup configure services
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<StoreCatalogDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("StoreCatalog")));
            services.AddTransient<IDbContext, StoreCatalogDbContext>();
            services.AddTransient<IRequestApi, RequestApi>();
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<IProductCore, ProductCore>();
            services.AddTransient<IProductionAreasCore, ProductionAreasCore>();
            services.AddTransient<IStoreCatalogCore, StoreCatalogCore>();

            var mvcCoreBuilder = services.AddMvcCore();
            mvcCoreBuilder
                .AddFormatterMappings()
                .AddJsonFormatters()
                .AddCors();

            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "GeekBurger Store Catalog",
                    Description = "Project for FIAP",
                    TermsOfService = "None"
                });

                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "GeekBurger.StoreCatalog.xml");
                c.IncludeXmlComments(xmlPath);

                ReceiveMessage receiveMessage = new ReceiveMessage();
                SendMessage sendMessage = new SendMessage(true); 
            });         
        }

        /// <summary>
        /// Startup configure
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "GeekBurger Store Catalog v1");
            });

            app.UseMvc();
        }
    }
}
