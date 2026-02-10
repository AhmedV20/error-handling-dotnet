using System.ComponentModel.DataAnnotations;
using FullApiSample.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FullApiSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly Dictionary<string, UserDto> Users = new()
    {
        ["user-1"] = new UserDto("user-1", "john@example.com", "John Doe"),
        ["user-2"] = new UserDto("user-2", "jane@example.com", "Jane Smith")
    };

    /// <summary>
    /// Get all users.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetAll()
    {
        return Ok(Users.Values);
    }

    /// <summary>
    /// Get user by ID - throws UserNotFoundException if not found.
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<UserDto> GetById(string id)
    {
        if (!Users.TryGetValue(id, out var user))
        {
            throw new UserNotFoundException(id);
        }

        return Ok(user);
    }

    /// <summary>
    /// Create a new user - demonstrates validation errors.
    /// </summary>
    [HttpPost]
    public ActionResult<UserDto> Create([FromBody] CreateUserRequest request)
    {
        // Check for duplicate email
        if (Users.Values.Any(u => u.Email == request.Email))
        {
            throw new DuplicateEmailException(request.Email);
        }

        var id = $"user-{Users.Count + 1}";
        var user = new UserDto(id, request.Email, request.Name);
        Users[id] = user;

        return CreatedAtAction(nameof(GetById), new { id }, user);
    }

    /// <summary>
    /// Transfer funds - demonstrates InsufficientFundsException.
    /// </summary>
    [HttpPost("{id}/transfer")]
    public ActionResult Transfer(string id, [FromBody] TransferRequest request)
    {
        if (!Users.ContainsKey(id))
        {
            throw new UserNotFoundException(id);
        }

        // Simulate insufficient funds
        var available = 100.00m;
        if (request.Amount > available)
        {
            throw new InsufficientFundsException(request.Amount, available);
        }

        return Ok(new { message = "Transfer successful", amount = request.Amount });
    }

    /// <summary>
    /// Update order state - demonstrates InvalidOrderStateException.
    /// </summary>
    [HttpPost("orders/{orderId}/state")]
    public ActionResult UpdateOrderState(string orderId, [FromBody] UpdateOrderStateRequest request)
    {
        // Simulate invalid state transition
        var currentState = "PENDING";
        if (request.NewState == "DELIVERED" && currentState == "PENDING")
        {
            throw new InvalidOrderStateException(currentState, request.NewState);
        }

        return Ok(new { orderId, state = request.NewState });
    }
}

public record UserDto(string Id, string Email, string Name);

public class CreateUserRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
}

public class TransferRequest
{
    [Required]
    [Range(0.01, 1000000, ErrorMessage = "Amount must be between 0.01 and 1,000,000")]
    public decimal Amount { get; set; }
}

public class UpdateOrderStateRequest
{
    [Required]
    public string NewState { get; set; } = string.Empty;
}
