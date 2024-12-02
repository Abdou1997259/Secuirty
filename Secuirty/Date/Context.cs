using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Secuirty.Extentions;
using Secuirty.Models;

namespace Secuirty.Date
{
    public class Context : IdentityDbContext<User>
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);
            builder.SeedDate();
            base.OnModelCreating(builder);
        }
    }
}
