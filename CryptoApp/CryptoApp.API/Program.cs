using CryptoApp.API.Common;
using CryptoApp.Data;
using CryptoApp.Data.Models;
using CryptoApp.Services.Implementations;
using CryptoApp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;
        
        const string CorsPolicyName = "CorsPolicy";
        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "CryptoApp.API", Version = "v1" });
            c.OperationFilter<FileUploadOperation>();
        });
        
        builder.Services.AddDbContext<CryptoAppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultDatabaseConnection")));
        
        builder.Services.AddIdentity<AspNetUser, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<CryptoAppDbContext>()
            .AddDefaultTokenProviders();

        
        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });
        
        builder.Services.AddCors(options =>
        {
            string[] allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();

            options.AddPolicy(CorsPolicyName, cpb =>
            {
                cpb.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.WithOrigins("http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        
        builder.Services.AddScoped<IPortfolioService, PortfolioService>();

        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CryptoApp.AIPI v1"));
        }
        
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        
        app.UseHttpsRedirection();
        
        app.UseRouting();
        
        app.UseCors(CorsPolicyName);
        
        app.UseCors(policy =>
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        
        app.UseHttpsRedirection();
        
        app.UseAuthentication();
        
        app.UseAuthorization();
        
        app.MapControllers();
        
        app.Run();
    }
}
