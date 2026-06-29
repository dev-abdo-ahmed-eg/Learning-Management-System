using LMS.Domain;
using LMS.Domain.Entities;
using LMS.Domain.Interfaces;

namespace LMS.Application.Grading;

public class QuizGradingStrategy : IGradingStrategy
{
    public GradeResult Grade(Submission submission, Assignment assignment)
    {
        var isCorrect = string.Equals(
            submission.Answer.Trim(),
            assignment.AnswerKey.Trim(),
            StringComparison.OrdinalIgnoreCase);

        var score = isCorrect ? assignment.MaxScore : 0;
        var feedback = isCorrect
            ? "Correct answer."
            : "Incorrect answer.";

        return new GradeResult(score, feedback);
    }
}
