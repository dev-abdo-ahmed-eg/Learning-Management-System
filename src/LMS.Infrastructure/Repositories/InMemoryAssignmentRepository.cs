using LMS.Domain.Entities;
using LMS.Domain.Interfaces;

namespace LMS.Infrastructure.Repositories;

public class InMemoryAssignmentRepository : IAssignmentRepository
{
    private readonly Dictionary<Guid, Assignment> _assignments = new();

    public Assignment? GetById(Guid id) =>
        _assignments.GetValueOrDefault(id);

    public void Add(Assignment assignment) =>
        _assignments[assignment.Id] = assignment;

    public IReadOnlyList<Assignment> GetAll() =>
        _assignments.Values.ToList();
}
