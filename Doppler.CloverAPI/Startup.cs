using System;
using Doppler.CloverAPI.Infrastructure;
using Doppler.CloverAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Doppler.CloverAPI;

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
        services.Configure<DopplerDatabaseSettings>(Configuration.GetSection(nameof(DopplerDatabaseSettings)));
        // Explicitly using Hellang because services.AddProblemDetails() generates an ambiguity
        // between Microsoft.AspNetCore.Http.Extensions.ProblemDetailsServiceCollectionExtensions
        // and Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions
        // TODO: consider replace Hellang by out of the box alternative (but it is not working fine right now)
        Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions.AddProblemDetails(services);
        services.AddHttpContextAccessor();
        services.AddDopplerSecurity();
        services.AddControllers();
        services.AddCors();
        services.AddScoped<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
        services.AddScoped<ICloverService, CloverService>();
        services.AddScoped<IClientAddressService, ClientAddressService>();

        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter the token into field as 'Bearer {token}'",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme },
                            },
                            Array.Empty<string>()
                        }
                });

            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Doppler.CloverAPI", Version = "v1" });

            var baseUrl = Configuration.GetValue<string>("BaseURL");
            if (!string.IsNullOrEmpty(baseUrl))
            {
                c.AddServer(new OpenApiServer() { Url = baseUrl });
            };
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        Hellang.Middleware.ProblemDetails.ProblemDetailsExtensions.UseProblemDetails(app);

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Doppler.CloverAPI v1"));

        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors(policy => policy
            .SetIsOriginAllowed(isOriginAllowed: _ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
