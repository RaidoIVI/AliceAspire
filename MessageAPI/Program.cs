using AliceAspire.ServiceDefaults;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using MessageAPI.Services.Implementations;
using MessageAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

IConfigurationSection userData = builder.Configuration.GetSection("UserSession");

UserSessionData userSession = new UserSessionData
{
    UserName = userData["UserName"],
    Password = userData["Password"]
};

var _instaApi = InstaApiBuilder.CreateBuilder()
    .SetUser(userSession)
    .UseLogger(new DebugLogger(InstagramApiSharp.Logger.LogLevel.Info))
    .Build();

builder.Services.AddSingleton<IInstaApi>(_instaApi);
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


