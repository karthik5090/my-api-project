using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ForgotPasswordApp.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ForgotPasswordApp.Controllers
{
    [Route("api/forgotpassword")]
    [ApiController]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly ApplicationDbContext _pontext;
        private readonly UserManager<ApplicationUser> _userManagero;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ForgotPasswordController> _logger;  // Add ILogger dependency

        // Inject ILogger into the constructor
        public ForgotPasswordController(ApplicationDbContext pontext, UserManager<ApplicationUser> userManagero, IEmailSender emailSender, ILogger<ForgotPasswordController> logger)
        {
            _pontext = pontext;
            _userManagero = userManagero;
            _emailSender = emailSender;
            _logger = logger;  // Assign to _logger
        }

        // Send Password Reset Link
  [HttpPost("send-password-reset-link")]
public async Task<IActionResult> SendPasswordResetLink([FromBody] ForgotPasswordRequest model)
{
    var user = await _userManagero.FindByEmailAsync(model.Email);
    if (user == null)
    {
        _logger.LogWarning("No user found with this email: {Email}", model.Email);  // Log warning
        return BadRequest("No user found with this email.");
    }
    var utcDate = DateTime.UtcNow; // Example UTC date
var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
var indiaTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, indiaTimeZone);

Console.WriteLine("utcDate: "+utcDate);
Console.WriteLine("India Time: " + indiaTime);

    // Generate reset token
    var token = Guid.NewGuid().ToString();
    var resetToken = new PasswordResetToken
    {
        Token = token,
        Email = model.Email,
        CreatedAt = indiaTime
    };

    // Store token in database
    _pontext.PasswordResetTokens.Add(resetToken);
    await _pontext.SaveChangesAsync();

    // Construct the desired reset link
    var resetLink = $"http://127.0.0.1:5500/ResetPassword.html?token={token}&email={WebUtility.UrlEncode(model.Email)}";

    // Send email
    await _emailSender.SendEmailAsync(model.Email, "Password Reset", $"Click the link to reset your password: {resetLink}");

    _logger.LogInformation("Password reset link sent to: {Email}", model.Email);  // Log info
    return Ok("Password reset link has been sent to your email.");
}


[HttpPost("verify-token-and-update-password")]
public async Task<IActionResult> VerifyTokenAndUpdatePassword([FromBody] VerifyTokenRequest model)
{
    // Validate the incoming request
    if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NewPassword) || string.IsNullOrEmpty(model.ConfirmPassword))
    {
        return BadRequest(new { status = false, message = "Email, token, and new password are required." });
    }

    // Check if new password and confirm password match
    if (model.NewPassword != model.ConfirmPassword)
    {
        return BadRequest(new { status = false, message = "Passwords do not match." });
    }

    // Step 1: Look for the token in the database
    var tokenRecord = await _pontext.PasswordResetTokens
        .FirstOrDefaultAsync(t => t.Email == model.Email && t.Token == model.Token);

    // Step 2: Check if the token exists and is still valid (assuming a 1-hour expiration)
    if (tokenRecord == null || tokenRecord.CreatedAt.AddHours(1) < DateTime.UtcNow)
    {
        return Ok(new { status = false, message = "Invalid or expired token." });
    }

    // Step 3: Find the user by email
    var user = await _userManagero.FindByEmailAsync(model.Email);
    if (user == null)
    {
        return NotFound(new { status = false, message = "User not found." });
    }

    // Step 4: Hash the new password using the UserManager's password hasher
    var passwordHasher = new PasswordHasher<ApplicationUser>();
    user.PasswordHash = passwordHasher.HashPassword(user, model.NewPassword);

    // Step 5: Update the user's password in the AspNetUsers table
    var updateResult = await _userManagero.UpdateAsync(user);

    if (updateResult.Succeeded)
    {
        // Step 6: Optionally delete the token from PasswordResetTokens after a successful password update
        _pontext.PasswordResetTokens.Remove(tokenRecord);
        await _pontext.SaveChangesAsync();

        return Ok(new { status = true, message = "Password updated successfully." });
    }
    else
    {
        return StatusCode(500, new { status = false, message = "Failed to update password." });
    }
}


    

