using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using TaskList.Data;
using TaskList.Middleware;
using TaskList.CustomSettings;


namespace TaskList
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

            //Enable to allow manual handling of model binding errors
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Entity Framework: Register the context with dependency injection
            services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Setup swagger 
            SetupSwaggerDocuments(services);

            // Enable multi-stream read
            services.AddTransient<EnableMultipleStreamReadMiddleware>();

            // DEMO: Setup custom settings
            //SetupCustomSettings(services);
        }

        /// <summary>
        /// Sets up the swagger documents
        /// </summary>
        /// <param name="services">The service collection</param>
        private static void SetupSwaggerDocuments(IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Task List API",
                    Version = "v1",
                    Description = "Task List API",
                });

                // Use method name as operationId so that ADD REST Client... will work
                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            SetupSwaggerJsonGenerationAndUI(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMultipleStreamReadMiddleware();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Sets up the Swagger JSON file and Swagger Interactive UI
        /// </summary>
        /// <param name="app">The application builder</param>
        private static void SetupSwaggerJsonGenerationAndUI(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                // Use the older 2.0 format so the ADD REST Client... will work
                c.SerializeAsV2 = true;
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            //       specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task List API");

                // Serve the Swagger UI at the app's root (http://localhost:<port>)
                c.RoutePrefix = string.Empty;
            });
        }

        /// <summary>
        /// Sets up custom, strongly typed settings
        /// </summary>
        /// <param name="services">The service colleciton</param>
        private void SetupCustomSettings(IServiceCollection services)
        {
            // Add support for injection of IOptions<T>
            services.AddOptions();

            // Add the class that represnets the settings for the CustomerLimits section 
            // in the JSON settings
            services.Configure<TaskLimit>(Configuration.GetSection(nameof(TaskLimit)));

            // Support Generic IConfiguration access for generic string access
            services.AddSingleton<IConfiguration>(Configuration);
        }
    }
}
