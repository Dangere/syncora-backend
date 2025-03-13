using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

//Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

//Add controllers, Lifecycle: Transient but behaves like scoped because it gets created per HTTP request
builder.Services.AddControllers();

//Add TaskServices, Lifecycle: Scoped
builder.Services.AddScoped<TaskServices>();

//Add DbContext to the services, Lifecycle: Scoped
builder.Services.AddDbContext<SyncoraDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.MapGet("/", () => "Hello World!");
app.Run();
