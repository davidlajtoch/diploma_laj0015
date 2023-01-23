using DiplomaThesis.Server.Models;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;

namespace DiplomaThesis.Server.Data;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions options,
        IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
    {
    }

    public DbSet<UserGroup> UserGroups { get; set; } = null!;
    public DbSet<ReportDb> Reports { get; set; } = null!;
    public DbSet<DatasetDb> Datasets { get; set; } = null!;
    public DbSet<Activity> Activities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var splitStringConverter = new ValueConverter<IEnumerable<string>, string>(
            v => string.Join(";", v),
            v => v.Split(new[] { ';' })
        );
        builder.Entity<DatasetDb>().Property(nameof(DatasetDb.ColumnNames)).HasConversion(splitStringConverter);
        builder.Entity<DatasetDb>().Property(nameof(DatasetDb.ColumnTypes)).HasConversion(splitStringConverter);

        builder.Entity<IdentityRole>().HasData(new IdentityRole
        {
            Id = "0ec7c133-c8f9-4887-b7a8-05a32466a584",
            ConcurrencyStamp = "e7189548-e780-49bb-9919-0a46280e014c",
            Name = "Admin",
            NormalizedName = "Admin".ToUpper()
        });
        builder.Entity<IdentityRole>().HasData(new IdentityRole
        {
            Id = "3159c51b-0c11-4f57-8547-9bc235283ef4",
            ConcurrencyStamp = "614a6174-b683-4122-a4a7-2bec6cc73143",
            Name = "Architect",
            NormalizedName = "Architect".ToUpper()
        });
    }
}