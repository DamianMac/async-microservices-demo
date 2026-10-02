using Autofac;
using Autofac.Extensions.DependencyInjection;
using Marten;
using Serilog;
using Shared.Messaging;
using Warehouse;
using Warehouse.Domain;

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

builder.Services.AddMarten(options =>
    {
        options.Connection(builder.Configuration.GetSection("Settings").GetValue<string>("ConnectionString")!);
        options.Schema.For<StockItem>().Identity(i => i.Sku);
        options.Schema.For<Dispatch>().Identity(d => d.OrderId).UseOptimisticConcurrency(true);
    })
    .UseLightweightSessions()
    .ApplyAllDatabaseChangesOnStartup();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
