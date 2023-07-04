﻿using AppMvc.Models.Contacts;
using AppMvc.Models.Blog;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            // Bỏ tiền tố AspNet của các bảng: mặc định
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }
            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
            });
            builder.Entity<PostCategory>( entity => {
                entity.HasKey( c => new {c.PostID, c.CategoryID});
                
            });

            builder.Entity<Post>( entity => {
                entity.HasIndex( p => p.Slug)
                      .IsUnique();
            });
        }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Category> Categories { get; set; } 
        public DbSet<Post> Posts {get; set;}
        public DbSet<PostCategory> PostCategories {get;set;}
    }
}
