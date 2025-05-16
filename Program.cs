using OpenSenseMapApiService.Models;
using OpenSenseMapApiService.Services;
using OpenSenseMapProxyApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<OpenSenseMapOptions>(
    builder.Configuration.GetSection("OpenSenseMap"));
builder.Services.AddHttpClient<IOpenSenseMapService, OpenSenseMapService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
