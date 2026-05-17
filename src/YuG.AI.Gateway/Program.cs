using YuG.AI.Gateway.Extensions;
using YuG.AI.Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAiOptions(builder.Configuration);
builder.Services.AddAiCoreServices();
builder.Services.AddSingleton<AiExceptionHandlingMiddleware>();

var app = builder.Build();

app.UseMiddleware<AiExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

await app.RunAsync();
