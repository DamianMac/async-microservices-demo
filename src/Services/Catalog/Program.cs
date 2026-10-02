using Autofac;
using Autofac.Extensions.DependencyInjection;
using Catalog;
using Catalog.Caching;
using Catalog.CatalogManager;
using Catalog.Domain;
using Catalog.Images;
using Marten;
using Microsoft.Extensions.FileProviders;
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

builder.Services.AddMarten(options =>
    {
        options.Connection(builder.Configuration.GetSection("Settings").GetValue<string>("ConnectionString")!);
        options.Schema.For<Product>().Identity(p => p.Sku);
        options.Schema.For<StockStatus>().Identity(s => s.Sku);
    })
    .UseLightweightSessions()
    .ApplyAllDatabaseChangesOnStartup();

builder.Services.AddHttpClient<CatalogManagerClient>(http =>
    http.BaseAddress = new Uri(builder.Configuration.GetSection("Settings").GetValue<string>("CatalogManagerUrl")!.TrimEnd('/') + "/"));
builder.Services.AddSingleton<ProductImages>();
builder.Services.AddSingleton<ProductCache>();
// After AddMarten, so the schema exists before the cache loads.
builder.Services.AddHostedService<LoadProductCache>();


var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Serves images downloaded by StoreNewCatalog. The folder may not exist at startup,
// so it gets its own file provider rather than relying on the web root.
var images = app.Services.GetRequiredService<ProductImages>();
Directory.CreateDirectory(images.Folder);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(images.Folder),
    RequestPath = ProductImages.RequestPath,
});

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
