using BakeryPay.Backend.Interfaces.Repositories;
using BakeryPay.Backend.Interfaces.Security;
using BakeryPay.Backend.Interfaces.Services;
using BakeryPay.Backend.Services;
using BakeryPay.Backend.Email;
using BakeryPay.Backend.Data;
using BakeryPay.Backend.Repositories;
using BakeryPay.Backend.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BakeryPay.Backend.Configuration;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var smtpSection = configuration.GetSection(SmtpSettings.SectionName);
        services.Configure<SmtpSettings>(options =>
        {
            options.Enabled = bool.TryParse(smtpSection["Enabled"], out var enabled) && enabled;
            options.Host = smtpSection["Host"] ?? string.Empty;
            options.Port = int.TryParse(smtpSection["Port"], out var port) ? port : 587;
            options.UseSsl = !bool.TryParse(smtpSection["UseSsl"], out var useSsl) || useSsl;
            options.UseDefaultCredentials = bool.TryParse(smtpSection["UseDefaultCredentials"], out var useDefaultCredentials) && useDefaultCredentials;
            options.Username = smtpSection["Username"] ?? string.Empty;
            options.Password = smtpSection["Password"] ?? string.Empty;
            options.FromEmail = smtpSection["FromEmail"] ?? string.Empty;
            options.FromName = smtpSection["FromName"] ?? "BakeryPay";
        });

        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(options =>
        {
            options.Issuer = jwtSection["Issuer"] ?? string.Empty;
            options.Audience = jwtSection["Audience"] ?? string.Empty;
            options.SecretKey = jwtSection["SecretKey"] ?? string.Empty;
            options.ExpirationMinutes = int.TryParse(jwtSection["ExpirationMinutes"], out var expirationMinutes)
                ? expirationMinutes
                : 120;
        });

        services.AddDbContext<BakeryPayDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing.");

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BakeryPayDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IUserBiometricRepository, UserBiometricRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProviderService, ProviderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
