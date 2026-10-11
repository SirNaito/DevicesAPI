using System.Text.Json.Serialization;
using Devices.Api.Data;
using Devices.Api.Models;
using Devices.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter<DeviceState>(
                namingPolicy: null,
                allowIntegerValues: false));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure the database connection string and ensure the database directory exists
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

var sqliteConnection =
    new SqliteConnectionStringBuilder(connectionString);

// If the DataSource is not an absolute path, combine it with the ContentRootPath to create an absolute path
if (!Path.IsPathRooted(sqliteConnection.DataSource))
{
    sqliteConnection.DataSource = Path.Combine(
        builder.Environment.ContentRootPath,
        sqliteConnection.DataSource);
}

// Ensure the directory for the database file exists
var databaseDirectory =
    Path.GetDirectoryName(sqliteConnection.DataSource);

// Create the directory if it doesn't exist
if (!string.IsNullOrWhiteSpace(databaseDirectory))
{
    Directory.CreateDirectory(databaseDirectory);
}

builder.Services.AddDbContext<DevicesDbContext>(options =>
    options.UseSqlite(sqliteConnection.ToString()));

builder.Services.AddScoped<IDeviceService, DeviceService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure the database is created and apply any pending migrations
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
