using System;
using System.Collections.Generic;
using System.Text;
using MedicalSupport.Entities;
using MedicalSupport.Helpers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedicalSupport.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<SickWarden> SickWardenRelations { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<VoiceNote> VoiceNotes { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(bool))
                    {
                        property.SetValueConverter(new BoolToIntConverter());
                    }
                }
            }

            base.OnModelCreating(builder);
        }
    }
}
