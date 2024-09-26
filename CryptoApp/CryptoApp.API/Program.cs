using System.Runtime.InteropServices;
using System.Text;
using CryptoApp.API.Common;
using CryptoApp.Data;
using CryptoApp.Data.Models;
using CryptoApp.Data.Repository;
using CryptoApp.Services.Implementations;
using CryptoApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
            c.SwaggerDoc("v1", new() { Title = "CryptoApp API", Version = "v1" });

            // Define the OAuth2.0 scheme that's in use (i.e., Implicit Flow)
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description =
                    "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        string GetOsDbPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Server=.\\SQLEXPRESS;Database=CryptoAppDB;Trusted_Connection=True;MultipleActiveResultSets=true;";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return @"Server=localhost,1433;Database=CryptoAppDB;User Id=sa;Password=reallyStrongPwd123;TrustServerCertificate=true";

            throw new Exception("Unsupported OS");
        }
        
        builder.Services.AddDbContext<CryptoAppDbContext>(options =>
        {
            var osPath = GetOsDbPath();
            options.UseSqlServer(osPath);
        }, ServiceLifetime.Scoped);
        
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
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<ICoinloreService, CoinloreService>();

        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CryptoApp.API v1"));
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
