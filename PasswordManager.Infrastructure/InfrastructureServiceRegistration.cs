using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Application.Interface;
using PasswordManager.Infrastructure.Helper;
using PasswordManager.Infrastructure.Persistence;
using PasswordManager.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connection = configuration.GetConnectionString("ConnectionString");// CEncryption.Decrypt(configuration.GetConnectionString("ConnectionString"), true);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connection).AddInterceptors(new HintInterceptor()).UseLowerCaseNamingConvention()).AddScoped<IRepositoryFactory, UnitOfWork<AppDbContext>>();
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(typeof(IAsyncRepository<>), typeof(RepositoryBase<>));

            services.AddScoped<IUnitOfWork, UnitOfWork<AppDbContext>>();
            services.AddScoped<IUnitOfWork<AppDbContext>, UnitOfWork<AppDbContext>>();
            return services;
        }
    }
}
