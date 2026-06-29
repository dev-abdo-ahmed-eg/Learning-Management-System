using LMS.Domain.Entities;

namespace LMS.Domain.Interfaces;

public interface ISubmissionRepository
{
    Submission? GetById(Guid id);
    void Add(Submission submission);
    void Update(Submission submission);
    IReadOnlyList<Submission> GetByAssignment(Guid assignmentId);
}
