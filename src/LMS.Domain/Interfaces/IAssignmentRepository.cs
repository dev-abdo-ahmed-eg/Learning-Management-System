using LMS.Domain.Entities;

namespace LMS.Domain.Interfaces;

public interface IAssignmentRepository
{
    Assignment? GetById(Guid id);
    void Add(Assignment assignment);
    IReadOnlyList<Assignment> GetAll();
}
