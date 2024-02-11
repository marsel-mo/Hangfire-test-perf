using Hangfire;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(x =>
{
    x.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireStorage"));
});

builder.Services.AddHangfire(configuration => configuration
          .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          
          .UseRedisStorage("*****")
);

builder.Services.AddHangfireServer(o =>
{
     o.WorkerCount = Environment.ProcessorCount * 100;
});

var app = builder.Build();


 app.UseSwagger();
 app.UseSwaggerUI();


app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllHangfireAuthorizationFilter() }
});

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapGet("/postsJobs/{amountOfJobs}", (int amountOfJobs, IRecurringJobManager _recurringJobManager) =>
{
    for (int i = 0; i < amountOfJobs; i++)
    {

        _recurringJobManager.AddOrUpdate($"TestJobId:{Guid.NewGuid()}", () => Task.Delay(5000), $"*/{10} * * * *");
        Task.Delay(10);
    }
});

app.Run();


public class AllowAllHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = ((AspNetCoreDashboardContext)context).HttpContext;

        return true; 
    }
}

