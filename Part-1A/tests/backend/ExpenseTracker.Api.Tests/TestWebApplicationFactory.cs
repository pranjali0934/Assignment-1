using ExpenseTracker.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExpenseTracker.Api.Tests;

/// <summary>
/// Swaps the SQL Server DbContext registration for a uniquely-named InMemory database per
/// factory instance, so contract tests run fast and in isolation without a real SQL Server.
///
/// AddDbContext registers an IDbContextOptionsConfiguration&lt;T&gt; entry that ACCUMULATES
/// across multiple AddDbContext calls (it doesn't just replace DbContextOptions&lt;T&gt;), so
/// both Program.cs's UseSqlServer and this fixture's UseInMemoryDatabase would otherwise both
/// get applied to the same options instance ("Only a single database provider can be
/// registered"). Removing both registrations before re-adding avoids that.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ExpenseTrackerDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ExpenseTrackerDbContext>>();
            services.AddDbContext<ExpenseTrackerDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}
