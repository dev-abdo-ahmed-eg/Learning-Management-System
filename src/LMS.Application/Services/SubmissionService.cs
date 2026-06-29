using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Domain.Interfaces;

namespace LMS.Application.Services;

public class SubmissionService
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IAssignmentRepository _assignmentRepository;

    public SubmissionService(
        ISubmissionRepository submissionRepository,
        IAssignmentRepository assignmentRepository)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
    }

    public Submission? GetById(Guid id) =>
        _submissionRepository.GetById(id);

    public Submission? Submit(Guid studentId, Guid assignmentId, string answer)
    {
        if (_assignmentRepository.GetById(assignmentId) is null)
        {
            return null;
        }

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            AssignmentId = assignmentId,
            Answer = answer,
            Status = SubmissionStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        _submissionRepository.Add(submission);
        return submission;
    }
}
