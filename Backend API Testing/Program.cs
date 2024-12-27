
using Microsoft.EntityFrameworkCore;
using VNPAY.NET;
using VNPAY.NET.Models;

namespace Backend_API_Testing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add VNPAY service to the container.
            builder.Services.AddScoped<IVnpay, Vnpay>();  // Đổi thành Scoped thay vì Singleton



            // Configure DbContext with SQL Server
            builder.Services.AddDbContext<VnpayDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("VnpayDatabase"))
                    .EnableSensitiveDataLogging() // Cho phép ghi log chi tiết
                    .LogTo(Console.WriteLine, LogLevel.Information));


            builder.Services.AddControllers();

            // Add Swagger UI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "VNPAY API with ASP.NET Core",
                    Version = "v1",
                    Description = "Created by Ngo Minh Thuan"
                });
            });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API VNPay v1");
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