//         // GET: /reset-password?token=<token>
//         [HttpGet("reset-password")]
//         public IActionResult ResetPassword(string token)
//         {
//             if (string.IsNullOrEmpty(token))
//             {
//                 _logger.LogWarning("Invalid token: {Token}", token);  // Log warning
//                 return BadRequest("Invalid token.");
//             }

//             // Validate token (ensure it's not expired)
//             var tokenRecord = _pontext.PasswordResetTokens.FirstOrDefault(t => t.Token == token);
//             if (tokenRecord == null || tokenRecord.CreatedAt.AddHours(1) < DateTime.UtcNow)
//             {
//                 _logger.LogWarning("Expired or invalid token: {Token}", token);  // Log warning
//                 return BadRequest("Invalid or expired token.");
//             }

//             // Instead of returning a View, return the token
//             return Ok(new { Token = token });
//         }



//         [HttpPost("verify-token")]
// public async Task<IActionResult> VerifyToken([FromBody] VerifyTokenRequest model)
// {
//     // Validate the incoming request
//     if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Token))
//     {
//         return BadRequest(new { status = false, message = "Email and token are required." });
//     }

//     // Look for the token in the database
//     var tokenRecord = await _pontext.PasswordResetTokens
//         .FirstOrDefaultAsync(t => t.Email == model.Email && t.Token == model.Token);

//     // Check if the token exists and is still valid (assuming a 1-hour expiration)
//     if (tokenRecord == null || tokenRecord.CreatedAt.AddHours(1) < DateTime.UtcNow)
//     {
//         // Token does not exist or is expired
//         return Ok(new { status = false, message = "Invalid or expired token." });
//     }

//     // If token and email match and are valid
//     return Ok(new { status = true, message = "Token is valid." });
// }



        

//         [HttpPost("reset-password")]
//         public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
//         {
//             if (!ModelState.IsValid)
//             {
//                 // Return the validation errors
//                 return BadRequest(ModelState);
//             }

//             // Check if the token is valid and not expired
//             var tokenRecord = await _pontext.PasswordResetTokens
//                 .FirstOrDefaultAsync(t => t.Token == model.Token && t.Email == model.Email);

//             if (tokenRecord == null)
//             {
//                 _logger.LogError("Token not found. Token: {Token}, Email: {Email}", model.Token, model.Email);  // Log error
//                 return BadRequest("Invalid token.");
//             }

//             // Check for token expiration (1 hour expiration time)
//             if (tokenRecord.CreatedAt.AddHours(1) < DateTime.UtcNow)
//             {
//                 _logger.LogError("Token expired. Token: {Token}, Email: {Email}", model.Token, model.Email);  // Log error
//                 return BadRequest("Invalid or expired token.");
//             }

//             // Fetch the user from the database based on the email
//             var user = await _userManager.FindByEmailAsync(model.Email);
//             if (user == null)
//             {
//                 _logger.LogError("User not found. Email: {Email}", model.Email);  // Log error
//                 return BadRequest("User not found.");
//             }

//             // Reset the password
//             var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
//             if (!result.Succeeded)
//             {
//                 _logger.LogError("Password reset failed. Email: {Email}, Errors: {Errors}", model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));  // Log error
//                 return BadRequest(result.Errors);
//             }

//             // If password reset is successful, remove the token from the database
//             _pontext.PasswordResetTokens.Remove(tokenRecord);
//             await _pontext.SaveChangesAsync();

//             _logger.LogInformation("Password reset successfully for email: {Email}", model.Email);  // Log info
//             return Ok("Password reset successfully.");
//         }
//     }


    public class VerifyTokenModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }



}
}
