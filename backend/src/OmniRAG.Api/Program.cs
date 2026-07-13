using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OmniRAG.Api.Models;
using System.IdentityModel.Tokens.Jwt;

// CRITICAL: Stop .NET from remapping claims like 'roles' or 'sub' into legacy XML schemas
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment.EnvironmentName;

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Force the API to load ALL configurations from the cloud immediately
var vaultName = builder.Configuration["Azure:KeyVaultName"];
var kvUri = new Uri($"https://{vaultName}.vault.azure.net/");
// Configure credential options to prevent it from probing cloud IMDS endpoints locally
var credentialOptions = new DefaultAzureCredentialOptions
{
    // Disables the cloud VM metadata service check that is causing the 5-try timeout crash
    ExcludeManagedIdentityCredential = builder.Environment.IsDevelopment(),

    // Ensures it looks for your local developer context instead
    ExcludeAzureCliCredential = false,
    ExcludeVisualStudioCredential = false
};

builder.Configuration.AddAzureKeyVault(kvUri, new DefaultAzureCredential(credentialOptions));

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();

// STABLE AUTHENTICATION: Direct, native JWT configuration that avoids middleware nesting crashes
var authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});

authenticationBuilder.AddJwtBearer(options =>
{
    var tenantId = builder.Configuration["AzureAD:TenantId"];
    var audience = builder.Configuration["AzureAD:Audience"];

    options.MapInboundClaims = false;
    options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
    options.Audience = audience;
    options.IncludeErrorDetails = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        RoleClaimType = "roles",
        NameClaimType = "oid",
        ValidIssuers = new[]
        {
            $"https://login.microsoftonline.com/{tenantId}/v2.0",
            $"https://sts.windows.net/{tenantId}/"
        }
    };
});

// Register SQL Server Context
builder.Services.AddDbContext<RagSqlDbContext>(options =>
{
    // Pull the connection string matching your Key Vault secret name 'ConnectionStrings--SqlDb'
    var sqlConnectionString = builder.Configuration.GetConnectionString("SqlDb");

    if (string.IsNullOrEmpty(sqlConnectionString))
    {
        throw new InvalidOperationException("SQL Connection String could not be resolved from Key Vault configuration. Check secret naming conventions.");
    }

    options.UseSqlServer(sqlConnectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });
});

// Register CosmosDB Client as a singleton service
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    // Pull the connection string matching your Key Vault secret name 'ConnectionStrings--CosmosDb'
    var cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosDb");

    if (string.IsNullOrEmpty(cosmosConnectionString))
    {
        throw new InvalidOperationException("Cosmos Connection String could not be resolved from Key Vault configuration.");
    }

    return new CosmosClient(cosmosConnectionString);
});

// Register Container by pulling configuration dynamically inside the factory
builder.Services.AddSingleton<Container>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<CosmosClient>();
    return client.GetContainer(builder.Configuration["CosmosDbSettings:DatabaseName"], builder.Configuration["CosmosDbSettings:ContainerName"]);
});


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your token. Example: 'Bearer {token}'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
//builder.Services.AddSingleton<IUploadRepository, InMemoryUploadRepository>();
//builder.Services.AddSingleton<IUploadFileStorage>(_ => new LocalUploadFileStorage(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "uploads")));
//builder.Services.AddScoped<IUploadService, UploadService>();

// Cross‑Origin Resource Sharing (CORS)
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
