import { api } from './authService'
import {
  ShareTodoRequest,
  ShareTodoResponse,
  UpdateSharePermissionRequest,
  SharedTodo,
  Activity
} from '../types/sharing'

export interface User {
  id: number
  email: string
  firstName?: string
  lastName?: string
}

export const sharingService = {
  async searchUsers(query: string): Promise<User[]> {
    const response = await api.get<User[]>('/user/search', {
      params: { query }
    })
    return response.data
  },

  async shareTodo(request: ShareTodoRequest): Promise<ShareTodoResponse> {
    const response = await api.post<ShareTodoResponse>('/sharing/share', request)
    return response.data
  },

  async unshareTodo(todoId: number, sharedWithUserId: number): Promise<void> {
    await api.delete(`/sharing/unshare/${todoId}/${sharedWithUserId}`)
  },

  async updatePermission(
    todoId: number,
    sharedWithUserId: number,
    request: UpdateSharePermissionRequest
  ): Promise<void> {
    await api.put(`/sharing/permission/${todoId}/${sharedWithUserId}`, request)
  },

  async getTodoShares(todoId: number): Promise<ShareTodoResponse[]> {
    const response = await api.get<ShareTodoResponse[]>(`/sharing/todo/${todoId}`)
    return response.data
  },

  async getSharedTodos(): Promise<SharedTodo[]> {
    const response = await api.get<SharedTodo[]>('/sharing/shared')
    return response.data
  },

  async getTodoActivities(todoId: number): Promise<Activity[]> {
    const response = await api.get<Activity[]>(`/sharing/activity/${todoId}`)
    return response.data
  },
}

