using LMS.API.Contracts;
using LMS.Domain.Entities;
using LMS.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

/// <summary>
/// Manages assignments in the create-assignment step of the grading workflow.
/// </summary>
[ApiController]
[Route("assignments")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentRepository _assignmentRepository;

    public AssignmentsController(IAssignmentRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    /// <summary>
    /// Creates an assignment with a type and answer key for the grading workflow.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateAssignmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Problem(
                title: "Invalid assignment",
                detail: "Title is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (request.MaxScore <= 0)
        {
            return Problem(
                title: "Invalid assignment",
                detail: "MaxScore must be greater than zero.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            AssignmentType = request.AssignmentType,
            MaxScore = request.MaxScore,
            AnswerKey = request.AnswerKey ?? string.Empty
        };

        _assignmentRepository.Add(assignment);

        return CreatedAtAction(
            nameof(GetById),
            new { id = assignment.Id },
            ToResponse(assignment));
    }

    /// <summary>
    /// Retrieves an assignment by id before a student submits an answer.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var assignment = _assignmentRepository.GetById(id);
        if (assignment is null)
        {
            return Problem(
                title: "Assignment not found",
                detail: $"No assignment with id {id} exists.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(ToResponse(assignment));
    }

    private static AssignmentResponse ToResponse(Assignment assignment) =>
        new(
            assignment.Id,
            assignment.Title,
            assignment.AssignmentType,
            assignment.MaxScore,
            assignment.AnswerKey);
}
