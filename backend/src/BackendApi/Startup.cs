using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
            services.AddSingleton<IRoverService, RoverService>();
            services.AddSignalR();
            services.AddMediatR(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
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
    

    public class RoverState
    {
        public string Name = "Foo";
        //public bool?[,] Map { get; private set; } = new bool?[3, 3];
        //public RoverState()
        //{
        //    Map[0, 0] = false;
        //    Map[2, 2] = false;
        //    Map[0, 2] = false;
        //    Map[2, 0] = false;
        //    Map[1, 1] = true;
        //}

    }

    public interface IRoverService
    {
        Task<RoverState> LandAsync();
    }
    public class RoverService : IRoverService
    {
        public Task<RoverState> LandAsync()
        {
            return Task.FromResult(new RoverState());
        }
    }
    public interface IPushNotificationHubClient
    {
        Task SendPongNotificationAsync(PongNotification notification);
        Task RoverLandResponseAsync(string state);
    }

    public interface IInvokeNotificationHubClient
    {
        Task SendPingNotificationAsync(string data);
        Task RoverLandRequestAsync();
    }


    public class NotificationHub : Hub<IPushNotificationHubClient>, IInvokeNotificationHubClient
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

        public Task SendPingNotificationAsync(string data)
        {
            _logger.LogInformation(nameof(NotificationHub));
            _logger.LogInformation(nameof(SendPingNotificationAsync));

            return _mediator.Publish(new PingNotification(Context.ConnectionId, data));
        }

        public Task RoverLandRequestAsync()
        {
            return _mediator.Publish(new RoverLandRequest(Context.ConnectionId));
        }
    }

    public class RoverLandRequest : INotification
    {
        public string ConnectionId { get; }
        public RoverLandRequest(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }
    public class RoverLandRequestNotificationHandler : INotificationHandler<RoverLandRequest>
    {
        private readonly IRoverService _roverService;
        private readonly IMediator _mediator;
        private readonly ILogger<RoverLandRequestNotificationHandler> _logger;

        public RoverLandRequestNotificationHandler(
            IRoverService roverService,
            IMediator mediator,
            ILogger<RoverLandRequestNotificationHandler> logger)
        {
            _roverService = roverService;
            _mediator = mediator;
            _logger = logger;
        }

        public Task Handle(RoverLandRequest notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("RoverLandRequestNotificationHandler - {notificationConnectionId}", notification.ConnectionId);
            return _roverService.LandAsync().ContinueWith(x =>
            {
                _logger.LogInformation("LandAsync - "+  x.Result.Name);
                _mediator.Publish(new RoverLandResponse(notification.ConnectionId, x.Result), cancellationToken);
            }, cancellationToken);
        }
    }
    public class RoverLandResponseNotificationHandler : INotificationHandler<RoverLandResponse>
    {
        private readonly IHubContext<NotificationHub, IPushNotificationHubClient> _hubContext;
        private readonly ILogger<RoverLandResponseNotificationHandler> _logger;

        public RoverLandResponseNotificationHandler(
            IHubContext<NotificationHub, IPushNotificationHubClient> hubContext,
            ILogger<RoverLandResponseNotificationHandler> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task Handle(RoverLandResponse notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("RoverLandResponseNotificationHandler - {notificationConnectionId}", notification.ConnectionId);
            return _hubContext.Clients.Clients(notification.ConnectionId).RoverLandResponseAsync(notification.State.Name);
        }
    }
    public class RoverLandResponse : INotification
    {
        public string ConnectionId { get; }
        public RoverState State { get; }

        public RoverLandResponse(string connectionId, RoverState state)
        {
            ConnectionId = connectionId;
            State = state;
        }
    }
    public class PingNotificationHandler : INotificationHandler<PingNotification>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PingNotificationHandler> _logger;

        public PingNotificationHandler(
            IMediator mediator,
            ILogger<PingNotificationHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public Task Handle(PingNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(nameof(PingNotificationHandler));
            _logger.LogInformation(nameof(Handle));
            _logger.LogInformation(JsonSerializer.Serialize(notification));
            return _mediator.Publish(new PongNotification(notification.ConnectionId, notification.Data), cancellationToken);
        }
    }
    public class PongNotificationHandler : INotificationHandler<PongNotification>
    {
        private readonly IHubContext<NotificationHub, IPushNotificationHubClient> _hubContext;
        private readonly ILogger<PongNotificationHandler> _logger;

        public PongNotificationHandler(
            IHubContext<NotificationHub, IPushNotificationHubClient> hubContext,
            ILogger<PongNotificationHandler> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task Handle(PongNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(nameof(PongNotificationHandler));
            _logger.LogInformation(nameof(Handle));
            _logger.LogInformation(JsonSerializer.Serialize(notification));
            _logger.LogInformation("sending typed client {notificationConnectionId}", notification.ConnectionId);
            return _hubContext.Clients.Clients(notification.ConnectionId).SendPongNotificationAsync(notification);
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
}
