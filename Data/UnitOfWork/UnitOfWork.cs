using ModelTest.Data.DataBase;
using ModelTest.Data.Repository;
using ModelTest.Models;

namespace ModelTest.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Doctors = new GenericRepository<Doctor>(_context);
            Patients = new GenericRepository<Patient>(_context);
        }

        public IGenericRepository<Doctor> Doctors { get; set; }

        public IGenericRepository<Patient> Patients { get; set; }

        public void Dispose()
        {
            _context.Dispose();
        }

        public Task SaveAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
