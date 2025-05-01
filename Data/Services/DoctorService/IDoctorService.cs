using Microsoft.AspNetCore.Mvc;
using ModelTest.DTOs;
using ModelTest.Models;

namespace ModelTest.Data.Services.DoctorService
{
    public interface IDoctorService
    {
        Task<Doctor> GetDoctorByIdAsync(int id);
        Task<IEnumerable<Doctor>> GetAllDoctorsAsync();
        Task AddDoctorAsync(Doctor doctor);
        Task UpdateDoctorAsync(Doctor doctor);
        Task DeleteDoctorAsync(int id);
        Task <IActionResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto, int doctorId);
    }
}
