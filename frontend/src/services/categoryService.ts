import { api } from './authService'
import { Category, CreateCategoryRequest, UpdateCategoryRequest, Tag, CreateTagRequest } from '../types/category'

export const categoryService = {
  async getCategories(): Promise<Category[]> {
    const response = await api.get<Category[]>('/category')
    return response.data
  },

  async getCategoryById(id: number): Promise<Category> {
    const response = await api.get<Category>(`/category/${id}`)
    return response.data
  },

  async createCategory(request: CreateCategoryRequest): Promise<Category> {
    const response = await api.post<Category>('/category', request)
    return response.data
  },

  async updateCategory(id: number, request: UpdateCategoryRequest): Promise<Category> {
    const response = await api.put<Category>(`/category/${id}`, request)
    return response.data
  },

  async deleteCategory(id: number): Promise<void> {
    await api.delete(`/category/${id}`)
  },
}

export const tagService = {
  async getTags(): Promise<Tag[]> {
    const response = await api.get<Tag[]>('/tag')
    return response.data
  },

  async createTag(request: CreateTagRequest): Promise<Tag> {
    const response = await api.post<Tag>('/tag', request)
    return response.data
  },

  async deleteTag(id: number): Promise<void> {
    await api.delete(`/tag/${id}`)
  },
}

