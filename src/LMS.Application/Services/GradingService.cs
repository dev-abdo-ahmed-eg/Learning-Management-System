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

    /// <summary>
    /// Grades a submission and completes the end-to-end grading workflow.
    ///
    /// The workflow consists of the following steps:
    /// <list type="number">
    /// <item>
    /// <description>Retrieve the submission and its associated assignment.</description>
    /// </item>
    /// <item>
    /// <description>
    /// Grade the submission by resolving and executing the appropriate grading strategy
    /// (Factory + Strategy patterns).
    /// </description>
    /// </item>
    /// <item>
    /// <description>Persist the calculated grade and mark the submission as graded.</description>
    /// </item>
    /// <item>
    /// <description>
    /// Publish an <see cref="AssignmentGradedEvent"/> to notify interested subscribers
    /// (Observer pattern), allowing independent services such as notifications,
    /// progress tracking, audit logging, and analytics to react without coupling
    /// them to the grading process.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// This method orchestrates the grading workflow while delegating grading behavior
    /// and post-processing responsibilities to their respective components.
    /// </summary>
    /// <param name="submissionId">The unique identifier of the submission to grade.</param>
    /// <returns>
    /// A <see cref="GradeResult"/> containing the calculated score and feedback.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the submission or its associated assignment cannot be found.
    /// </exception>
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

    /// <summary>
    /// Grades a submission using the grading algorithm associated with the assignment type.
    ///
    /// This service coordinates two design patterns:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <b>Factory:</b> Selects and returns the appropriate <see cref="IGradingStrategy"/>
    /// implementation based on the assignment's type.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <b>Strategy:</b> Executes the grading algorithm encapsulated by the selected strategy,
    /// allowing each assignment type to define its own grading behavior.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// The service is responsible for orchestrating the grading process and delegates the
    /// actual grading logic to the resolved strategy.
    /// </summary>
    /// <param name="submission">The student's submission to be graded.</param>
    /// <param name="assignment">The assignment associated with the submission.</param>
    /// <returns>
    /// A <see cref="GradeResult"/> containing the calculated score and feedback.
    /// </returns>
    public GradeResult Grade(Submission submission, Assignment assignment)
    {
        var strategy = _strategyFactory.Create(assignment.AssignmentType);
        return strategy.Grade(submission, assignment);
    }
}
