export interface Category {
  id: number
  name: string
  color: string
  userId: number
  createdAt: string
  updatedAt?: string
}

export interface CreateCategoryRequest {
  name: string
  color: string
}

export interface UpdateCategoryRequest {
  name?: string
  color?: string
}

export interface Tag {
  id: number
  name: string
  userId: number
  createdAt: string
}

export interface CreateTagRequest {
  name: string
}

