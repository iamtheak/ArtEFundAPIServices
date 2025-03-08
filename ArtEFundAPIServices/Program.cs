using System.Text;
using System.Text.Json;
using ArtEFundAPIServices.Data.DatabaseContext;
using ArtEFundAPIServices.DataAccess.ContentType;
using ArtEFundAPIServices.DataAccess.Creator;
using ArtEFundAPIServices.DataAccess.Donation;
using ArtEFundAPIServices.DataAccess.Membership;
using ArtEFundAPIServices.DataAccess.RefreshToken;
using ArtEFundAPIServices.DataAccess.User;
using ArtEFundAPIServices.Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddLogging();
builder.Services.AddScoped<IUserInterface, UserRepo>();
builder.Services.AddScoped<IRefreshTokenInterface, RefreshTokenRepo>();
builder.Services.AddScoped<IContentTypeInterface, ContentTypeRepo>();
builder.Services.AddScoped<ICreatorInterface, CreatorRepo>();
builder.Services.AddScoped<IMembershipInterface, MembershipRepository>();
builder.Services.AddScoped<IDonationInterface, DonationRepository>();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers(); // Add MVC services

builder.Services.AddRouting(config => { config.LowercaseUrls = true; });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? Constants.DEFAULT_JWT_SECRET))
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse(); // Prevents the default response

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";


                var result = string.Empty;

                if (string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
                {
                    result = JsonSerializer.Serialize(new
                    {
                        error = "no_token",
                        errorDescription = "No token provided"
                    });
                }
                else
                {
                    result = JsonSerializer.Serialize(new
                    {
                        error = "invalid_token",
                        errorDescription = "The token is invalid or has expired"
                    });
                }

                return context.Response.WriteAsync(result);
            }
        };
    })
    .AddGoogle(context =>
    {
        context.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        context.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });


builder.Services.AddSwaggerGen(x =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    x.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(myAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://localhost:3000").AllowAnyHeader().AllowAnyMethod()
                .AllowCredentials();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseCors(myAllowSpecificOrigins);

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization(); // Optional, if using authorization
app.MapControllers(); // Map controllers for routing


app.Run();