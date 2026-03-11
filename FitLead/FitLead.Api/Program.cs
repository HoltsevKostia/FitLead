using FitLead.Api.Common.Errors;
using FitLead.Api.Identity;
using FitLead.Application.Identity;
using FitLead.Application.Trainings.TrainingPrograms.Commands;
using FitLead.Infrastructure;
using FitLead.Infrastructure.Identity;
using FitLead.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
    throw new InvalidOperationException("Jwt SigningKey is not configured.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.Events = new JwtBearerEvents
        {
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
            IssuerSigningKey = signingKey,
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
