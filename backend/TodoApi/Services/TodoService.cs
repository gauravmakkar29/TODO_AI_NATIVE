using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTOs;

namespace TodoApi.Services;

public class TodoService : ITodoService
{
    private readonly ApplicationDbContext _context;

    public TodoService(ApplicationDbContext context)
    {
        _context = context;
    }

    private bool SupportsTransactions()
    {
        // In-memory database doesn't support transactions
        // Check if the database provider is in-memory
        var providerName = _context.Database.ProviderName;
        return providerName != null && !providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<TodoDto>> GetTodosByUserIdAsync(int userId, string? sortBy = null, int? priorityFilter = null)
    {
        IQueryable<Todo> query = _context.Todos
            .Where(t => t.UserId == userId && !t.IsArchived) // Exclude archived by default
            .Include(t => t.TodoCategories)
                .ThenInclude(tc => tc.Category)
            .Include(t => t.TodoTags)
                .ThenInclude(tt => tt.Tag);

        // Apply priority filter if provided
        if (priorityFilter.HasValue)
        {
            query = query.Where(t => t.Priority == priorityFilter.Value);
        }

        // Apply sorting
        IOrderedQueryable<Todo> orderedQuery = sortBy?.ToLower() switch
        {
            "priority" => query.OrderBy(t => t.DisplayOrder).ThenByDescending(t => t.Priority).ThenByDescending(t => t.CreatedAt),
            "priority_asc" => query.OrderBy(t => t.DisplayOrder).ThenBy(t => t.Priority).ThenByDescending(t => t.CreatedAt),
            "duedate" => query.OrderBy(t => t.DisplayOrder).ThenBy(t => t.DueDate.HasValue).ThenBy(t => t.DueDate ?? DateTime.MaxValue).ThenByDescending(t => t.CreatedAt),
            "duedate_desc" => query.OrderBy(t => t.DisplayOrder).ThenByDescending(t => t.DueDate.HasValue).ThenByDescending(t => t.DueDate ?? DateTime.MinValue).ThenByDescending(t => t.CreatedAt),
            _ => query.OrderBy(t => t.DisplayOrder).ThenByDescending(t => t.CreatedAt)
        };

        var todos = await orderedQuery.ToListAsync();
        return todos.Select(t => MapToDto(t));
    }

    public async Task<TodoDto?> GetTodoByIdAsync(int todoId, int userId)
    {
        var todo = await _context.Todos
            .Include(t => t.TodoCategories)
                .ThenInclude(tc => tc.Category)
            .Include(t => t.TodoTags)
                .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);

        if (todo == null)
            return null;

        return MapToDto(todo);
    }

