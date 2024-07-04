using ServiceCache;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServiceCache(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/GetOrCreateAsync/{key}", async (string key, ICacheService cache) =>
{
    return await cache.GetOrCreateAsync(key, () => Task.FromResult($"{nameof(cache.GetOrCreateAsync)} - Hello World"));
})
.WithName("GetOrCreateAsync")
.WithOpenApi();


app.MapGet("/GetOrDefault/{key}", async (string key, ICacheService cache) =>
{
    return await cache.GetOrDefaultAsync(key, $"{nameof(cache.GetOrDefault)} - Hello World");
})
.WithName("GetOrDefault")
.WithOpenApi();


app.MapGet("/CreateAndSet/{key}", async (string key, ICacheService cache) =>
{
    await cache.CreateAndSet(key, $"{nameof(cache.CreateAndSet)} - Hello World");
})
.WithName("CreateAndSet")
.WithOpenApi();


app.MapDelete("/RemoveAsync", (string key, ICacheService cache) =>
{
    cache.RemoveAsync(key);
})
.WithName("RemoveAsync")
.WithOpenApi();



app.Run();

