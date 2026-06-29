using LMS.Application.Events;
using LMS.Domain;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using LMS.Domain.Interfaces;

namespace LMS.Application.Services;

public class GradingService
{
    private readonly IGradingStrategyFactory _strategyFactory;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IEventBus _eventBus;

    public GradingService(
        IGradingStrategyFactory strategyFactory,
        IAssignmentRepository assignmentRepository,
        ISubmissionRepository submissionRepository,
        IEventBus eventBus)
    {
        _strategyFactory = strategyFactory;
        _assignmentRepository = assignmentRepository;
        _submissionRepository = submissionRepository;
        _eventBus = eventBus;
    }

    public GradeResult GradeSubmission(Guid submissionId)
    {
        var submission = _submissionRepository.GetById(submissionId)
            ?? throw new InvalidOperationException($"Submission {submissionId} not found.");

        var assignment = _assignmentRepository.GetById(submission.AssignmentId)
            ?? throw new InvalidOperationException($"Assignment {submission.AssignmentId} not found.");

        var result = Grade(submission, assignment);

        submission.Status = SubmissionStatus.Graded;
        submission.Grade = new Grade
        {
            Id = Guid.NewGuid(),
            SubmissionId = submission.Id,
            Score = result.Score,
            Feedback = result.Feedback,
            GradedAt = DateTime.UtcNow
        };
        _submissionRepository.Update(submission);

        _eventBus.Publish(new AssignmentGradedEvent(
            submission.Id,
            submission.StudentId,
            submission.AssignmentId,
            result.Score,
            result.Feedback));

        return result;
    }

    public GradeResult Grade(Submission submission, Assignment assignment)
    {
        var strategy = _strategyFactory.Create(assignment.AssignmentType);
        return strategy.Grade(submission, assignment);
    }
}
