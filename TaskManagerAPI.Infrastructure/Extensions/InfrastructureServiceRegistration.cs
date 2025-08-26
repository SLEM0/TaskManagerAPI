using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Infrastructure.Data;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Infrastructure.Extensions;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Services
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

        return services;
    }
}