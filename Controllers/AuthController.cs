using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ModelTest.Data.Interfaces;
using ModelTest.Data.Repository;
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
        private readonly IGenericRepository<Doctor> _repository;
        private readonly IConfiguration _configuration;
        public AuthController(IGenericRepository<Doctor> repository, IConfiguration configuration)
        {
            _configuration = configuration;
            _repository = repository;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DoctorRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingDoctor = await _repository.GetAllAsync();
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

            await _repository.AddAsync(doctor);
            return Ok(new { message = "Doctor registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DoctorLoginDto loginDto)
        {
            var doctors = await _repository.GetAllAsync();
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
    }
}
