using LMS.Domain;
using LMS.Domain.Entities;
using LMS.Domain.Interfaces;

namespace LMS.Application.Grading;

public class ProgrammingGradingStrategy : IGradingStrategy
{
    public GradeResult Grade(Submission submission, Assignment assignment)
    {
        var requiredKeywords = assignment.AnswerKey
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (requiredKeywords.Length == 0)
        {
            return new GradeResult(0, "No test cases configured for this assignment.");
        }

        var answer = submission.Answer;
        var foundCount = requiredKeywords.Count(keyword =>
            answer.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        var score = assignment.MaxScore * foundCount / requiredKeywords.Length;
        var feedback = $"Passed {foundCount} of {requiredKeywords.Length} test cases.";

        return new GradeResult(score, feedback);
    }
}
