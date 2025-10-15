using Microsoft.EntityFrameworkCore;
using VideoCallApp.Models;

namespace VideoCallApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<CallLog> CallLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasOne(m => m.Sender)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Receiver)
                      .WithMany(u => u.ReceivedMessages)
                      .HasForeignKey(m => m.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CallLog>(entity =>
            {
                entity.HasOne(c => c.Caller)
                      .WithMany(u => u.InitiatedCalls)
                      .HasForeignKey(c => c.CallerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Callee)
                      .WithMany(u => u.ReceivedCalls)
                      .HasForeignKey(c => c.CalleeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
