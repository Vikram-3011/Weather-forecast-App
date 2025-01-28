using BlazorApp.Components;
using BlazorApp.Controllers;
using MudBlazor.Services;
using BlazorApp.Singletons;
using BlazorApp.Services;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

var supabaseUrl = "https://nxkuxdowqjedwznnymkv.supabase.co";
var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im54a3V4ZG93cWplZHd6bm55bWt2Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzQ5MjM2NjgsImV4cCI6MjA1MDQ5OTY2OH0.DqDceQsmgNxZpS33G2oD6sebgRowSGdP-tqXKu2COmM";

var options = new SupabaseOptions { AutoConnectRealtime = true };
var supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, options);
// MongoDB connection parameters
var mongoConnectionString = "mongodb://localhost:27017/";  // Change to your MongoDB connection string
var databaseName = "\r\nauthentication";  // Your database name
var collectionName = "user details";  // Your collection name

// Register the PersonalInfoService with MongoDB connection details
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
