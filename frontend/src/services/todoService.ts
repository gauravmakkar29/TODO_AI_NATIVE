import { api } from './authService'
import { Todo, CreateTodoRequest, UpdateTodoRequest, BulkTodoRequest, TodoStatistics } from '../types/todo'

export const todoService = {
  async getTodos(sortBy?: string, priorityFilter?: number): Promise<Todo[]> {
    const params = new URLSearchParams()
    if (sortBy) params.append('sortBy', sortBy)
    if (priorityFilter !== undefined) params.append('priorityFilter', priorityFilter.toString())
    
    const queryString = params.toString()
    const url = queryString ? `/todo?${queryString}` : '/todo'
    const response = await api.get<Todo[]>(url)
    return response.data
  },

  async getTodoById(id: number): Promise<Todo> {
    const response = await api.get<Todo>(`/todo/${id}`)
    return response.data
  },

  async createTodo(request: CreateTodoRequest): Promise<Todo> {
    const response = await api.post<Todo>('/todo', request)
    return response.data
  },

  async updateTodo(id: number, request: UpdateTodoRequest): Promise<Todo> {
    const response = await api.put<Todo>(`/todo/${id}`, request)
    return response.data
  },

  async deleteTodo(id: number): Promise<void> {
    await api.delete(`/todo/${id}`)
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
    const response = await api.get<TodoStatistics>('/todo/statistics')
    return response.data
  },

  async archiveOldCompletedTodos(daysOld: number = 30): Promise<{ message: string; count: number }> {
    const response = await api.post<{ message: string; count: number }>(`/todo/archive-old?daysOld=${daysOld}`)
    return response.data
  },
}

