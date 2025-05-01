using ModelTest.Data.Repository;
using ModelTest.Models;

namespace ModelTest.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Doctor> Doctors { get; }
        IGenericRepository<Patient> Patients { get; }

        Task SaveAsync();
    }
}
