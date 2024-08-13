using AliceAspire.ServiceDefaults;
using MessageAPI.Managers.Implementations;
using MessageAPI.Managers.Interfaces;
using MessageAPI.Services.Implementations;
using MessageAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<ISessionService>(new SessionService());
builder.Services.AddScoped<IInstagramManager, InstagramManager>();
builder.Services.AddScoped<IInstagramApiService, InstagramApiService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAuthorization();

app.MapControllers();

app.Run();


