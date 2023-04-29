using CorePush.Apple;
using Megastonks.Helpers;
using Megastonks.Middleware;
using Megastonks.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Megastonks.Entities;
using Megastonks.Hubs;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<PushNotificationSettings>(builder.Configuration.GetSection("PushNotificationSettings"));

//Configure My Services
builder.Services.AddDbContext<DataContext>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Configure Scoped Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IMediaUploadService, MediaUploadService>();
builder.Services.AddScoped<ITribeService, TribeService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
builder.Services.AddHttpClient<ApnSender>();

builder.Services.AddSignalR().AddAzureSignalR(builder.Configuration.GetConnectionString("AzureSignalRService"));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        if (options.Events == null)
            options.Events = new();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["AppSettings:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
        };

        options.Events.OnTokenValidated = async context =>
        {
            var token = context.SecurityToken;
            var jwtToken = (JwtSecurityToken)token;
            var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            var dbContext = context.HttpContext.RequestServices.GetService<DataContext>();
            context.HttpContext.Items["Account"] = await dbContext.Accounts.FindAsync(accountId);
            Account account = context.HttpContext.Items["Account"] as Account;
        };
    });

builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.UseCors(x => x
   .SetIsOriginAllowed(origin => true)
   .AllowAnyMethod()
   .AllowAnyHeader()
   .AllowCredentials());

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();
app.MapHub<AppHub>("/appHub");

app.Run();