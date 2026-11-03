using Confluent.Kafka;
using EmployeeServiceDepartmentService.data_access;
using EmployeeServiceDepartmentService.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EmployeeServiceDepartmentService.Services
{
    public class NotificationMessage
    {
        public string Action { get; set; }
        public Guid LeaveId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? ManagerId { get; set; }
        public string Message { get; set; }
    }

    public class NotificationConsumerService : BackgroundService
    {
        private readonly ILogger<NotificationConsumerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConsumerConfig _consumerConfig;

        public NotificationConsumerService(ILogger<NotificationConsumerService> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "leave-notification-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield(); // free the caller

            using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
            consumer.Subscribe("leave-events");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);
                        if (consumeResult != null)
                        {
                            var messageJson = consumeResult.Message.Value;
                            _logger.LogInformation($"Received Kafka message: {messageJson}");
                            
                            var msgObj = JsonSerializer.Deserialize<NotificationMessage>(messageJson);
                            if (msgObj != null)
                            {
                                await ProcessMessageAsync(msgObj, stoppingToken);
                            }
                        }
                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogError($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }

        private async Task ProcessMessageAsync(NotificationMessage message, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmpDepartmentDbContext>();

            if (message.Action == "LeaveApplied")
            {
                // Create a notification for the Manager
                if (message.ManagerId.HasValue && message.ManagerId.Value != Guid.Empty)
                {
                    var notification = new Notification
                    {
                        EmployeeId = message.ManagerId.Value,
                        Message = message.Message,
                        LeaveRequestId = message.LeaveId
                    };
                    dbContext.Notifications.Add(notification);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation($"Notification created for Manager: {message.ManagerId}");
                }
            }
            else if (message.Action == "LeaveApproved" || message.Action == "LeaveRejected")
            {
                // Delete the notification from the manager's dashboard for this leave request
                var existingNotifications = await dbContext.Notifications
                    .Where(n => n.LeaveRequestId == message.LeaveId)
                    .ToListAsync(stoppingToken);
                
                if (existingNotifications.Any())
                {
                    dbContext.Notifications.RemoveRange(existingNotifications);
                }

                // Create a notification for the Employee
                var newNotification = new Notification
                {
                    EmployeeId = message.EmployeeId,
                    Message = message.Message,
                    LeaveRequestId = message.LeaveId
                };
                dbContext.Notifications.Add(newNotification);

                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation($"Notification updated. Manager notification deleted, Employee notification created.");
            }
        }
    }
}
