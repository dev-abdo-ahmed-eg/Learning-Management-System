using LMS.Domain.Enums;

namespace LMS.Domain.Interfaces;

public interface IGradingStrategyFactory
{
    IGradingStrategy Create(AssignmentType type);
}
