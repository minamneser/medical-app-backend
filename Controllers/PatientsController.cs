using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelTest.Data.Repository;
using ModelTest.Data.Services.PatientService;
using ModelTest.DTOs;
using ModelTest.Models;
using System.Security.Claims;

namespace ModelTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var doctorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(doctorIdClaim, out int doctorId))
            {
                return Unauthorized("Invalid Doctor ID");
            }

            var allPatients = await _patientService.GetAllPatientsAsync(); 
            var filteredPatients = allPatients.Where(p => p.DoctorId == doctorId);

            return Ok(filteredPatients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patients = await _patientService.GetAllPatientsAsync();
            var patient = patients.FirstOrDefault(p => p.Id == id);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto patientDto)
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

            var patient = new Patient
            {
                Name = patientDto.Name,
                Age = patientDto.Age,
                PhoneNumber = patientDto.PhoneNumber,
                GPD = patientDto.GPD,
                GRDA = patientDto.GRDA,
                IPD = patientDto.IPD,
                IRDA = patientDto.IRDA,
                Seizure = patientDto.Seizure,
                Other = patientDto.Other,
                DoctorId = doctorId, 
                DateOfCreation = DateTime.UtcNow
            };

            await _patientService.AddPatientAsync(patient);
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
                await _patientService.UpdatePatientAsync(patient);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _patientService.GetPatientByIdAsync(id) == null)
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
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            await _patientService.DeletePatientAsync(id);
            return NoContent();
        }
    }
}
