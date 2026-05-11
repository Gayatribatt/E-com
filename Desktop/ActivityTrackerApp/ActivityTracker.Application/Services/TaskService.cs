using ActivityTracker.Application.DTOs;
using ActivityTracker.Application.Interfaces;
using ActivityTracker.Domain.Models;

namespace ActivityTracker.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICurrentUserService _currentUserService;

    public TaskService(ITaskRepository taskRepository, ICurrentUserService currentUserService)
    {
        _taskRepository = taskRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyCollection<TaskResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _taskRepository.GetAllAsync(cancellationToken);
        return items.Select(Map).ToArray();
    }

    public async Task<TaskResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _taskRepository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<TaskResponseDto> CreateAsync(UpsertTaskDto request, CancellationToken cancellationToken = default)
    {
        var item = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = request.IsCompleted,
            CreatedByUserId = _currentUserService.UserId ?? 0
        };
        await _taskRepository.AddAsync(item, cancellationToken);
        return Map(item);
    }

    public async Task<TaskResponseDto> UpdateAsync(int id, UpsertTaskDto request, CancellationToken cancellationToken = default)
    {
        var item = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");

        item.Title = request.Title;
        item.Description = request.Description;
        item.IsCompleted = request.IsCompleted;
        await _taskRepository.UpdateAsync(item, cancellationToken);
        return Map(item);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _taskRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");
        await _taskRepository.SoftDeleteAsync(item, cancellationToken);
    }

    private static TaskResponseDto Map(TaskItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        IsCompleted = item.IsCompleted,
        CreatedByUserId = item.CreatedByUserId
    };
}
