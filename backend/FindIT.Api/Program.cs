using FindIT.Api.Models;
using FindIT.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("MongoDb"));

builder.Services.AddSingleton<UsersService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
       options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // TODO: Change this to the actual origin of client
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.MapControllers();

app.Run();









//using MongoDB.Driver;
//using MongoDB.Bson;
//const string connectionUri = "mongodb+srv://db_admin:g3Ip5T3GVocOpTB5@finditdatabase.irmcyk0.mongodb.net/?appName=FindITDatabase";
//var settings = MongoClientSettings.FromConnectionString(connectionUri);
//// Set the ServerApi field of the settings object to set the version of the Stable API on the client
//settings.ServerApi = new ServerApi(ServerApiVersion.V1);
//// Create a new client and connect to the server
//var client = new MongoClient(settings);
//// Send a ping to confirm a successful connection
//try
//{
//    var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
//    Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
//}
//catch (Exception ex)
//{
//    Console.WriteLine(ex);
//}