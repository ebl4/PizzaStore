using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PizzaStore.Models;

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=Pizzas.db";

// Provedor de banco de dados
builder.Services.AddSqlite<PizzaDb>(connectionString);
//builder.Services.AddDbContext<PizzaDb>(opt => opt.UseInMemoryDatabase("items"));

// Incluindo o Serviço Swagger Open API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "PizzaStore API",
        Description = "Making the Pizzas you love",
        Version = "v1"
    });
});

builder.Services.AddCors(options => {
    options.AddPolicy(name: MyAllowSpecificOrigins,
      builder => {
          builder.WithOrigins("*");
      });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});

app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/", () => "Hello World!");
app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());
app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));
app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) => {
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});
app.MapPut("/pizza{id}", async (PizzaDb db, Pizza pizza, int id) => {
    var pizzaToUpdate = await db.Pizzas.FindAsync(id);
    if (pizzaToUpdate is null) return Results.NotFound();
    pizzaToUpdate.Name = pizza.Name;
    pizzaToUpdate.Description = pizza.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) => {
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();
