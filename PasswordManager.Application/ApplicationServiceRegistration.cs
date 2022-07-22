using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Application.Interface.Login;
using PasswordManager.Domain.Models;
using System.Reflection;
namespace PasswordManager.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddScoped<IUserService, UserService>();
            return services;
        }
    }
}
