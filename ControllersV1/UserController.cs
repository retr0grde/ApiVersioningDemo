using Asp.Versioning;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;

namespace ApiVersioningDemo.ControllersV1;

[ApiController]
[ApiVersion(1)]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IValidator<UserRequestV1> _validator;
    
    public UserController(ILogger<UserController> logger, IValidator<UserRequestV1> validator)
    {
        _logger = logger;
        _validator = validator;
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseV1>> Post(UserRequestV1 user)
    {
        ValidationResult? result = await _validator.ValidateAsync(user);
        if (!result.IsValid)
        {
            string[] failingFields = result.Errors.Select(s => s.PropertyName).ToArray();
            
            _logger.LogError("Incoming payload did not pass the validation on fields: {FailingFields}", failingFields);

            return BadRequest(new UserResponseV1($"Incoming payload did not pass the validation on fields: {string.Join(", ", failingFields)}"));
        }

        var helloMessage = $"Hello {user.FirstName} {user.LastName}. Your roles are: {string.Join(", ", user.userRole)}";
        
        return Ok(new UserResponseV1(helloMessage));
    }
}

public record UserRequestV1(string FirstName, string LastName, string[] userRole);
public record UserResponseV1(string Message);

public class UserRequestV1Validator : AbstractValidator<UserRequestV1>
{
    public UserRequestV1Validator()
    {
        RuleFor(e => e.FirstName).NotEmpty();
        RuleFor(e => e.LastName).NotEmpty();
        RuleFor(e => e.userRole).NotEmpty();
    }
}