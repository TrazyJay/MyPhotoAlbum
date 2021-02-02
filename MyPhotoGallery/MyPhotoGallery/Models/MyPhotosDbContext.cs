using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MyPhotoGallery.Models
{
    public partial class MyPhotosDbContext : DbContext
    {
        public MyPhotosDbContext()
        {
        }

        public MyPhotosDbContext(DbContextOptions<MyPhotosDbContext> options)
            : base(options)
        {
        }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
