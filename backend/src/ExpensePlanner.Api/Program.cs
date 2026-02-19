var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Swagger/OpenAPI in development
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new(new() { Title = "Expense Planner API", Version = "v1" }));
});

// Add basic DI wiring (to be expanded with actual services)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

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

/// <summary>
/// Generic repository interface for future implementation
/// </summary>
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

/// <summary>
/// Placeholder repository implementation
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult(Enumerable.Empty<T>());
    public Task<T?> GetByIdAsync(int id) => Task.FromResult((T?)null);
    public Task AddAsync(T entity) => Task.CompletedTask;
    public Task UpdateAsync(T entity) => Task.CompletedTask;
    public Task DeleteAsync(int id) => Task.CompletedTask;
}
