using Microsoft.AspNetCore.Mvc;
using ModelTest.Data.UnitOfWork;
using ModelTest.DTOs;
using ModelTest.Models;

namespace ModelTest.Data.Services.DoctorService
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DoctorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddDoctorAsync(Doctor doctor)
        {
            await _unitOfWork.Doctors.AddAsync(doctor);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto changePasswordDto, int doctorId)
        {
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
            {
                return new BadRequestObjectResult("New password and confirm password do not match.");
            }

            var doctor = await _unitOfWork.Doctors.FindByIdAsync(doctorId);
            if (doctor == null)
            {
                return new NotFoundObjectResult("Doctor not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, doctor.Password))
            {
                return new BadRequestObjectResult("Old password is incorrect.");
            }
            doctor.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _unitOfWork.Doctors.UpdateAsync(doctor);
            return new OkObjectResult(doctor);
        }

        public async Task DeleteDoctorAsync(int id)
        {
            await _unitOfWork.Doctors.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
        {
            return await _unitOfWork.Doctors.GetAllAsync();
        }

        public async Task<Doctor> GetDoctorByIdAsync(int id)
        {
            return await _unitOfWork.Doctors.FindByIdAsync(id);
        }

        public async Task UpdateDoctorAsync(Doctor doctor)
        {
            await _unitOfWork.Doctors.UpdateAsync(doctor);
            await _unitOfWork.SaveAsync();
        }
    }
}
