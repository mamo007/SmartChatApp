using DALayer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Entities
{
    public class SmartChatContext: IdentityDbContext<User,IdRoles, int>
    {
        public virtual DbSet<Messages> Messages { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<IdRoles> Roles{ get; set; }
        public SmartChatContext() { }
        public SmartChatContext(DbContextOptions<SmartChatContext> options) : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(DBManager.GetConnectionString());

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.UseCollation("utf8mb4");
            builder.Entity<Messages>().Property(e => e.Message)
                .UseCollation("utf8mb4_bin");
            base.OnModelCreating(builder);
        }
    }
}
