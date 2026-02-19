using ExpensePlanner.Application;
using ExpensePlanner.DataAccess.Csv;
using ExpensePlanner.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpensePlanner.Api.Tests;

public sealed class TransactionsApiTestFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _dataPath = Path.Combine(Path.GetTempPath(), $"expense-planner-api-tests-{Guid.NewGuid():N}");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Csv:RootPath"] = _dataPath
            });
        });

        builder.ConfigureServices(services =>
        {
            RemoveService<ITransactionRepository>(services);
            RemoveService<IRecurringTransactionRepository>(services);
            RemoveService<IRecurrenceRuleRepository>(services);
            RemoveService<IDataResetRepository>(services);

            var csvOptions = new CsvStorageOptions { RootPath = _dataPath };
            var csvStorageInitializer = new CsvStorageInitializer();

            services.AddScoped<ITransactionRepository>(_ =>
                new CsvTransactionRepository(AppContext.BaseDirectory, csvOptions, csvStorageInitializer));

            services.AddScoped<IRecurringTransactionRepository>(_ =>
                new CsvRecurringTransactionRepository(AppContext.BaseDirectory, csvOptions, csvStorageInitializer));

            services.AddScoped<IRecurrenceRuleRepository>(_ =>
                new CsvRecurrenceRuleRepository(AppContext.BaseDirectory, csvOptions, csvStorageInitializer));

            services.AddScoped<IDataResetRepository>(_ =>
                new CsvDataResetRepository(AppContext.BaseDirectory, csvOptions, csvStorageInitializer));

            var clockDescriptor = services.SingleOrDefault(descriptor => descriptor.ServiceType == typeof(IClock));
            if (clockDescriptor is not null)
            {
                services.Remove(clockDescriptor);
            }

            services.AddSingleton<IClock>(new FixedClock(new DateOnly(2025, 2, 1)));
        });
    }

    public new void Dispose()
    {
        base.Dispose();

        if (Directory.Exists(_dataPath))
        {
            Directory.Delete(_dataPath, recursive: true);
        }
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateOnly today)
        {
            Today = today;
        }

        public DateOnly Today { get; }
    }

    private static void RemoveService<TService>(IServiceCollection services)
    {
        var descriptors = services.Where(descriptor => descriptor.ServiceType == typeof(TService)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
