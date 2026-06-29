using LMS.Domain.Enums;

namespace LMS.Domain.Entities;

public class Assignment
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public AssignmentType AssignmentType { get; set; }
    public double MaxScore { get; set; }
}
