using Configuration;
using Configuration.Extensions;
using Configuration.Options;
using DbContext.Extensions;
using DbRepos;
using Encryption.Extensions;
using Services;
using Services.Interfaces;
using System.Text.Json.Serialization;



var builder = WebApplication.CreateBuilder(args);

// Configure System.Text.Json for circular references
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

builder.Configuration.AddSecrets(builder.Environment);

builder.Services.AddEncryptions(builder.Configuration);
builder.Services.AddJwtToken(builder.Configuration);
builder.Services.AddDatabaseConnections(builder.Configuration);
builder.Services.AddEnvironmentInfo();
builder.Services.AddUserBasedDbContext();
// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpClient("ApiUrl", client =>
{
    client.BaseAddress = new Uri("http://localhost:5115/");
});



builder.Services.AddScoped<AdminDbRepos>();
builder.Services.AddScoped<FriendsDbRepos>();
builder.Services.AddScoped<AddressesDbRepos>();
builder.Services.AddScoped<PetsDbRepos>();
builder.Services.AddScoped<QuotesDbRepos>();

builder.Services.AddScoped<IAdminService, AdminServiceDb>();
builder.Services.AddScoped<IFriendsService, FriendsServiceDb>();
builder.Services.AddScoped<IAddressesService, AddressesServiceDb>();
builder.Services.AddScoped<IPetsService, PetsServiceDb>();
builder.Services.AddScoped<IQuotesService, QuotesServiceDb>();

builder.Services.AddScoped<LoginDbRepos>();
builder.Services.AddScoped<ILoginService, LoginService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy
            .WithOrigins("https://localhost:5115")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
