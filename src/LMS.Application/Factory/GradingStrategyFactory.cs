using LMS.Application.Grading;
using LMS.Domain.Enums;
using LMS.Domain.Interfaces;

namespace LMS.Application.Factory;

public class GradingStrategyFactory : IGradingStrategyFactory
{
    public IGradingStrategy Create(AssignmentType type) => type switch
    {
        AssignmentType.Quiz => new QuizGradingStrategy(),
        AssignmentType.ProgrammingAssignment => new ProgrammingGradingStrategy(),
        AssignmentType.Essay => new EssayGradingStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
