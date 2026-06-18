using StockPortfolio.Application.Interfaces;
using StockPortfolio.Infrastructure.Fugle;
using StockPortfolio.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<IFugleService, FugleService>();
builder.Services.AddSingleton<IPortfolioRepository>(_ =>
    new JsonPortfolioRepository(
        Path.Combine(builder.Environment.ContentRootPath, "App_Data")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
