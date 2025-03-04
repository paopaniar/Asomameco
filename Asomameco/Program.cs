using System.Text;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Asomameco.Infraestructure.Data;
using Asomameco.Infraestructure.Repository.Interfaces;
using Asomameco.Infraestructure.Repository.Implementations;
using Asomameco.Application.Services.Interfaces;
using Asomameco.Application.Services.Implementations;
using Asomameco.Application.Profiles;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using Serilog.Events;
using Asomameco.Web.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de sesi�n
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiraci�n de la sesi�n
    options.Cookie.HttpOnly = true; // Cookies accesibles solo por HTTP
    options.Cookie.IsEssential = true; // Necesario para GDPR si aplica
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo en conexiones seguras
});


builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Configuraci�n de la base de datos
builder.Services.AddDbContext<AsomamecoContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDataBase"));
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// Configuraci�n de dependencias (Repository y Services)
// Repositories


builder.Services.AddTransient<IRepositoryAsamblea, RepositoryAsamblea>();
builder.Services.AddTransient<IRepositoryEstado1Usuario, RepositoryEstado1Usuario>();
builder.Services.AddTransient<IRepositoryEstado2Usuario, RepositoryEstado2Usuario>();
builder.Services.AddTransient<IRepositoryEstadoAsamblea, RepositoryEstadoAsamblea>();
builder.Services.AddTransient<IRepositoryMetodoConfirmacion, RepositoryMetodoConfirmacion>();
builder.Services.AddTransient<IRepositoryUsuario, RepositoryUsuario>();
builder.Services.AddTransient<IRepositoryTipoUsuario, RepositoryTipoUsuario>();

// Services 

//builder.Services.AddTransient<ServiceEmail>();


builder.Services.AddTransient<IServiceAsamblea, ServiceAsamblea>();
builder.Services.AddTransient<IServiceEstado2Usuario, ServiceEstado2Usuario>();
builder.Services.AddTransient<IServiceEstadoAsamblea, ServiceEstadoAsamblea>();
builder.Services.AddTransient<IServiceMetodoConfirmacion, ServiceMetodoConfirmacion>();
builder.Services.AddTransient<IServiceUsuario, ServiceUsuario>();
builder.Services.AddTransient<IServiceTipoUsuario, ServiceTipoUsuario>();

// Configuraci�n de AutoMapper
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<AsambleaProfile>();
    config.AddProfile<AsistenciaProfile>();
    config.AddProfile<ConfirmacionProfile>();
    config.AddProfile<Estado1UsuarioProfile>();
    config.AddProfile<Estado2UsuarioProfile>();
    config.AddProfile<EstadoAsambleaProfile>();
    config.AddProfile<MetodoConfirmacionProfile>();
    config.AddProfile<TipoUsuarioProfile>();
    config.AddProfile<UsuarioProfile>();

});


// Configuraci�n de Serilog
var logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .Enrich.FromLogContext()
    .WriteTo.Console(LogEventLevel.Information)
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
        .WriteTo.File(@"Logs\Info-.log", shared: true, encoding: Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
        .WriteTo.File(@"Logs\Debug-.log", shared: true, encoding: Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
        .WriteTo.File(@"Logs\Warning-.log", shared: true, encoding: Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
        .WriteTo.File(@"Logs\Error-.log", shared: true, encoding: Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
        .WriteTo.File(@"Logs\Fatal-.log", shared: true, encoding: Encoding.ASCII, rollingInterval: RollingInterval.Day))
    .CreateLogger();
builder.Host.UseSerilog(logger);

// Add services to the container.
builder.Services.AddControllersWithViews()
       .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";  // P�gina de inicio de sesi�n
        options.AccessDeniedPath = "/Home/AccessDenied"; // P�gina de acceso denegado
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Expiraci�n de sesi�n
    });


var app = builder.Build();


// Habilitar sesiones
app.UseSession();


app.Use(async (context, next) =>
{
    var culture = context.Request.Cookies["Culture"] ?? "es-ES"; // Idioma predeterminado
    var cultureInfo = new System.Globalization.CultureInfo(culture);
    System.Globalization.CultureInfo.CurrentCulture = cultureInfo;
    System.Globalization.CultureInfo.CurrentUICulture = cultureInfo;

    await next.Invoke();
});




// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseMiddleware<ErrorHandlingMiddleware>();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
