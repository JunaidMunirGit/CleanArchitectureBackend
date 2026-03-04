using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.OpenApi.Models;

namespace Web.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(static o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            // Temporarily commented out
            //var securityScheme = new OpenApiSecurityScheme
            //{
            //    Name = "JWT Authentication",
            //    Description = "Enter your JWT token in this field",
            //    In = ParameterLocation.Header,
            //    Type = SecuritySchemeType.Http,
            //    Scheme = JwtBearerDefaults.AuthenticationScheme,
            //    BearerFormat = "JWT"
            //};

            //o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

            //var securityRequirement = new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference
            //            {
            //                Type = ReferenceType.SecurityScheme,
            //                Id = JwtBearerDefaults.AuthenticationScheme
            //            }
            //        },
            //        []
            //    }
            //};

            //o.AddSecurityRequirement(securityRequirement);
        });

        return services;



        // Source - https://stackoverflow.com/a/79835686
        // Posted by Nermin, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-03-04, License - CC BY-SA 4.0

        //services.AddSwaggerGen(options =>
        //{

        //    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
        //    {
        //        Type = SecuritySchemeType.Http,
        //        Scheme = "bearer",
        //        BearerFormat = "JWT",
        //        Description = "JWT Authorization header using the Bearer scheme."
        //    });

        //    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        //    {
        //        [new OpenApiSecuritySchemeReference("bearer", document)] = []
        //    });
        //});
        //return services;

    }
}
