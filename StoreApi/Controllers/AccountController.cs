using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using StoreApi.Models;
using StoreApi.Services;

namespace StoreApi.Controllers;

[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    public AccountController(IConfiguration configuration, ApplicationDbContext context, EmailService emailService)
    {
        _configuration = configuration;
        _context = context;
        _emailService = emailService;
    }

    [HttpPost("Register")]
    public IActionResult Register(UserDto userDto)
    {
        //Check if the email address is already is Database or not
        var emailCount = _context.Users.Count(user => user.Email == userDto.Email);
        if (emailCount > 0)
        {
            ModelState.AddModelError("Email", "This Email Address is already used!");
            return BadRequest(ModelState);
        }

        // encrypt the password
        var passwordHasher = new PasswordHasher<User>();
        var encryptedPassword = passwordHasher.HashPassword(new User(), userDto.Password);

        // create new account
        var user = new User
        {
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Email = userDto.Email,
            Phone = userDto.Phone,
            Address = userDto.Address,
            Password = encryptedPassword,
            Role = "Client",
            CreatedAt = DateTime.Now
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        var jwt = CreateJWToken(user);

        var userProfileDto = new UserProfileDto()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = DateTime.Now
        };
        var response = new
        {
            Token = jwt,
            User = userProfileDto
        };
        return Ok(response);
    }

    [HttpPost("Login")]
    public IActionResult Login(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(user => user.Email == email);
        if (user == null)
        {
            ModelState.AddModelError("Error", "Email or Password is not valid");
            return BadRequest(ModelState);
        }

        //verify password
        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(new User(), user.Password, password);
        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("Password", "Wrong Password");
            return BadRequest(ModelState);
        }

        var jwt = CreateJWToken(user);
        var userProfileDto = new UserProfileDto()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = DateTime.Now
        };
        var response = new
        {
            Token = jwt,
            User = userProfileDto
        };

        return Ok(response);
    }

    [Authorize]
    [HttpGet("GetTokenClaims")]
    public IActionResult GetTokenClaims()
    {
        var identity = User.Identity as ClaimsIdentity;
        if (identity != null)
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();
            foreach (var claim in identity.Claims)
            {
                claims.Add(claim.Type, claim.Value);
            }

            return Ok(claims);
        }

        return Ok();
    }

    [HttpPost("ForgotPassword")]
    public IActionResult ForgotPassword(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            return NotFound();
        }
        
        //delete any old password reset 
        var oldPasswordReset = _context.PasswordResets.FirstOrDefault(r => r.Email == email);
        if (oldPasswordReset != null)
        {
            _context.Remove(oldPasswordReset);
        }
        
        //create passsword reset token
        string token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();
        var pswReset = new PasswordReset()
        {
            Email = email,
            Token = token,
            CreatedAt = DateTime.Now
        };
        _context.PasswordResets.Add(pswReset);
        _context.SaveChanges();
        
        //send password reset token by email 
        string emailSubject = "Password Reset";
        string username = user.FirstName + " " + user.LastName;
        string emailMessage = "Dear" + username + "\n" +
                              "we received your password reset request.\n" +
                              "please copy the following token and paste it in the password reset form\n" +
                              token + "\n\n" +
                              "Best Regards\n";
        _emailService.SendEmail(emailSubject , emailMessage);
        return Ok();
    }

    [HttpPost("ResetPassword")]
    public IActionResult ResetPassword(string token, string password)
    {
        var pwdReset = _context.PasswordResets.FirstOrDefault(r => r.Token == token);
        if (pwdReset == null)
        {
            ModelState.AddModelError("Token", "Wrong Token or invalid token");
            return BadRequest(ModelState);
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == pwdReset.Email);
        if (user == null)
        {
            ModelState.AddModelError("Token", "Wrong Token or invalid token");
            return BadRequest(ModelState);
        }
        
        //encrypted password
        var passwordHasher = new PasswordHasher<User>();
        var encryptedPassword = passwordHasher.HashPassword(new User(), password);
        
        // save the new hashed password
        user.Password = encryptedPassword;
        
        // delete the token
        _context.PasswordResets.Remove(pwdReset);

        _context.SaveChanges();

        return Ok();
    }

    [Authorize]
    [HttpGet("Profile")]
    public IActionResult GetProfile()
    {
        var identity = User.Identity as ClaimsIdentity;
        if (identity == null)
        {
            return Unauthorized();
        }

        var claim = identity.Claims.FirstOrDefault(c => c.Type.ToLower() == "id");
        if (claim == null)
        {
            return Unauthorized();
        }

        int id;
        try
        {
            id = int.Parse(claim.Value);
        }
        catch (Exception e)
        {
            ModelState.AddModelError("Profile", e.ToString());
            return Ok(ModelState);
        }
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return Unauthorized();
        }

        var userProfileDto = new UserProfileDto()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
        return Ok(userProfileDto);
    }
    
    // [Authorize]
    // [HttpGet("AuthorizeAuthenticatedUser")]
    // public IActionResult AuthorizeAuthenticatedUser()
    // {
    //     return Ok("You Are Authorized");
    // }
    //
    // [Authorize(Roles = "admin")]
    // [HttpGet("AuthorizeAdmin")]
    // public IActionResult AuthorizeAdmin()
    // {
    //     return Ok("You Are Authorized");
    // }
    //
    // [Authorize(Roles = "admin, seller")]
    // [HttpGet("AuthorizeAdminAndSeller")]
    // public IActionResult AuthorizeAdminAndSeller()
    // {
    //     return Ok("You Are Authorized");
    // }

    // [HttpGet("TestToken")]
    // public IActionResult TestToken()
    // {
    //     var user = new User()
    //     {
    //         Id = 2,
    //         Role = "admin"
    //     };
    //     var jwt = CreateJWToken(user);
    //     var response = new { jwToken = jwt };
    //     return Ok(response);
    // }
    private string CreateJWToken(User user)
    {
        List<Claim> claims = new List<Claim>()
        {
            new Claim("id", "" + user.Id),
            new Claim("role", user.Role)
        };
        var strKey = _configuration["JwtSettings:key"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }
}