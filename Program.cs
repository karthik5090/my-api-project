using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using ForgotPasswordApp.Models;
using Microsoft.AspNetCore.Cors;
using ForgotPasswordApp.Services;
using Serilog; // Ensure this is correctly referenced

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure EF Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();  // Add this line if you're using token providers like for password reset

// Add the IEmailSender service
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Add CORS services (must be before building app)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://127.0.0.1:5502","http://127.0.0.1:5500")  // Allow your frontend URL
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/myapp.log", rollingInterval: RollingInterval.Day)  // Log to a file
    .CreateLogger());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enable CORS (must be before UseRouting)
app.UseCors("AllowSpecificOrigin");

// Use HTTPS Redirection
app.UseHttpsRedirection();
app.UseStaticFiles();

// Use Routing and Authentication
app.UseRouting();
app.UseAuthentication(); // Add Authentication middleware
app.UseAuthorization();  // Add Authorization middleware

app.MapControllers();

app.Run();
