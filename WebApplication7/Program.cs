using WebApplication7.Controllers;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddSingleton<DatabaseHelper>(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            return new DatabaseHelper(configuration);
        });

        // Add controllers and other services
        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<IDoktorController, DoktorService>();
        builder.Services.AddScoped<IHastaController, HastaService>();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
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
            pattern: "{controller=User}/{action=Login}/{id?}");

        app.Run();
    }
}
