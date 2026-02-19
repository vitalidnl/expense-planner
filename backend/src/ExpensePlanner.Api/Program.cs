using ExpensePlanner.Application;
using ExpensePlanner.Api.Validation.Transactions;
using ExpensePlanner.Api.Services;
using ExpensePlanner.DataAccess;
using ExpensePlanner.DataAccess.Csv;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTransactionRequestValidator>();

// Add Swagger/OpenAPI in development
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Expense Planner API",
        Version = "v1",
        Description = "REST API for expense planning with recurrence support"
    });
});

// Add basic DI wiring for generic repository (placeholder)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var csvOptions = builder.Configuration.GetSection("Storage:Csv").Get<CsvStorageOptions>() ?? new CsvStorageOptions();
var csvStorageInitializer = new CsvStorageInitializer();
await csvStorageInitializer.EnsureCreatedAsync(builder.Environment.ContentRootPath, csvOptions);

builder.Services.AddScoped<ITransactionRepository>(_ =>
    new CsvTransactionRepository(builder.Environment.ContentRootPath, csvOptions, csvStorageInitializer));

builder.Services.AddScoped<IRecurringTransactionRepository>(_ =>
    new CsvRecurringTransactionRepository(builder.Environment.ContentRootPath, csvOptions, csvStorageInitializer));

builder.Services.AddScoped<IRecurrenceRuleRepository>(_ =>
    new CsvRecurrenceRuleRepository(builder.Environment.ContentRootPath, csvOptions, csvStorageInitializer));

builder.Services.AddScoped<IDataResetRepository>(_ =>
    new CsvDataResetRepository(builder.Environment.ContentRootPath, csvOptions, csvStorageInitializer));

builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<DataResetService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Planner API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
