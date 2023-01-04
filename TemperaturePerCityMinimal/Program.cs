using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(opt =>
{
    opt.SerializerOptions.IncludeFields = true;
});
builder.Services.AddDbContext<CityDb>(opt => opt.UseInMemoryDatabase("CityList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

app.MapGet("/", () => Results.Json(new City { Id=1, Name="Oslo", Country="Norway", Continent="Europe", CurrentTime=DateTime.Now, Temperature=-6.0 }, options));

app.MapGet("/cities", async (CityDb db) => await db.Cities.ToListAsync());

app.MapGet("/cities/bycontinent", async (CityDb db) => await db.Cities.GroupBy(c => c.Continent).ToListAsync());

app.MapGet("/cities/{id}", async (int id, CityDb db) => await db.Cities.FindAsync(id) is City city ? Results.Ok(city) : Results.NotFound());

app.MapPost("/cities", async (City city, CityDb db) =>
{
    db.Cities.Add(city);
    await db.SaveChangesAsync();
    return Results.Created($"/cities/{city.Id}", city);
});

app.MapPut("/cities/{id}", async (int id, City inputCity, CityDb db) =>
{
    var city = await db.Cities.FindAsync(id);
    if (city == null) return Results.NotFound();
    city.Name = inputCity.Name;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/cities/{id}", async (int id, CityDb db) =>
{
    if (await db.Cities.FindAsync(id) is City city)
    {
        db.Cities.Remove(city);
        await db.SaveChangesAsync();
        return Results.Ok(city);
    }
    return Results.NotFound();
});

app.Run();


public class City
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Continent { get; set; }
    public double Temperature { get; set; }
    public DateTime CurrentTime { get; set; }
}

class CityDb : DbContext
{
    public CityDb(DbContextOptions<CityDb> options)
    : base(options) { }

    public DbSet<City> Cities => Set<City>();
}