using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Microblogging.API;
using Microblogging.Repository;
using Microblogging.Service.Images;
using Microblogging.Service.Posts;
using Microblogging.Service.Services.Posts;
using Microblogging.Service.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



// =======================
// Service Configuration
// =======================
builder.Services.AddScoped<IPostService, PostService>();

builder.Services.AddSingleton<AzureBlobStorageStrategy>();
builder.Services.AddSingleton<LocalFileStorageStrategy>();

builder.Services.AddSingleton<ImageStorageStrategyFactory>();
builder.Services.AddScoped<IImageStorageStrategy>(sp =>
    sp.GetRequiredService<ImageStorageStrategyFactory>().GetStrategy());
builder.Services.AddSingleton<IImageProcessorService, ImageProcessorService>();
builder.Services.AddScoped<UsersDatabaseSeeder>();



// =======================
// JWT Configuration
// =======================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

// =======================
// Mongo Identity
// =======================
builder.Services.AddIdentityMongoDbProvider<MongoUser, MongoRole>(identityOptions =>
{
    identityOptions.Password.RequireDigit = false;
    identityOptions.Password.RequireLowercase = false;
    identityOptions.Password.RequireUppercase = false;
    identityOptions.Password.RequiredLength = 6;
    identityOptions.Password.RequireNonAlphanumeric = false;
},
mongoIdentityOptions =>
{
    mongoIdentityOptions.ConnectionString = builder.Configuration.GetConnectionString("MongoDb");
    mongoIdentityOptions.UsersCollection = "Users";
    mongoIdentityOptions.RolesCollection = "Roles";
});

// =======================
// JWT Authentication
// =======================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// =======================
// Swagger
// =======================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Microblogging API",
        Version = "v1",
        Description = "API documentation for the Microblogging app"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// =======================
// Custom Services & Mongo Repos
// =======================
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

// =======================
// MVC + API Controllers
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// =======================
// Pipeline Configuration
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Microblogging API V1");
        c.RoutePrefix = string.Empty;
    });
}




app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<UsersDatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
