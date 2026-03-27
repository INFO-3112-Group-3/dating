using FindIT.Api.Models;
using FindIT.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Bind DatabaseSettings to the configuration section
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("MongoDb"));

// 2. Register MongoDB Client and Database as Singletons
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});

// 3. Register Services
builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<SkillTagsService>();
builder.Services.AddSingleton<MatchingService>();

// Register HttpClient and the Geocoding Service
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IGeocodingService, NominatimGeocodingService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();