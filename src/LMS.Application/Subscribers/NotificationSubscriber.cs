using LMS.Application.Events;
using LMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LMS.Application.Subscribers;

public class NotificationSubscriber : ISubscriber<AssignmentGradedEvent>
{
    private readonly ILogger<NotificationSubscriber> _logger;

    public NotificationSubscriber(ILogger<NotificationSubscriber> logger)
    {
        _logger = logger;
    }

    public void Handle(AssignmentGradedEvent @event)
    {
        _logger.LogInformation(
            "Notification sent to student {StudentId}: score {Score} for submission {SubmissionId}",
            @event.StudentId,
            @event.Score,
            @event.SubmissionId);
    }
}
