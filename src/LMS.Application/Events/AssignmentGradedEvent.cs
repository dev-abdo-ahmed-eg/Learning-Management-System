namespace LMS.Application.Events;

public record AssignmentGradedEvent(
    Guid SubmissionId,
    Guid StudentId,
    Guid AssignmentId,
    double Score,
    string Feedback);
