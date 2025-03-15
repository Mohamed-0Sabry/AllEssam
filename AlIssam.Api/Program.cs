using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using AlIssam.API.Config;
using AlIssam.API.Services;
using AlIssam.API.Services.interFaces;
using AlIssam.API.Services.InterFaces;
using AlIssam.DataAccessLayer;
using AlIssam.DataAccessLayer.Entities;
using System.Text;
using System.Text.Json.Serialization;
using AlIssam.Api.Config;
using AlIssam.Api.Services.interFaces;
using AlIssam.Api.Services;
using AlIssam.Api.MiddleWare;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First()); 
});

builder.Services.Configure<EmailServiceConfigruation>(builder.Configuration.GetSection("EmailServiceConfigruation"));
builder.Services.Configure<GoogleAuthConfiguration>(builder.Configuration.GetSection("AuthConfigruation:Google"));
//builder.Services.Configure<FacebookAuthConfiguration>(builder.Configuration.GetSection("AuthConfiguration:Facebook"));

builder.Services.Configure<FatoorahConfig>(builder.Configuration.GetSection("Fatoorah"));

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig.Secret)),
    ValidIssuer = jwtConfig.ValidIssuer,
    // change in production
    ValidateIssuer = false,
    ValidAudience = jwtConfig.ValidAudience,
    // change in production
    ValidateAudience = false,
    RequireExpirationTime = true,
    //ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(jwtConfig.ClockSkewMinutes),
};

builder.Services.AddSingleton<TokenValidationParameters>(tokenValidationParameters);
builder.Services.AddSingleton<JwtConfig>(jwtConfig);


builder.Services.AddProjectServices(builder.Configuration);

builder.Services.AddIdentityCore<User>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<AlIssamDbContext>()
    .AddDefaultTokenProviders();


builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(2);
});


builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IJwtHandlerService, JwtHandlerService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IFatoorahService, FatoorahService>();



//builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<FatoorahConfig>>().Value);

builder.Services.AddHttpClient("FatoorahClient", client =>
{
    client.BaseAddress = new Uri("https://api.myfatoorah.com/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.AddRouting(opt =>
{
    opt.LowercaseUrls = true;
});

// https://alessam.store
var allowedOrigins = new[] { "http://localhost:3000", "http://localhost:5173", "https://alessam.store", "https://api.alessam.store", "http://alessam.store", "http://api.alessam.store"};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
        builder.WithOrigins(allowedOrigins) 
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials() 
               .SetIsOriginAllowed(origin => true) 
    );
});


builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = null;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseCors("AllowFrontend");
app.UseRouting();
//app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.MapControllers();




app.Run();
