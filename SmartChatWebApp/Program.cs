using ChatCore.Services.MessageHandlers;
using DBLayer.Entities;
using DBLayer.EntitiesManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SmartChatWebApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IChatManager, ChatManager>();
builder.Services.AddTransient<IMessageManager, MessageManager>();
builder.Services.AddTransient<IUserManager, UserManager>();
builder.Services.AddScoped<IMessageHandler, SimpleMessageHandler>();
builder.Services.AddScoped<IMessageHandler, UserListMessageHandler>();
builder.Services.AddScoped<IMessageHandler, PrivateMessageHandler>();

builder.Services.AddDbContext<SmartChatContext>(a =>
    a.UseSqlServer(builder.Configuration.GetConnectionString("SmartChatConn")));

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdRoles>()
    .AddEntityFrameworkStores<SmartChatContext>();

//Access ClaimPrinciples
builder.Services.AddHttpContextAccessor();

builder.Services.AddSignalR();

var app = builder.Build();//

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapHub<ChatHub>("/chatHub");

app.Run();
