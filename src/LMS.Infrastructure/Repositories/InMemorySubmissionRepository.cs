using LMS.Domain.Entities;
using LMS.Domain.Interfaces;

namespace LMS.Infrastructure.Repositories;

public class InMemorySubmissionRepository : ISubmissionRepository
{
    private readonly Dictionary<Guid, Submission> _submissions = new();

    public Submission? GetById(Guid id) =>
        _submissions.GetValueOrDefault(id);

    public void Add(Submission submission) =>
        _submissions[submission.Id] = submission;

    public void Update(Submission submission) =>
        _submissions[submission.Id] = submission;

    public IReadOnlyList<Submission> GetByAssignment(Guid assignmentId) =>
        _submissions.Values
            .Where(s => s.AssignmentId == assignmentId)
            .ToList();
}
