using System;
using System.Linq;
using System.Text.Json.Serialization;
using Elasticsearch.Net;
using Election.Domain.Elasticsearch.Repository;
using Election.Helper;
using Election.Middleware;
using Election.Service;
using Election.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Nest;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Election
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options => 
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Election.Api", Version = "v1", Description = "Election API"});
                c.OperationFilter<SwaggerFileOperationFilter>();
            });
            #region Elastic
            var elasticsearchSettings = Configuration.GetSection("ElasticsearchSettings").Get<ElasticsearchSettings>();
            var uriList = elasticsearchSettings.Url.Split(';').Select(x => new Uri(x));
            var pool = new StaticConnectionPool(uriList);
            var elasticConnectionSettings = new ConnectionSettings(pool);
            var elasticClient = new ElasticClient(elasticConnectionSettings);

            services.AddSingleton(elasticClient);
            services.AddSingleton(new ImageHelper());

            services.AddSingleton<IApplicationService, ApplicationService>();
            services.AddSingleton<IElectionRepository>(_ => new ElectionRepository(elasticClient, "election-index"));

            services.AddSingleton(elasticClient);
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Election.Api API"); });
            });
            
            app.UseMiddleware<ErrorHandling>();
            // Enable Swagger middleware and endpoint
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.

            app.UseSwaggerUI(c =>
            {
                c.InjectJavascript("https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/4.1.3/swagger-ui-bundle.js", "text/javascript");
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FitApp API");
            });
            app.UseStaticFiles();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseCors();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });            
        }
        
        private class SwaggerFileOperationFilter : IOperationFilter  
        {  
            public void Apply(OpenApiOperation operation, OperationFilterContext context)  
            {  
                var fileUploadMime = "multipart/form-data";  
                if (operation.RequestBody == null || !operation.RequestBody.Content.Any(x => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)))  
                    return;  
  
                var fileParams = context.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(IFormFile));  
                operation.RequestBody.Content[fileUploadMime].Schema.Properties =  
                    fileParams.ToDictionary(k => k.Name, _ => new OpenApiSchema()  
                    {  
                        Type = "string",  
                        Format = "binary"  
                    });
            }  
        }
    }
}