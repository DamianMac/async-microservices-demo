using Autofac;
using Autofac.Extensions.DependencyInjection;
using Search;
using Search.CatalogManager;
using Search.Data.Migration;
using Serilog;
using Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterAssemblyModules(typeof(Program).Assembly);
});
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.With<CorrelationIdEnricher>()
    .Enrich.WithProperty("Instance", context.Configuration["Instance"]));

// Add services to the container.

builder.Services.AddControllers();
// The shop calls the API cross-origin (shop.localtest.me -> api.localtest.me).
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Settings").GetValue<string>("ShopUrl")!)
    .AllowAnyHeader()
    .AllowAnyMethod()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHostedService<NimbusBusService>();

builder.Services.AddHttpClient<CatalogManagerClient>(http =>
    http.BaseAddress = new Uri(builder.Configuration.GetSection("Settings").GetValue<string>("CatalogManagerUrl")!.TrimEnd('/') + "/"));

var app = builder.Build();

DatabaseMigrator.Migrate(app.Configuration.GetSection("Settings").GetValue<string>("ConnectionString")!, Log.Logger);

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
