using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoloBill.Data;
using SoloBill.Services;
using System.Globalization;
using SoloBill.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SoloBillDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        var sqlite = builder.Configuration.GetConnectionString("SoloBillSqlite")
                     ?? "Data Source=solobill.db";
        options.UseSqlite(sqlite);
    }
    else
    {
        var sqlServer = builder.Configuration.GetConnectionString("SoloBillDbContext")
                      ?? throw new InvalidOperationException(
                          "Connection string 'SoloBillDbContext' not found.");
        options.UseSqlServer(sqlServer);
    }
});
    


builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<SoloBillDbContext>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();

// PDF service
builder.Services.AddScoped<InvoicePdfService>();

//Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton<IEmailService, EmailService>();

var app = builder.Build();

+// Auto-apply EF Core migrations at startup (esp. useful in Production)
+using (var scope = app.Services.CreateScope())
+{
+    var db = scope.ServiceProvider.GetRequiredService<SoloBillDbContext>();
+    db.Database.Migrate();
+}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();           
    app.UseMigrationsEndPoint();
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}
else
{
    app.UseExceptionHandler("/Error/500");    
    app.UseHsts();
}


var cultureInfo = new CultureInfo("en-IE");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();