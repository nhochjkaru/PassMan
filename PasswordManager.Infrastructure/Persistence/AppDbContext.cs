using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor contextAccessor) : base(options)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var mapper = new Npgsql.NameTranslation.NpgsqlSnakeCaseNameTranslator();
            var types = modelBuilder.Model.GetEntityTypes().ToList();
        }
        public DbSet<User> dbUsers { get; set; }
        public DbSet<LoginActivities> dbLoginActivities { get; set; }
        public DbSet<UserCred> dbUserCred { get; set; }
        public DbSet<UserCredHistory> dbUserCredHistory { get; set; }
    }
}