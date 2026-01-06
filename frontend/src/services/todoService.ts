import { api } from './authService'
import { Todo, CreateTodoRequest, UpdateTodoRequest } from '../types/todo'

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
}

