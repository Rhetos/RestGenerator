/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Rhetos;
using System;

namespace TestApp
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
                c.CustomSchemaIds(type => type.ToString()); // Allows multiple entities with the same name in different modules.
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TestApp", Version = "v1" });
                // Adding Rhetos REST API to Swagger with document name "rhetos".
                c.SwaggerDoc("rhetos", new OpenApiInfo { Title = "Rhetos REST API", Version = "v1" });
            });

            // Using NewtonsoftJson for backward-compatibility with older versions of Rhetos.RestGenerator:
            // legacy Microsoft DateTime serialization and
            // byte[] serialization as JSON array of integers instead of Base64 string.
            services.AddControllers()
                .AddNewtonsoftJson(o =>
                {
                    o.UseMemberCasing();
                    o.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                    o.SerializerSettings.Converters.Add(new Rhetos.Host.AspNet.RestApi.Utilities.ByteArrayConverter());
                });

            // Adding Rhetos to AspNetCore application.
            services.AddRhetosHost(ConfigureRhetosHostBuilder)
                .AddAspNetCoreIdentityUser()
                .AddHostLogging()
                .AddRestApi(o =>
                {
                    o.BaseRoute = "rest";
                    o.ConceptInfoRestMetadataProviders.Add(new RhetosExtendedControllerMetadataProvider());
                    o.GroupNameMapper = (conceptInfo, controller, oldName) => "rhetos"; // OpenAPI document name.
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    // Add Swagger endpoint for Rhetos REST API.
                    c.SwaggerEndpoint("/swagger/rhetos/swagger.json", "Rhetos REST API");
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestApp v1");
                });
            }

            app.UseRhetosRestApi();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureRhetosHostBuilder(IServiceProvider serviceProvider, IRhetosHostBuilder rhetosHostBuilder)
        {
            rhetosHostBuilder
                .ConfigureRhetosAppDefaults()
                .UseBuilderLogProviderFromHost(serviceProvider)
                .ConfigureConfiguration(cfg => cfg
                    .AddJsonFile("ConnectionString.local.json")
                    .MapNetCoreConfiguration(Configuration));
        }
    }
}
