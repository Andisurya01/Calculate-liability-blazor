using IMSFinance.Components;
using IMSFinance.Data;
using IMSFinance.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddScoped<IKreditService, KreditDbService>();
builder.Services.AddScoped<IKreditJsonService, KreditJsonService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
