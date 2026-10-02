using Autofac;
using Autofac.Extensions.DependencyInjection;
using CatalogManager;
using CatalogManager.Data.Migration;
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
builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<NimbusBusService>();

var app = builder.Build();

DatabaseMigrator.Migrate(app.Configuration.GetSection("Settings").GetValue<string>("ConnectionString")!, Log.Logger);

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Serves product images copied into wwwroot at runtime by the import, which MapStaticAssets doesn't know about.
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
