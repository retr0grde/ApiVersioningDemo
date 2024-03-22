# API Versioning demo
## Introduction
This repository demonstrates the approach to API versioning in ASP.NET Web APIs using `Asp.Versioning` NuGet package. Additionally, it extends Swagger / OpenAPI's built-in support to generate docs for versioned endpoint.

## API versioning setup
### Required NuGet packages
To enable API versioning in WebAPI project we need to install two NuGet packages:
 - `Asp.Versioning.Http`
 - `Asp.Versioning.Mvc.ApiExplorer`

### Global setup
After installing these packages, we need to set up versioning in `Program.cs` file by registering versioning service with DI.

Following example will register and configure versioning service with default V1 version and option to select API version through `api-version` query parameter (e.g. `?api-version=1`).

```csharp
builder.Services.AddApiVersioning(options =>
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
```

Other options of selecting API versions include selection by:
 - including version number in URL using route parameter and `UrlSegmentApiVersionReader`,
 - additional header and `HeaderApiVersionReader`,
 - media type with `MediaTypeApiVersionReader`.

### Versioned controllers
In this repository each API version is separated on the controller-level. Versioning this way can be achieved by using `[ApiVersion(x)]` annotation placed above controller class.

```csharp
...
[ApiVersion(1)]
public class UserController : ControllerBase
{
    ...
}
```

### Versioned actions
Another option of versioning can be done on action-level. This means all versions of the action can be kept in single controller. This approach uses mix of two annotations:
 - `[ApiVersion(x)]` - multiple annotations put above controller to indicate which API versions are supported by this controller,
 - `[MapToApiVersion(x)]` - put above each action handling different version.

```csharp
...
[ApiVersion(1)]
[ApiVersion(2)]
public class UserController : ControllerBase
{
    ...
    
    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult<UserResponseV1>> PostV1(UserRequestV1 user)
    {
        ...
    }
    
    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult<UserResponseV2>> PostV2(UserRequestV2 user)
    {
        ...
    }
}
```

## Swagger / OpenAPI versioned documentation
ASP.NET Core applications are shipped with `Swashbuckle.AspNetCore` package that is responsible for generating OpenAPI specification and Swagger UI for generating documentation for  API endpoints.

The default Swagger configuration generates documents only for the default API version by implementing custom option generator for `SwaggerGen` and populating version list in Swagger UI.  

> **Note:** In march 2024 ASP.NET Core team announced on their GitHub that they will be retiring `Swashbuckle.AspNetCore` dependency from coming .NET 9 since it was no longer actively maintained.
>
> Announcement link: https://github.com/dotnet/aspnetcore/issues/54599

### Custom option generator
To force Swagger to generate documentation for versions other than the default one, custom options generator is required.

Following generator class collects every API version detected by `Asp.Versioning.Mvc.ApiExplorer` dependency and creates OpenAPI docs for every one of them. 

```csharp
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
```

This generator then has to be registered with DI in `Program.cs` file.

```csharp
...
builder.Services.ConfigureOptions<VersionedSwaggerGenOptions>();
...
```

The setup above will cause Swagger to generate OpenAPI JSON specification documents for every version.
These documents can be consumed then by Swagger UI.

## Swagger UI setup
After making OpenAPI JSON specification becomes available it can be consumed by Swagger UI using similiar logic used to generate OpenAPI documents.
These documents can be added by iterating available API versions and building URLs to JSON documents in `UseSwaggerUI()` calls.  

```csharp
app.UseSwaggerUI(options =>
{
    foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant()); 
});
```