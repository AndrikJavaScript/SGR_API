using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SGR_API.Data;
using System;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Configurar el puerto dinámicamente
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Configurar CORS para Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
    );
});

// ---- Código para transformar la URL de conexión MySQL ----
// Obtén la variable de entorno "MYSQL_URL"
var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
if (string.IsNullOrWhiteSpace(mysqlUrl))
{
    throw new Exception("La variable MYSQL_URL no está configurada");
}

// Parsear la URL de conexión
Uri uri = new Uri(mysqlUrl); 
string[] userInfo = uri.UserInfo.Split(':'); // Extrae usuario y contraseña

string server = uri.Host;
int portNumber = uri.Port;
string database = uri.AbsolutePath.TrimStart('/'); // Quita la barra inicial
string username = userInfo[0];
string password = userInfo[1];

// Construir la cadena de conexión en formato ADO.NET
string connectionString = $"Server={server};Port={portNumber};Database={database};User={username};Password={password};";

// Configurar el DbContext usando la cadena de conexión procesada
builder.Services.AddDbContext<SGRContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)))
);
// ---- Fin del código para MySQL ----

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

// Configuración de serialización JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

var app = builder.Build();

// Manejo de errores
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
app.MapControllers();

app.Run();