    public async Task<TodoDto> CreateTodoAsync(CreateTodoRequest request, int userId)
    {
        // Use transaction to ensure data integrity (only if supported)
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
        if (SupportsTransactions())
        {
            transaction = await _context.Database.BeginTransactionAsync();
        }

        try
        {
            var todo = new Todo
            {
                UserId = userId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                ReminderDate = request.ReminderDate,
                Priority = request.Priority,
                CreatedAt = DateTime.UtcNow
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            // Assign categories
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                var validCategoryIds = await _context.Categories
                    .Where(c => request.CategoryIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var categoryId in validCategoryIds)
                {
                    todo.TodoCategories.Add(new TodoCategory
                    {
                        TodoId = todo.Id,
                        CategoryId = categoryId
                    });
                }
            }

            // Assign tags
            if (request.TagIds != null && request.TagIds.Any())
            {
                var validTagIds = await _context.Tags
                    .Where(t => request.TagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                foreach (var tagId in validTagIds)
                {
                    todo.TodoTags.Add(new TodoTag
                    {
                        TodoId = todo.Id,
                        TagId = tagId
                    });
                }
            }

            await _context.SaveChangesAsync();
            
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            // Reload with relationships
            await _context.Entry(todo)
                .Collection(t => t.TodoCategories)
                .Query()
                .Include(tc => tc.Category)
                .LoadAsync();

            await _context.Entry(todo)
                .Collection(t => t.TodoTags)
                .Query()
                .Include(tt => tt.Tag)
                .LoadAsync();

            return MapToDto(todo);
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
            throw;
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public async Task<TodoDto?> UpdateTodoAsync(int todoId, UpdateTodoRequest request, int userId)
    {
        // Use transaction to ensure data integrity (only if supported)
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
        if (SupportsTransactions())
        {
            transaction = await _context.Database.BeginTransactionAsync();
        }

        try
        {
            var todo = await _context.Todos
                .Include(t => t.TodoCategories)
                .Include(t => t.TodoTags)
                .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);

            if (todo == null)
                return null;

            if (!string.IsNullOrWhiteSpace(request.Title))
                todo.Title = request.Title;

            if (request.Description != null)
                todo.Description = request.Description;

            if (request.IsCompleted.HasValue)
            {
                var wasCompleted = todo.IsCompleted;
                todo.IsCompleted = request.IsCompleted.Value;
                
                // Update status and completion date
                if (request.IsCompleted.Value && !wasCompleted)
                {
                    todo.Status = Models.TodoStatus.Completed;
                    todo.CompletedAt = DateTime.UtcNow;
                }
                else if (!request.IsCompleted.Value && wasCompleted)
                {
                    todo.Status = Models.TodoStatus.Pending;
                    todo.CompletedAt = null;
                }
            }

            if (request.DueDate.HasValue)
                todo.DueDate = request.DueDate;

            if (request.ReminderDate.HasValue)
                todo.ReminderDate = request.ReminderDate;

            if (request.Priority.HasValue)
                todo.Priority = request.Priority.Value;

            // Update categories if provided
            if (request.CategoryIds != null)
            {
                // Remove existing categories
                _context.TodoCategories.RemoveRange(todo.TodoCategories);

                // Add new categories
                if (request.CategoryIds.Any())
                {
                    var validCategoryIds = await _context.Categories
                        .Where(c => request.CategoryIds.Contains(c.Id))
                        .Select(c => c.Id)
                        .ToListAsync();

                    foreach (var categoryId in validCategoryIds)
                    {
                        todo.TodoCategories.Add(new TodoCategory
                        {
                            TodoId = todo.Id,
                            CategoryId = categoryId
                        });
                    }
                }
            }

            // Update tags if provided
            if (request.TagIds != null)
            {
                // Remove existing tags
                _context.TodoTags.RemoveRange(todo.TodoTags);

                // Add new tags
                if (request.TagIds.Any())
                {
                    var validTagIds = await _context.Tags
                        .Where(t => request.TagIds.Contains(t.Id))
                        .Select(t => t.Id)
                        .ToListAsync();

                    foreach (var tagId in validTagIds)
                    {
                        todo.TodoTags.Add(new TodoTag
                        {
                            TodoId = todo.Id,
                            TagId = tagId
                        });
                    }
                }
            }

            todo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            // Reload with relationships
            await _context.Entry(todo)
                .Collection(t => t.TodoCategories)
                .Query()
                .Include(tc => tc.Category)
                .LoadAsync();

            await _context.Entry(todo)
                .Collection(t => t.TodoTags)
                .Query()
                .Include(tt => tt.Tag)
                .LoadAsync();

            return MapToDto(todo);
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
            throw;
        }
        finally
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public async Task<bool> DeleteTodoAsync(int todoId, int userId)
    {
        var todo = await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId);

        if (todo == null)
            return false;

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<TodoDto>> GetTodosByCategoryAsync(int userId, int categoryId)
    {
        var todos = await _context.Todos
            .Where(t => t.UserId == userId && t.TodoCategories.Any(tc => tc.CategoryId == categoryId))
            .Include(t => t.TodoCategories)
                .ThenInclude(tc => tc.Category)
            .Include(t => t.TodoTags)
                .ThenInclude(tt => tt.Tag)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return todos.Select(t => MapToDto(t));
    }

    public async Task<IEnumerable<TodoDto>> GetTodosByTagAsync(int userId, int tagId)
    {
        var todos = await _context.Todos
            .Where(t => t.UserId == userId && t.TodoTags.Any(tt => tt.TagId == tagId))
            .Include(t => t.TodoCategories)
                .ThenInclude(tc => tc.Category)
            .Include(t => t.TodoTags)
                .ThenInclude(tt => tt.Tag)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return todos.Select(t => MapToDto(t));
    }

    public async Task<SearchFilterResponse> SearchAndFilterTodosAsync(int userId, SearchFilterRequest request)
    {
        var query = _context.Todos
            .Where(t => t.UserId == userId)
            .Include(t => t.TodoCategories)
                .ThenInclude(tc => tc.Category)
            .Include(t => t.TodoTags)
                .ThenInclude(tt => tt.Tag)
            .AsQueryable();

        // Text search - search in title, description, category names, and tag names
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var searchTerm = request.SearchQuery.ToLower().Trim();
            query = query.Where(t =>
                t.Title.ToLower().Contains(searchTerm) ||
                (t.Description != null && t.Description.ToLower().Contains(searchTerm)) ||
                t.TodoCategories.Any(tc => tc.Category.Name.ToLower().Contains(searchTerm)) ||
                t.TodoTags.Any(tt => tt.Tag.Name.ToLower().Contains(searchTerm))
            );
        }

        // Overdue filter (must be checked before IsCompleted filter)
        if (request.IsOverdue == true)
        {
            var now = DateTime.UtcNow.Date;
            query = query.Where(t => !t.IsCompleted && t.DueDate != null && t.DueDate.Value.Date < now);
        }

        // Archive filter
        if (request.IsArchived.HasValue)
        {
            query = query.Where(t => t.IsArchived == request.IsArchived.Value);
        }
        else if (request.HideCompleted != true)
        {
            // By default, exclude archived unless explicitly requested
            query = query.Where(t => !t.IsArchived);
        }

        // Status filter
        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }
        else if (request.IsCompleted.HasValue)
        {
            if (request.IsCompleted.Value)
            {
                // Completed
                query = query.Where(t => t.IsCompleted && t.Status == Models.TodoStatus.Completed);
            }
            else if (request.IsOverdue != true) // Only filter pending if not filtering for overdue
            {
                // Pending (not completed and not overdue)
                var now = DateTime.UtcNow.Date;
                query = query.Where(t => !t.IsCompleted && t.Status == Models.TodoStatus.Pending && (t.DueDate == null || t.DueDate.Value.Date >= now));
            }
        }

        // Hide completed filter
        if (request.HideCompleted == true)
        {
            query = query.Where(t => !t.IsCompleted || t.Status != Models.TodoStatus.Completed);
        }

        // Priority filter
        if (request.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == request.Priority.Value);
        }

        // Category filter
        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            query = query.Where(t => t.TodoCategories.Any(tc => request.CategoryIds.Contains(tc.CategoryId)));
        }

