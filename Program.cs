using BlazorApp.Components;
using BlazorApp.Controllers;
using MudBlazor.Services;
using BlazorApp.Singletons;
using BlazorApp.Services;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

var supabaseUrl = "{Supabase URl}"; // Supabase connection string
var supabaseKey = "{your Supabase APi key }"; // your Supabase APi key 

var options = new SupabaseOptions { AutoConnectRealtime = true };
var supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, options);


builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddSingleton(supabaseClient);
builder.Services.AddScoped<AuthService>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<DatabaseController>();
builder.Services.AddScoped<SupabaseController>();
builder.Services.AddScoped<UserStateManager>();
builder.Services.AddSingleton<WeatherDataService>();
builder.Services.AddHttpClient();
builder.Services.AddMudBlazorSnackbar();
builder.Services.AddSingleton<PersonalInfoService>();
builder.Services.AddSingleton<FavoriteCityService>();






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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
