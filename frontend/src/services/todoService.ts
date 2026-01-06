import { api } from './authService'
import { Todo, CreateTodoRequest, UpdateTodoRequest } from '../types/todo'

export const todoService = {
  async getTodos(): Promise<Todo[]> {
    const response = await api.get<Todo[]>('/todo')
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

