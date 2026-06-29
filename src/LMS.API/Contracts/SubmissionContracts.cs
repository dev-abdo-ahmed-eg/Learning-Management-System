using LMS.Domain.Enums;

namespace LMS.API.Contracts;

public record CreateSubmissionRequest(
    Guid StudentId,
    Guid AssignmentId,
    string Answer);

public record GradeResponse(
    Guid Id,
    double Score,
    string Feedback,
    DateTime GradedAt);

public record SubmissionResponse(
    Guid Id,
    Guid StudentId,
    Guid AssignmentId,
    string Answer,
    SubmissionStatus Status,
    DateTime SubmittedAt,
    GradeResponse? Grade);

public record GradeSubmissionResponse(
    double Score,
    string Feedback,
    SubmissionResponse Submission);
