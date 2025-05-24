using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SGR_API.Data;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Configurar el puerto dinámicamente (por ejemplo, usando la variable de entorno "PORT")
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Definir política de CORS para permitir Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
    );
});

// Configurar la conexión a la base de datos
builder.Services.AddDbContext<SGRContext>(options =>
    options.UseMySql(Environment.GetEnvironmentVariable("MYSQL_URL"),
        new MySqlServerVersion(new Version(8, 0, 36))) // Ajusta la versión según la de Railway
);



// Configuración de autenticación y JWT
var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// Aquí es donde agregamos la configuración para el serializador JSON:
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

var app = builder.Build();

// Manejo de errores en producción
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Aplicar política de CORS
app.UseCors("AllowAngularApp");

// Middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.UseRouting();
app.MapGet("/test-db", async (SGRContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect ? "Conexión exitosa a MySQL" : "Error en la conexión a la base de datos";
});

// Mapear controladores de API
app.MapControllers();

app.Run();
