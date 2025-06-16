using BillingPayment.Components;
using BillingPayment.Interfaces;
using BillingPayment.Providers;
using BillingPayment.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
    builder.Services.AddAuthorization();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<BillingPayment.Services.InvoiceService>();

    builder.Services.AddScoped<IMemberKeyProvider, MemberKeyProvider>();
    builder.Services.AddScoped<IRandomProvider, DefaultRandomProvider>();
    builder.Services.AddScoped<IInvoiceService, InvoiceService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    // Add these lines to enable authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Console.Error.WriteLine($": 'Failed to load configuration from file 'C:\\sourcecontrol\\BillingPayment\\BillingPayment\\appsettings.Development.json'.'");
    Console.Error.WriteLine($"Exception: {ex.Message}");
    throw;
}