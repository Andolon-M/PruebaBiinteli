using Microsoft.EntityFrameworkCore;
using PruebaBiinteli.Models;

namespace PruebaBiinteli.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Flights> Flights { get; set; }
    }
}
