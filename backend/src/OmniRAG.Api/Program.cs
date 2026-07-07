using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using OmniRAG.Api.Models;
using OmniRAG.Application.Abstractions;
using OmniRAG.Application.Services;
using OmniRAG.Infrastructure.Persistence;
using OmniRAG.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment.EnvironmentName;
   
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200")    
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Register SQL Server Context
builder.Services.AddDbContext<RagSqlDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlDb"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }
        ));

// Register CosmosDB Container as a singleton service
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("CosmosDb");
    return new CosmosClient(connectionString);
});

// Register Container by pulling configuration dynamically inside the factory
builder.Services.AddSingleton<Container>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var client = serviceProvider.GetRequiredService<CosmosClient>();

    // Pull settings safely right here
    var databaseName = config["CosmosDbSettings:DatabaseName"];
    var containerName = config["CosmosDbSettings:ContainerName"];

    if (string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(containerName))
    {
        throw new InvalidOperationException(
            "CosmosDB initialization failed. Ensure 'CosmosDbSettings:DatabaseName' and 'CosmosDbSettings:ContainerName' are populated in appsettings.json.");
    }

    return client.GetContainer(databaseName, containerName);
});

// Add Microsoft Entra ID JWT bearer token authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddSingleton<IUploadRepository, InMemoryUploadRepository>();
//builder.Services.AddSingleton<IUploadFileStorage>(_ => new LocalUploadFileStorage(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "uploads")));
//builder.Services.AddScoped<IUploadService, UploadService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Create Azure SQL schema on first run if it doesn't exist, useful for development and testing purposes,
// but in production, should use migrations instead.

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<RagDbContext>();
//    db.Database.EnsureCreated();
//}

app.Run();
