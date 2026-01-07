import { api } from './authService'
import { Comment, CreateCommentRequest, UpdateCommentRequest } from '../types/sharing'

export const commentService = {
  async createComment(request: CreateCommentRequest): Promise<Comment> {
    const response = await api.post<Comment>('/comment', request)
    return response.data
  },

  async updateComment(id: number, request: UpdateCommentRequest): Promise<Comment> {
    const response = await api.put<Comment>(`/comment/${id}`, request)
    return response.data
  },

  async deleteComment(id: number): Promise<void> {
    await api.delete(`/comment/${id}`)
  },

  async getTodoComments(todoId: number): Promise<Comment[]> {
    const response = await api.get<Comment[]>(`/comment/todo/${todoId}`)
    return response.data
  },

  async getCommentById(id: number): Promise<Comment> {
    const response = await api.get<Comment>(`/comment/${id}`)
    return response.data
  },
}

