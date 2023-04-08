using Azure.Core;
using Azure.Identity;
using FeatureFlagsDemo.Data;
using FeatureFlagsDemo.FeatureFlags;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

var appConfigUri = builder.Configuration.GetValue<string>("AppConfig:Uri");

if (!string.IsNullOrEmpty(appConfigUri))
{
    builder.Configuration.AddAzureAppConfiguration(o =>
    {
        TokenCredential credential = builder.Environment.IsDevelopment()
            ? new AzureCliCredential(new AzureCliCredentialOptions
            {
                TenantId = builder.Configuration.GetValue<string>("LocalDevelopmentTenantId")
            })
            : new ManagedIdentityCredential();

        o.Connect(new Uri(appConfigUri), credential)
            .UseFeatureFlags();
    });
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement()
    .AddFeatureFilter<AppVersionFeatureFilter>();

builder.Services.AddSingleton<InMemoryUserStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!string.IsNullOrEmpty(appConfigUri))
{
    app.UseAzureAppConfiguration();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
