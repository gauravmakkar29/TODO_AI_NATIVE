import { api } from './authService'
import { cacheService } from './cacheService'
import { Todo, CreateTodoRequest, UpdateTodoRequest, BulkTodoRequest, TodoStatistics } from '../types/todo'

// Check if online
const isOnline = () => navigator.onLine;

export const todoService = {
  async getTodos(sortBy?: string, priorityFilter?: number): Promise<Todo[]> {
    const cacheKey = `todos:${sortBy || 'default'}:${priorityFilter ?? 'all'}`
    
    // Try cache first
    const cached = await cacheService.get<Todo[]>(cacheKey)
    if (cached && !isOnline()) {
      return cached
    }

    try {
    const params = new URLSearchParams()
    if (sortBy) params.append('sortBy', sortBy)
    if (priorityFilter !== undefined) params.append('priorityFilter', priorityFilter.toString())
    
    const queryString = params.toString()
    const url = queryString ? `/todo?${queryString}` : '/todo'
    const response = await api.get<Todo[]>(url)
      
      // Cache the result (2 minutes expiry)
      await cacheService.set(cacheKey, response.data, 2)
      
    return response.data
    } catch (error: any) {
      // If offline and we have cached data, return it
      if (!isOnline() && cached) {
        return cached
      }
      throw error
    }
  },

  async getTodoById(id: number): Promise<Todo> {
    const cacheKey = `todo:${id}`
    
    // Try cache first
    const cached = await cacheService.get<Todo>(cacheKey)
    if (cached && !isOnline()) {
      return cached
    }

    try {
    const response = await api.get<Todo>(`/todo/${id}`)
      
      // Cache the result (10 minutes expiry)
      await cacheService.set(cacheKey, response.data, 10)
      
    return response.data
    } catch (error: any) {
      // If offline and we have cached data, return it
      if (!isOnline() && cached) {
        return cached
      }
      throw error
    }
  },

  async createTodo(request: CreateTodoRequest): Promise<Todo> {
    try {
    const response = await api.post<Todo>('/todo', request)
      
      // Invalidate todos list cache
      await cacheService.remove('todos:default:all')
      await cacheService.remove('todos:default:undefined')
      
      // Cache the new todo
      await cacheService.set(`todo:${response.data.id}`, response.data, 10)
      
    return response.data
    } catch (error: any) {
      // If offline, queue for sync
      if (!isOnline()) {
        // Store in offline queue (would need additional implementation)
        console.warn('Offline: Todo creation queued for sync')
      }
      throw error
    }
  },

  async updateTodo(id: number, request: UpdateTodoRequest): Promise<Todo> {
    try {
    const response = await api.put<Todo>(`/todo/${id}`, request)
      
      // Invalidate todos list cache
      await cacheService.remove('todos:default:all')
      await cacheService.remove('todos:default:undefined')
      
      // Update cached todo
      await cacheService.set(`todo:${id}`, response.data, 10)
      
    return response.data
    } catch (error: any) {
      // If offline, queue for sync
      if (!isOnline()) {
        console.warn('Offline: Todo update queued for sync')
      }
      throw error
    }
  },

  async deleteTodo(id: number): Promise<void> {
    try {
    await api.delete(`/todo/${id}`)
      
      // Remove from cache
      await cacheService.remove(`todo:${id}`)
      await cacheService.remove('todos:default:all')
      await cacheService.remove('todos:default:undefined')
    } catch (error: any) {
      // If offline, queue for sync
      if (!isOnline()) {
        console.warn('Offline: Todo deletion queued for sync')
      }
      throw error
    }
  },

  async reorderTodos(todoOrders: { todoId: number; displayOrder: number }[]): Promise<void> {
    await api.post('/todo/reorder', { 
      todoOrders: todoOrders
    })
  },

  async bulkMarkComplete(request: BulkTodoRequest): Promise<{ message: string; count: number }> {
    const response = await api.post<{ message: string; count: number }>('/todo/bulk-complete', request)
    return response.data
  },

  async getStatistics(): Promise<TodoStatistics> {
    const cacheKey = 'statistics'
    
    // Try cache first
    const cached = await cacheService.get<TodoStatistics>(cacheKey)
    if (cached && !isOnline()) {
      return cached
    }

    try {
    const response = await api.get<TodoStatistics>('/todo/statistics')
      
      // Cache the result (1 minute expiry)
      await cacheService.set(cacheKey, response.data, 1)
      
    return response.data
    } catch (error: any) {
      // If offline and we have cached data, return it
      if (!isOnline() && cached) {
        return cached
      }
      throw error
    }
  },

  async archiveOldCompletedTodos(daysOld: number = 30): Promise<{ message: string; count: number }> {
    const response = await api.post<{ message: string; count: number }>(`/todo/archive-old?daysOld=${daysOld}`)
    return response.data
  },
}

