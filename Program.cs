using ApiVersioningDemo.ControllersV1;
using ApiVersioningDemo.Options;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

using FluentValidation;

namespace ApiVersioningDemo;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add API versioning support
        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new QueryStringApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

        builder.Services.AddValidatorsFromAssemblyContaining<UserRequestV1Validator>();
        builder.Services.ConfigureOptions<VersionedSwaggerGenOptions>();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            IApiVersionDescriptionProvider? provider = app.Services.GetService<IApiVersionDescriptionProvider>();
            
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant()); 
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers().WithOpenApi();

        app.Run();
    }
}