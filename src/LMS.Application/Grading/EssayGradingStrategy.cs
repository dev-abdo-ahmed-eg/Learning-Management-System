using LMS.Domain;
using LMS.Domain.Entities;
using LMS.Domain.Interfaces;

namespace LMS.Application.Grading;

public class EssayGradingStrategy : IGradingStrategy
{
    private const int MinimumWordCount = 50;

    public GradeResult Grade(Submission submission, Assignment assignment)
    {
        var wordCount = submission.Answer
            .Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Length;

        if (wordCount < MinimumWordCount)
        {
            return new GradeResult(
                0,
                $"Essay too short. {wordCount} words submitted; minimum is {MinimumWordCount}.");
        }

        var bonusKeywords = assignment.AnswerKey
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var keywordsFound = bonusKeywords.Count(keyword =>
            submission.Answer.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        var baseScore = assignment.MaxScore * 0.6;
        var bonusPerKeyword = bonusKeywords.Length > 0
            ? assignment.MaxScore * 0.4 / bonusKeywords.Length
            : 0;
        var score = Math.Min(assignment.MaxScore, baseScore + keywordsFound * bonusPerKeyword);

        var feedback = bonusKeywords.Length > 0
            ? $"Essay meets length requirement. Used {keywordsFound} of {bonusKeywords.Length} key concepts."
            : "Essay meets length requirement.";

        return new GradeResult(score, feedback);
    }
}
