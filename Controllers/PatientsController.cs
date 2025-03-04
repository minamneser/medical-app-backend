using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelTest.Data.Interfaces;
using ModelTest.Models;
using System.Security.Claims;

namespace ModelTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IGenericRepository<Patient> _repository;
        public PatientsController(IGenericRepository<Patient> repository)
        {
            _repository = repository;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var doctorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(doctorIdClaim, out int doctorId))
            {
                return Unauthorized("Invalid Doctor ID");
            }

            var allPatients = await _repository.GetAllAsync(); 
            var filteredPatients = allPatients.Where(p => p.DoctorId == doctorId);

            return Ok(filteredPatients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await _repository.FindByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
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

            patient.DoctorId = doctorId;
            patient.DateOfCreation = DateTime.UtcNow;

            await _repository.AddAsync(patient);
            return CreatedAtAction(nameof(GetPatientById), new { id = patient.Id }, patient);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] Patient patient)
        {
            if (id != patient.Id)
            {
                return BadRequest("Incorrect ID");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _repository.UpdateAsync(patient);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _repository.FindByIdAsync(id) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpDelete("id")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _repository.FindByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
