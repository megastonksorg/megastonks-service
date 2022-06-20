using Microsoft.EntityFrameworkCore;
using Megastonks.Entities;

namespace Megastonks.Helpers
{
	public class DataContext : DbContext
	{
		public DbSet<Account> Accounts { get; set; }

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
	}
}