using LMS.Domain.Enums;

namespace LMS.API.Contracts;

public record CreateAssignmentRequest(
    string Title,
    AssignmentType AssignmentType,
    double MaxScore,
    string AnswerKey);

public record AssignmentResponse(
    Guid Id,
    string Title,
    AssignmentType AssignmentType,
    double MaxScore,
    string AnswerKey);
