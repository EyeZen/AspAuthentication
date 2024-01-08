using AspAuthentication.Data;
using AspAuthentication.Data.Models;
using AspAuthentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// For Minimal APIs, `AddApiExplorer` requires MVC Core
// For API projects, `AddControllers` calls `AddApiExplorer`
builder.Services.AddEndpointsApiExplorer();

// generate swagger ui and endpoints from minimal-api and controllers
builder.Services.AddSwaggerGen(option =>
{
    // Ask swagger to provide filed for adding token to the swagger-ui
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Test API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    // Tell swagger when and how to attach token to a request, viz. `Bearer <token>` Scheme
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

// wraps unhandled expceptions
builder.Services.AddProblemDetails();
// Adds support and route handling for different versions of the api
// ref: https://www.infoworld.com/article/3562355/how-to-use-api-versioning-in-aspnet-core.html
builder.Services.AddApiVersioning();
// Adds services requried for routing requests
// ref: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.routingservicecollectionextensions.addrouting?view=aspnetcore-8.0
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Add DB Contexts

// Move the connection string to user secrets for a real app
builder.Services.AddDbContext<ApplicationDbContext>(opt => 
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<TokenService, TokenService>();

// Support string to enum conversions
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Specify Identity requirements
// Must be added before Authentication, else 404 is thrown
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // IdentityOptions ref: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.identityoptions?view=aspnetcore-8.0

        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    //.AddRoles<IdentityRole>() // TODO: Is this statement needed after spacifying IdentityRole in the above function's type-parameter
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ALternatively, Identity Options can also be configured separately as -

//builder.Services.Configure<IdentityOptions>(options =>
//{
//    // Password settings
//    options.Password.RequireDigit = true;
//    options.Password.RequiredLength = 8;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequireUppercase = true;
//    options.Password.RequireLowercase = false;
//    options.Password.RequiredUniqueChars = 6;

//    // Lockout settings
//    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
//    options.Lockout.MaxFailedAccessAttempts = 10;
//    options.Lockout.AllowedForNewUsers = true;

//    // User settings
//    options.User.RequireUniqueEmail = true;
//});

var validIssuer = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidIssuer");
var validAudience = builder.Configuration.GetValue<string>("JwtTokneSettings:ValidAudience");
var symmetricSecurityKey = builder.Configuration.GetValue<string>("JwtTokenSettings:SymmetricSecurityKey");

builder.Services
    // Adds Authorization by default
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        // Challenge defines how to handle and unauthenticated request, e.g., redirecting to login, sending 401-unauthorized, etc
        // ref: https://stackoverflow.com/questions/60510015/what-exactly-does-challenge-mean-in-asp-net-core-3
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        // TODO: Whats the difference between `DefaultScheme` and `DefaultAuthenticateScheme`
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        //options.SaveToken = true;
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = validIssuer,
            ValidAudience = validAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(symmetricSecurityKey!)
            ),
            //RequireSignedTokens = true,
            //RequireExpirationTime = true,
        };
    });

// Alternatively, add custom policies in authorization
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("CustomPolicy1Name", policyBuilder =>
//    {
//        // list of claims required by the policy
//        // * Shouldn't use string constants directly in code
//        policyBuilder.RequireAuthenticatedUser()    // require authentication
//            .RequireClaim(...);                     // additionally, require some claim
//        // requires only that the type exists on user, irrespective of value
//        policyBuilder.RequireClaim("Claim-Type1-Name");
//        // requries that claim-type is present and has one of the specified values, values can be specified in an IEnumerable list as well
//        policyBuilder.RequireClaim("Claim-Type2-Name", "Claim-Value1", "Claim-Value2", ...);
//    });
//});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirects `http` request to `https` requests
// TODO: verify
app.UseHttpsRedirection();
// TODO: what does it actually do?
app.UseStatusCodePages();

app.UseAuthentication();
// (msdn) Role based authorization: https://learn.microsoft.com/en-us/aspnet/web-forms/overview/older-versions-security/roles/role-based-authorization-cs
app.UseAuthorization();

// maps routes for data-annotation-attribute routed controllers
// ref: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-8.0#ar6
app.MapControllers();

app.Run();
