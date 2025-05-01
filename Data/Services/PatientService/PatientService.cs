using ModelTest.Data.UnitOfWork;
using ModelTest.Models;

namespace ModelTest.Data.Services.PatientService
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PatientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddPatientAsync(Patient patient)
        {
            await _unitOfWork.Patients.AddAsync(patient);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeletePatientAsync(int id)
        {
            await _unitOfWork.Patients.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            return await _unitOfWork.Patients.GetAllAsync();
        }

        public async Task<Patient> GetPatientByIdAsync(int id)
        {
            return await _unitOfWork.Patients.FindByIdAsync(id);
        }

        public async Task UpdatePatientAsync(Patient patient)
        {
            await _unitOfWork.Patients.UpdateAsync(patient);
            await _unitOfWork.SaveAsync();
        }
    }
}
