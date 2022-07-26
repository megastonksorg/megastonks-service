using Megastonks.Helpers;
using Megastonks.Middleware;
using Megastonks.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

//Configure My Services
builder.Services.AddDbContext<DataContext>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Configure Scoped Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseCors(x => x
   .SetIsOriginAllowed(origin => true)
   .AllowAnyMethod()
   .AllowAnyHeader()
   .AllowCredentials());

app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.Run();

