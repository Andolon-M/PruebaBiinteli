using Microsoft.EntityFrameworkCore;
using PruebaBiinteli.Models;

namespace PruebaBiinteli.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Flights> Flights { get; set; }
        public DbSet<Transports> Transports { get; set; }
        public DbSet<Journey> Journeys { get; set; }
    }
}
