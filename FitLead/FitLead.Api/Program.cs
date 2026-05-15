using FitLead.Api.Errors;
using FitLead.Api.Auth;
using FitLead.Api.Hubs;
using FitLead.Api.Identity;
using FitLead.Application.Identity;
using FitLead.Application.Trainings.TrainingPrograms.Commands;
using FitLead.Infrastructure;
using FitLead.Infrastructure.Identity;
using FitLead.Infrastructure.Persistence;
using FitLead.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = CsrfTokenNames.RequestHeader;
    options.Cookie.Name = CsrfTokenNames.AntiforgeryCookie;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Path = "/";
    options.Cookie.SecurePolicy = builder.Environment.IsProduction()
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateTrainingProgramCommand).Assembly);
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientApp", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddIdentityCore<AppIdentityUser>()
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<FitLeadDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
    {
        var jwtOptions = jwtOptionsAccessor.Value;
        JwtSigningKeyResolver.Validate(jwtOptions);

        var validationSigningKey = JwtSigningKeyResolver.CreateValidationKey(jwtOptions);
        var validAlgorithms = JwtSigningKeyResolver.GetValidAlgorithms(jwtOptions);

        options.MapInboundClaims = false;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrWhiteSpace(context.Token) &&
                    context.Request.Cookies.TryGetValue(AuthCookieNames.AccessToken, out var accessToken))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;
                if (principal is null)
                {
                    context.Fail("Token principal is missing.");
                    return;
                }

                if (principal.HasClaim(x => x.Type == CustomClaimTypes.DomainUserId))
                    return;

                var identityUserId =
                    principal.FindFirstValue("sub");

                if (string.IsNullOrWhiteSpace(identityUserId))
                {
                    context.Fail("Identity user id claim is missing.");
                    return;
                }

                var resolver = context.HttpContext.RequestServices
                    .GetRequiredService<IIdentityDomainUserLinkResolver>();

                var domainUserId = await resolver.ResolveDomainUserIdAsync(
                    identityUserId,
                    context.HttpContext.RequestAborted);

                if (!domainUserId.HasValue)
                {
                    context.Fail("User context enrichment failed.");
                    return;
                }

                if (principal.Identity is not ClaimsIdentity claimsIdentity)
                {
                    context.Fail("Claims identity is missing.");
                    return;
                }

                claimsIdentity.AddClaim(
                    new Claim(CustomClaimTypes.DomainUserId, domainUserId.Value.ToString()));
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = validationSigningKey,
            ValidAlgorithms = validAlgorithms,
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TrainerOnly", policy => policy.RequireRole("Trainer"));
    options.AddPolicy("ClientOnly", policy => policy.RequireRole("Client"));
});
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await DevIdentitySeeder.SeedAsync(app.Services);
    await DemoMessengerSeeder.SeedAsync(app.Services);
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FitLeadDbContext>();
        await PlatformExerciseSeeder.SeedAsync(dbContext);
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("ClientApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();

public partial class Program;
