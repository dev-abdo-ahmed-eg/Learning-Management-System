using LMS.Application.Events;
using LMS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace LMS.Application.Subscribers;

public class ProgressSubscriber : ISubscriber<AssignmentGradedEvent>
{
    private readonly Dictionary<Guid, List<Guid>> _completedAssignments = new();
    private readonly ILogger<ProgressSubscriber> _logger;

    public ProgressSubscriber(ILogger<ProgressSubscriber> logger)
    {
        _logger = logger;
    }

    public void Handle(AssignmentGradedEvent @event)
    {
        if (!_completedAssignments.TryGetValue(@event.StudentId, out var assignments))
        {
            assignments = new List<Guid>();
            _completedAssignments[@event.StudentId] = assignments;
        }

        assignments.Add(@event.AssignmentId);

        _logger.LogInformation(
            "Progress updated for student {StudentId}: {CompletedCount} assignment(s) completed",
            @event.StudentId,
            assignments.Count);
    }
}
