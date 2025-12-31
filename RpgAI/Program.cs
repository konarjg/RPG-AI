using Domain.Entities;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RpgAI;

using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(LoggingConfiguration.ConfigureLogging);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
       .AddNewtonsoftJson(options => {
         options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
       });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGenNewtonsoftSupport();


WebApplication app = builder.Build();

try {
  Console.WriteLine("Applying database migrations...");
  
  using (IServiceScope scope = app.Services.CreateScope()) {
    DatabaseContext dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    await dbContext.Database.MigrateAsync();

    Guid id = Guid.AllBitsSet;
    
    User test = new() {
      Id = id
    };

    if (!(await dbContext.Users.AnyAsync(u => u.Id == id))) {
      dbContext.Users.Add(test);
      await dbContext.SaveChangesAsync();
    }
  }
  
  Console.WriteLine("Database migrations applied successfully.");
}
catch (Exception ex) {
  Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
  Environment.Exit(1);
}


app.UseSwagger();
app.UseSwaggerUI(); 

app.UseSerilogRequestLogging(); 

app.UseRouting(); 
app.MapControllers();

//app.UseHttpsRedirection();

app.Run();