        // Tag filter
        if (request.TagIds != null && request.TagIds.Any())
        {
            query = query.Where(t => t.TodoTags.Any(tt => request.TagIds.Contains(tt.TagId)));
        }

        // Due date range filter
        if (request.DueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate != null && t.DueDate >= request.DueDateFrom.Value);
        }

        if (request.DueDateTo.HasValue)
        {
            var dueDateTo = request.DueDateTo.Value.Date.AddDays(1).AddTicks(-1); // End of day
            query = query.Where(t => t.DueDate != null && t.DueDate <= dueDateTo);
        }

        // Created date range filter
        if (request.CreatedAtFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.CreatedAtFrom.Value);
        }

        if (request.CreatedAtTo.HasValue)
        {
            var createdAtTo = request.CreatedAtTo.Value.Date.AddDays(1).AddTicks(-1); // End of day
            query = query.Where(t => t.CreatedAt <= createdAtTo);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Sorting
        var sortBy = request.SortBy?.ToLower() ?? "createdAt";
        var sortOrder = request.SortOrder?.ToLower() ?? "desc";

        query = sortBy switch
        {
            "title" => sortOrder == "asc" ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title),
            "priority" => sortOrder == "asc" ? query.OrderBy(t => t.Priority) : query.OrderByDescending(t => t.Priority),
            "duedate" => sortOrder == "asc"
                ? query.OrderBy(t => t.DueDate ?? DateTime.MaxValue)
                : query.OrderByDescending(t => t.DueDate ?? DateTime.MinValue),
            "createdat" or _ => sortOrder == "asc"
                ? query.OrderBy(t => t.CreatedAt)
                : query.OrderByDescending(t => t.CreatedAt)
        };

        // Pagination
        var pageNumber = request.PageNumber ?? 1;
        var pageSize = request.PageSize ?? 50;
        var skip = (pageNumber - 1) * pageSize;

        var todos = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return new SearchFilterResponse
        {
            Todos = todos.Select(t => MapToDto(t)).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> ReorderTodosAsync(int userId, ReorderTodosRequest request)
    {
        if (request.TodoOrders == null || !request.TodoOrders.Any())
            return false;

        var todoIds = request.TodoOrders.Select(to => to.TodoId).ToList();
        var todos = await _context.Todos
            .Where(t => t.UserId == userId && todoIds.Contains(t.Id))
            .ToListAsync();

        if (todos.Count != request.TodoOrders.Count)
            return false; // Not all todos belong to the user

        foreach (var orderItem in request.TodoOrders)
        {
            var todo = todos.FirstOrDefault(t => t.Id == orderItem.TodoId);
            if (todo != null)
            {
                todo.DisplayOrder = orderItem.DisplayOrder;
                todo.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> BulkMarkCompleteAsync(int userId, BulkTodoRequest request)
    {
        var todos = await _context.Todos
            .Where(t => t.UserId == userId && request.TodoIds.Contains(t.Id))
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var todo in todos)
        {
            todo.IsCompleted = request.IsCompleted;
            if (request.IsCompleted)
            {
                todo.Status = Models.TodoStatus.Completed;
                todo.CompletedAt = now;
            }
            else
            {
                todo.Status = Models.TodoStatus.Pending;
                todo.CompletedAt = null;
            }
            todo.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        return todos.Count;
    }

    public async Task<TodoStatisticsDto> GetTodoStatisticsAsync(int userId)
    {
        var todos = await _context.Todos
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var totalTodos = todos.Count;
        var completedTodos = todos.Count(t => t.IsCompleted && t.Status == Models.TodoStatus.Completed);
        var pendingTodos = todos.Count(t => !t.IsCompleted && t.Status == Models.TodoStatus.Pending);
        var archivedTodos = todos.Count(t => t.IsArchived || t.Status == Models.TodoStatus.Archived);
        var overdueTodos = todos.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value < now);
        var highPriorityTodos = todos.Count(t => !t.IsCompleted && t.Priority == 2);
        var mediumPriorityTodos = todos.Count(t => !t.IsCompleted && t.Priority == 1);
        var lowPriorityTodos = todos.Count(t => !t.IsCompleted && t.Priority == 0);

        var completionRate = totalTodos > 0 ? (double)completedTodos / totalTodos * 100 : 0;

        // Completion by date (last 30 days)
        var completionByDate = todos
            .Where(t => t.CompletedAt.HasValue && t.CompletedAt.Value >= now.AddDays(-30))
            .GroupBy(t => t.CompletedAt!.Value.Date)
            .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count());

        return new TodoStatisticsDto
        {
            TotalTodos = totalTodos,
            CompletedTodos = completedTodos,
            PendingTodos = pendingTodos,
            ArchivedTodos = archivedTodos,
            CompletionRate = Math.Round(completionRate, 2),
            OverdueTodos = overdueTodos,
            HighPriorityTodos = highPriorityTodos,
            MediumPriorityTodos = mediumPriorityTodos,
            LowPriorityTodos = lowPriorityTodos,
            CompletionByDate = completionByDate
        };
    }

    public async Task<int> ArchiveOldCompletedTodosAsync(int userId, int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var todos = await _context.Todos
            .Where(t => t.UserId == userId 
                && t.IsCompleted 
                && t.Status == Models.TodoStatus.Completed
                && t.CompletedAt.HasValue
                && t.CompletedAt.Value < cutoffDate
                && !t.IsArchived)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var todo in todos)
        {
            todo.IsArchived = true;
            todo.Status = Models.TodoStatus.Archived;
            todo.ArchivedAt = now;
            todo.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        return todos.Count;
    }

    private TodoDto MapToDto(Todo todo)
    {
        var now = DateTime.UtcNow;
        var isOverdue = todo.DueDate.HasValue && !todo.IsCompleted && todo.DueDate.Value < now;
        var isApproachingDue = todo.DueDate.HasValue && !todo.IsCompleted && !isOverdue && 
                               todo.DueDate.Value <= now.AddDays(3) && todo.DueDate.Value >= now;

        return new TodoDto
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            Status = todo.Status,
            IsArchived = todo.IsArchived,
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt,
            CompletedAt = todo.CompletedAt,
            ArchivedAt = todo.ArchivedAt,
            DueDate = todo.DueDate,
            ReminderDate = todo.ReminderDate,
            Priority = todo.Priority,
            IsOverdue = isOverdue,
            IsApproachingDue = isApproachingDue,
            Categories = todo.TodoCategories
                .Select(tc => new CategoryDto
                {
                    Id = tc.Category.Id,
                    Name = tc.Category.Name,
                    Color = tc.Category.Color,
                    Description = tc.Category.Description,
                    CreatedAt = tc.Category.CreatedAt,
                    UpdatedAt = tc.Category.UpdatedAt
                })
                .ToList(),
            Tags = todo.TodoTags
                .Select(tt => new TagDto
                {
                    Id = tt.Tag.Id,
                    Name = tt.Tag.Name,
                    Color = tt.Tag.Color,
                    Description = tt.Tag.Description,
                    CreatedAt = tt.Tag.CreatedAt,
                    UpdatedAt = tt.Tag.UpdatedAt
                })
                .ToList()
        };
    }
}



