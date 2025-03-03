using TaskManagementWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

//Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();
builder.Services.AddSingleton<TaskServices>();


var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.MapGet("/", () => "Hello World!");
app.Run();
