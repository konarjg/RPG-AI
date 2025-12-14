using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RpgAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

try {
  Console.WriteLine("Applying database migrations...");
  
  using (var scope = app.Services.CreateScope()) {
    DatabaseContext dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    await dbContext.Database.MigrateAsync();
  }
  
  Console.WriteLine("Database migrations applied successfully.");
}
catch (Exception ex) {
  Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
  Environment.Exit(1);
}


app.UseSwagger();
app.UseSwaggerUI(); 

app.UseRouting(); 
app.MapControllers();

//app.UseHttpsRedirection();

app.Run();
