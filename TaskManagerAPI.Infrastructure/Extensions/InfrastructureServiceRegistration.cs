using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Infrastructure.BackgroundServices;
using TaskManagerAPI.Infrastructure.Data;
using TaskManagerAPI.Infrastructure.Data.Repositories;
using TaskManagerAPI.Infrastructure.Options;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Infrastructure.Extensions;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));


        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        services.AddScoped<ITaskListRepository, TaskListRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICheckAccessService, CheckAccessService>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<ILabelService, LabelService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITaskListService, TaskListService>();
        services.AddScoped<ICommentService,  CommentService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddHostedService<DueDateNotificationWorker>();
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
}