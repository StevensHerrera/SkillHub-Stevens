using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Services;
using SkillHub.Infrastructure.Excel;
using SkillHub.Infrastructure.Identity;
using SkillHub.Infrastructure.Pdf;
using SkillHub.Infrastructure.Persistence;
using SkillHub.Infrastructure.Persistence.Repositories;

namespace SkillHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=skillhub.db";

        services.AddDbContext<SkillHubDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IWorkshopRepository, WorkshopRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IParticipantRepository, ParticipantRepository>();
        services.AddScoped<IInstructorRepository, InstructorRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ICertificatePdfGenerator, QuestPdfCertificateGenerator>();
        services.AddScoped<IEnrollmentExcelParser, ClosedXmlEnrollmentParser>();

        services.AddScoped<AuthService>();
        services.AddScoped<WorkshopService>();
        services.AddScoped<EnrollmentService>();
        services.AddScoped<ReportService>();
        services.AddScoped<InstructorService>();
        services.AddScoped<ParticipantService>();

        return services;
    }
}
