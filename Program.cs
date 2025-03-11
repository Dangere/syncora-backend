using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

//Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

//Add controllers
builder.Services.AddControllers();

//Add TaskServices
builder.Services.AddSingleton<TaskServices>();

//Add DbContext to the services
builder.Services.AddDbContext<SyncoraDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.MapGet("/", () => "Hello World!");
app.Run();
