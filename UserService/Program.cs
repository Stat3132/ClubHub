using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Models;

// TODO: add eureka
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;

var builder = WebApplication.CreateBuilder(args);

// Swagger & API Docs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//HTTP client for cross service communication
builder.Services.AddHttpClient();

// EF Core
builder.Services.AddDbContext<ClubHubDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("conn_orderservice_sqlserver")));

// Controllers + JSON serialization
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

//JWT Token
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"])
            ),
            RoleClaimType = ClaimTypes.Role // ✅ THIS LINE IS ESSENTIAL
        };
    });


// TODO: Eureka Discovery (if used)
builder.Services.AddDiscoveryClient(builder.Configuration);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// Required if Eureka is enabled. call is now obsolete, unnecessary
//app.UseDiscoveryClient(); 

app.Run();
