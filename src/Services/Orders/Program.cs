using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Orders;
using Orders.Auth;
using Orders.CatalogManager;
using Orders.Data.Migration;
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

// Validates the JWT that Profile issues, using the same settings.
var jwt = builder.Configuration.GetSection("Settings:Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep the JWT claim names ("sub", "email") rather than mapping them to the long WS-* URIs.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = jwt.SigningKey(),
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpClient<CatalogManagerClient>(http =>
    http.BaseAddress = new Uri(builder.Configuration.GetSection("Settings").GetValue<string>("CatalogManagerUrl")!.TrimEnd('/') + "/"));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHostedService<NimbusBusService>();

var app = builder.Build();

DatabaseMigrator.Migrate(app.Configuration.GetSection("Settings").GetValue<string>("ConnectionString")!, Log.Logger);

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
