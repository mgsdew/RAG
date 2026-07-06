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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IUploadRepository, InMemoryUploadRepository>();
builder.Services.AddSingleton<IUploadFileStorage>(_ => new LocalUploadFileStorage(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "uploads")));
builder.Services.AddScoped<IUploadService, UploadService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.MapControllers();

app.Run();
