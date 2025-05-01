using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ModelTest.Data.Services.DoctorService;
using ModelTest.DTOs;
using ModelTest.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ModelTest.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IConfiguration _configuration;
        public AuthController(IDoctorService doctorService, IConfiguration configuration)
        {
            _configuration = configuration;
            _doctorService = doctorService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DoctorRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingDoctor = await _doctorService.GetAllDoctorsAsync();
            if (existingDoctor.Any(d => d.Email == registerDto.Email))
            {
                return BadRequest("Email already registered.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var doctor = new Doctor
            {
                Email = registerDto.Email,
                Password = hashedPassword,
                PhoneNumber = registerDto.PhoneNumber,
                Name = registerDto.Name,
            };

            await _doctorService.AddDoctorAsync(doctor);
            return Ok(new { message = "Doctor registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DoctorLoginDto loginDto)
        {
            var doctors = await _doctorService.GetAllDoctorsAsync();
            var doctor = doctors.FirstOrDefault(d => d.Email == loginDto.Email);

            if (doctor == null)
            {
                return Unauthorized("Invalid credentials.");
            }


            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, doctor.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = GenerateJwtToken(doctor);


            return Ok(new { token });
        }

        private string GenerateJwtToken(Doctor doctor)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, doctor.Id.ToString()),
                new Claim(ClaimTypes.Email, doctor.Email),                
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), 
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"], 
                Audience = _configuration["Jwt:Audience"] 
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var doctorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (doctorIdClaim == null)
            {
                return Unauthorized("Doctor ID claim not found.");
            }

            if (!int.TryParse(doctorIdClaim.Value, out int doctorId))
            {
                return Unauthorized("Invalid Doctor ID format.");
            }

            var result = await _doctorService.ChangePasswordAsync(changePasswordDto, doctorId);
            return result;
        }
        [HttpDelete("id")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            await _doctorService.DeleteDoctorAsync(id);
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return BadRequest();
            }

            var existingDoctor = await _doctorService.GetDoctorByIdAsync(id);
            if (existingDoctor == null)
            {
                return NotFound();
            }

            await _doctorService.UpdateDoctorAsync(doctor);
            return NoContent();
        }
    }
}
