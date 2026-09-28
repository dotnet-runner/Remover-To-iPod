using System.Text;
using SpotifyApiWorker.Middlewares;
using SpotifyApiWorker.Services.Contracts;
using SpotifyApiWorker.Services.Implementations;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RouteOptions>(options => 
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddSingleton<IServerSessionKeyGenerator, ServerSessionKeyGenerator>();
builder.Services.AddSingleton<IConnectionMultiplexer>(options =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis:ConnectionString")!));

builder.Services.AddScoped<IAuthorization, Authorization>();
builder.Services.AddScoped<ICookieSetting, CookieSetting>();
builder.Services.AddScoped<IRedisService, RedisService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.MapControllers();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.Run();