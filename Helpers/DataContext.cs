using Microsoft.EntityFrameworkCore;
using Megastonks.Entities;

namespace Megastonks.Helpers
{
    public class DataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Tribe> Tribes { get; set; }
        public DbSet<TribeInviteCode> TribeInviteCodes { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<MessageViewer> MessageViewers { get; set; }

        private readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to SqlServer database
            options.UseSqlServer(Configuration.GetConnectionString("WebApiDatabase"));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Account>()
                .HasIndex(x => new { x.WalletAddress })
                .IsUnique();

            builder.Entity<Account>()
                .HasIndex(x => new { x.PublicKey })
                .IsUnique();

            builder.Entity<Account>()
              .Property(x => x.DeviceType)
              .HasConversion<string>();

            builder.Entity<Account>()
              .Property(x => x.Role)
              .HasConversion<string>();

            builder.Entity<TribeInviteCode>()
                .HasIndex(inviteCode => inviteCode.Code)
                .IsUnique();

            builder.Entity<Message>()
              .Property(x => x.Type)
              .HasConversion<string>();

            builder.Entity<Message>()
                .Property(x => x.Tag)
                .HasConversion<string>();
        }
    }
}