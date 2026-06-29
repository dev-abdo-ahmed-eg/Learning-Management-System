using LMS.Domain;
using LMS.Domain.Entities;
using LMS.Domain.Interfaces;

namespace LMS.Application.Services;

public class GradingService
{
    private readonly IGradingStrategyFactory _strategyFactory;

    public GradingService(IGradingStrategyFactory strategyFactory)
    {
        _strategyFactory = strategyFactory;
    }

    public GradeResult Grade(Submission submission, Assignment assignment)
    {
        var strategy = _strategyFactory.Create(assignment.AssignmentType);
        return strategy.Grade(submission, assignment);
    }
}
