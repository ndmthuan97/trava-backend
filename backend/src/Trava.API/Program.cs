
using Microsoft.AspNetCore.Mvc.Controllers;
using Trava.Application.Extensions;
using Trava.Infrastructure.Extensions;
using Trava.Infrastructure.Services.Identify;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApplicationPartManager(apm =>
    {
        apm.FeatureProviders.Add(new ControllerFeatureProvider());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.MigrationsDatabaseAsync();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
