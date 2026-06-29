using LMS.Domain.Enums;

namespace LMS.Domain.Entities;

public class Submission
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid AssignmentId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public Grade? Grade { get; set; }
}
