using LMS.API.Contracts;
using LMS.Application.Services;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

/// <summary>
/// Manages student submissions and grading in the submit-and-grade workflow.
/// </summary>
[ApiController]
[Route("submissions")]
public class SubmissionsController : ControllerBase
{
    private readonly SubmissionService _submissionService;
    private readonly GradingService _gradingService;

    public SubmissionsController(
        SubmissionService submissionService,
        GradingService gradingService)
    {
        _submissionService = submissionService;
        _gradingService = gradingService;
    }

    /// <summary>
    /// Submits a student answer for an existing assignment.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubmissionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] CreateSubmissionRequest request)
    {
        if (request.StudentId == Guid.Empty)
        {
            return Problem(
                title: "Invalid submission",
                detail: "StudentId is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (request.AssignmentId == Guid.Empty)
        {
            return Problem(
                title: "Invalid submission",
                detail: "AssignmentId is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.Answer))
        {
            return Problem(
                title: "Invalid submission",
                detail: "Answer is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var submission = _submissionService.Submit(
            request.StudentId,
            request.AssignmentId,
            request.Answer.Trim());

        if (submission is null)
        {
            return Problem(
                title: "Assignment not found",
                detail: $"No assignment with id {request.AssignmentId} exists.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = submission.Id },
            ToResponse(submission));
    }

    /// <summary>
    /// Retrieves a submission with its grade after grading completes.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var submission = _submissionService.GetById(id);
        if (submission is null)
        {
            return Problem(
                title: "Submission not found",
                detail: $"No submission with id {id} exists.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(ToResponse(submission));
    }

    /// <summary>
    /// Grades a submitted assignment by executing the complete grading workflow.
    ///
    /// This endpoint demonstrates the three design patterns implemented in the project:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <b>Factory:</b> Resolves the appropriate grading strategy based on the assignment type.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <b>Strategy:</b> Executes the grading algorithm specific to the resolved assignment type (e.g., Quiz, Programming, or Essay).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <b>Observer:</b> Publishes an <c>AssignmentGraded</c> event after grading, allowing independent subscribers
    /// (such as notification, progress tracking, audit logging, and analytics) to react without coupling them to the grading workflow.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Returns the calculated grade, feedback, and the updated submission details.
    /// </summary>
    [HttpPost("{id:guid}/grade")]
    [ProducesResponseType(typeof(GradeSubmissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Grade(Guid id)
    {
        var submission = _submissionService.GetById(id);
        if (submission is null)
        {
            return Problem(
                title: "Submission not found",
                detail: $"No submission with id {id} exists.",
                statusCode: StatusCodes.Status404NotFound);
        }

        if (submission.Status == SubmissionStatus.Graded)
        {
            return Problem(
                title: "Submission already graded",
                detail: $"Submission {id} has already been graded.",
                statusCode: StatusCodes.Status422UnprocessableEntity);
        }

        var result = _gradingService.GradeSubmission(id);
        var gradedSubmission = _submissionService.GetById(id)!;

        return Ok(new GradeSubmissionResponse(
            result.Score,
            result.Feedback,
            ToResponse(gradedSubmission)));
    }

    private static SubmissionResponse ToResponse(Domain.Entities.Submission submission) =>
        new(
            submission.Id,
            submission.StudentId,
            submission.AssignmentId,
            submission.Answer,
            submission.Status,
            submission.SubmittedAt,
            submission.Grade is null
                ? null
                : new GradeResponse(
                    submission.Grade.Id,
                    submission.Grade.Score,
                    submission.Grade.Feedback,
                    submission.Grade.GradedAt));
}
