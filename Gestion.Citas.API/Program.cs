using Gestion.Citas.API.Endpoints;
using Gestion.Citas.API.Middleware;
using Gestion.Citas.Business;
using Gestion.Citas.DataAccess;
using Gestion.Citas.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDbContext<AppointmentsDbContext>(p =>
{
    p.UseSqlServer(builder.Configuration.GetConnectionString("DbAppointments"));
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
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "API Para Gestión de Citas Médicas",
        Version = "v1",
        Description = "Esta es una API para una clínica que necesita gestionar sus citas de manera eficiente, evitando cruces de horarios, datos guardados manualmente, etc",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingrese el token JWT."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddAuthorization();

builder.Services
    .AddBusiness()
    .AddRepositories();


var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Gestion de Citas API v1"
    );

    options.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapGroup("api/auth").MapAuthEndpoints().WithTags("Authtentication");
app.MapGroup("api/users").MapUserEndpoints().WithTags("Users");
app.MapGroup("api/specialties").MapSpecialtyEndpoints().WithTags("Specialties");
app.MapGroup("api/doctors").MapDoctorEndpoints().WithTags("Doctors");
app.MapGroup("api/patients").MapPatientEndpoints().WithTags("Patients");
app.MapGroup("api/businessHours").MapBusinessHoursEndpoints().WithTags("BusinessHours");
app.MapGroup("api/appointments").MapAppointmentEndpoints().WithTags("Appointments");
app.MapGroup("api/appointmentBlocks").MapAppointmentBlockEndpoints().WithTags("AppointmentBlocks");

app.Run();
