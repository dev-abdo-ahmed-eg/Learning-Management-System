using LMS.Application.Events;
using LMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LMS.Application.Subscribers;

public class AuditSubscriber : ISubscriber<AssignmentGradedEvent>
{
    private readonly ILogger<AuditSubscriber> _logger;

    public AuditSubscriber(ILogger<AuditSubscriber> logger)
    {
        _logger = logger;
    }

    public void Handle(AssignmentGradedEvent @event)
    {
        _logger.LogInformation(
            "Audit: submission {SubmissionId} graded with score {Score} — {Feedback}",
            @event.SubmissionId,
            @event.Score,
            @event.Feedback);
    }
}
