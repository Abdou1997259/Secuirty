using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Secuirty.Behaviors;
using Secuirty.Date;
using Secuirty.Dtos;
using Secuirty.Extentions;
using Secuirty.Helper;
using Secuirty.Models;
using Secuirty.Services;
using Serilog;
using System;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

#region logging Register 
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog(logger);
#endregion

#region System Registering  Dependecies
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
#endregion

#region Db Register
builder.Services.AddDbContext<Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion



#region Register Mediator and validations
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
// Add services to the container.

#endregion

#region Settings From app Settings
var jwt = new Jwt();
builder.Configuration.GetSection("Jwt").Bind(jwt);
builder.Services.AddSingleton(jwt);

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
#endregion

#region  Authentication Register


builder.Services.AddIdentity<User, IdentityRole>(
   options =>
   {
       options.Password.RequireNonAlphanumeric = false;
       options.Password.RequiredLength = 6;
       options.Password.RequireDigit = false;
       options.Password.RequireLowercase = false;
       options.Password.RequireUppercase = false;


   }
    ).AddEntityFrameworkStores<Context>().AddDefaultTokenProviders();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(1);
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,
        ValidateAudience = true,
        ValidAudience = jwt.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ClockSkew = TimeSpan.Zero
    };
});
#endregion

#region BackGroundTasks

builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
#endregion

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

#region Register Services

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IValidationService, ValidationService>();

#endregion


#region Swagger Services
builder.Services.AddSwaggerGen();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
#region Swagger MiddleWares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
#endregion

#region Custom MiddleWares
app.UseCustomMiddleWares();

#endregion

#region System MiddleWares
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/dashboard");

app.MapControllers();
#endregion

app.Run();
