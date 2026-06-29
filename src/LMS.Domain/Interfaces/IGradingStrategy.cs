using LMS.Domain.Entities;

namespace LMS.Domain.Interfaces;

public interface IGradingStrategy
{
    GradeResult Grade(Submission submission, Assignment assignment);
}
