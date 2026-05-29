using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DuelApp.Shared.Infrastructure.Auth;

public static class Extensions
{
    private const string SectionName = "Keycloak";
    
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var keycloakOptions = services.GetOptions<KeycloakOptions>(SectionName);
                
                options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
                options.MetadataAddress = keycloakOptions.MetadataAddress;
                options.Audience = keycloakOptions.Audience;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = keycloakOptions.Issuer,
                    NameClaimType = "sub",
                };
                    
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        
        services.AddScoped<AppUserMiddleware>();
        
        return services;
    }
}