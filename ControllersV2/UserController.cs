using Asp.Versioning;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;

namespace ApiVersioningDemo.ControllersV2;

[ApiController]
[ApiVersion(2)]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IValidator<UserRequestV2> _validator;

    public UserController(ILogger<UserController> logger, IValidator<UserRequestV2> validator)
    {
        _logger = logger;
        _validator = validator;
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseV2>> Post(UserRequestV2 user)
    {
        ValidationResult? result = await _validator.ValidateAsync(user);
        if (!result.IsValid)
        {
            string[] failingFields = result.Errors.Select(s => s.PropertyName).ToArray();
            
            _logger.LogError("Incoming payload did not pass the validation on fields: {FailingFields}", failingFields);

            return BadRequest(new UserResponseV2($"Incoming payload did not pass the validation on fields: {string.Join(", ", failingFields)}", false));
        }

        var helloMessage = $"Hello {user.FirstName} {user.LastName}. Your roles are: {string.Join(", ", user.UserRoles)}";
        
        return Ok(new UserResponseV2(helloMessage, true));
    }
}

public record UserRequestV2(string FirstName, string LastName, string[] UserRoles);
public record UserResponseV2(string Message, bool Success);

public class UserRequestV2Validator : AbstractValidator<UserRequestV2>
{
    public UserRequestV2Validator()
    {
        RuleFor(e => e.FirstName).NotEmpty();
        RuleFor(e => e.LastName).NotEmpty();
        RuleFor(e => e.UserRoles).NotEmpty();
    }
}