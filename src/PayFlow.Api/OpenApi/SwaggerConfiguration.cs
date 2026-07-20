using Microsoft.OpenApi;

namespace PayFlow.Api.OpenApi;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddPayFlowSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PayFlow API",
                Version = "v1",
                Description = "Digital wallet & P2P payments API (portfolio / demo)."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT access token. Example: Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });

            options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
        });

        return services;
    }

    public static WebApplication UsePayFlowSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "PayFlow API v1");
            options.DocumentTitle = "PayFlow API";
            options.DisplayRequestDuration();
        });

        return app;
    }
}
