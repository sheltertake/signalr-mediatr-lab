using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;

namespace BackendApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddTransient<ChatService>();
            services.AddSignalR();
            services.AddControllers();
            services.AddMediatR(typeof(Startup));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BackendApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BackendApi v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/notifications");
            });



            app.UseSpa(spa =>
            {
                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }

    public class NotificationHub : Hub
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(
            IMediator mediator,
            ILogger<NotificationHub> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task SendPingNotification(string data)
        {
            _logger.LogInformation(nameof(NotificationHub));
            _logger.LogInformation(nameof(SendPingNotification));
            return _mediator.Publish(new PingNotification(Context.ConnectionId, data));
        }
    }
    public class PingNotification : INotification
    {
        public string ConnectionId { get; }
        public string Data { get; }

        public PingNotification(string connectionId, string data)
        {
            ConnectionId = connectionId;
            Data = data;
        }
    }
    public class PingNotificationHandler : INotificationHandler<PingNotification>
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMediator _mediator;
        private readonly ILogger<PingNotificationHandler> _logger;

        public PingNotificationHandler(
            IHubContext<NotificationHub> hubContext,
            IMediator mediator,
            ILogger<PingNotificationHandler> logger)
        {
            _hubContext = hubContext;
            _mediator = mediator;
            _logger = logger;
        }

        public Task Handle(PingNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(nameof(PingNotificationHandler));
            _logger.LogInformation(nameof(Handle));
            _logger.LogInformation(JsonSerializer.Serialize(notification));
            return _mediator.Publish(new PongNotification(notification.ConnectionId, notification.Data));
        }
    }
    public class PongNotificationHandler : INotificationHandler<PongNotification>
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMediator _mediator;
        private readonly ILogger<PongNotificationHandler> _logger;

        public PongNotificationHandler(
            IHubContext<NotificationHub> hubContext,
            IMediator mediator,
            ILogger<PongNotificationHandler> logger)
        {
            _hubContext = hubContext;
            _mediator = mediator;
            _logger = logger;
        }

        public Task Handle(PongNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(nameof(PongNotificationHandler));
            _logger.LogInformation(nameof(Handle));
            _logger.LogInformation(JsonSerializer.Serialize(notification));
            var c = _hubContext.Clients.Clients(notification.ConnectionId);
            return c.SendAsync("SendPongNotification", notification.Data);
        }

    }
    
    public class PongNotification : INotification
    {
        public string ConnectionId { get; }
        public string Data { get; }

        public PongNotification(string connectionId, string data)
        {
            ConnectionId = connectionId;
            Data = data;
        }
    }
}
