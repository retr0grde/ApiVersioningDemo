using Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiVersioningDemo.Options;

public class VersionedSwaggerGenOptions(IApiVersionDescriptionProvider provider) : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(string? name, SwaggerGenOptions options) => Configure(options);
        
    public void Configure(SwaggerGenOptions options)
    {
        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
    }
        
    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription apiVersionDescription)
    {
        return new OpenApiInfo
        {
            Title = "API " + apiVersionDescription.GroupName,
            Version = apiVersionDescription.ApiVersion.ToString(),
            Description = apiVersionDescription.IsDeprecated ? "This version has been marked as deprecated." : ""
        };
    }
}
