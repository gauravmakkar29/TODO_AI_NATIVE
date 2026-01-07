export enum SharePermission {
  ViewOnly = 0,
  Edit = 1,
  Admin = 2
}

export enum ActivityType {
  Created = 0,
  Updated = 1,
  Completed = 2,
  Uncompleted = 3,
  Deleted = 4,
  Shared = 5,
  Unshared = 6,
  Assigned = 7,
  Unassigned = 8,
  CommentAdded = 9,
  PermissionChanged = 10
}

export interface ShareTodoRequest {
  todoId: number;
  sharedWithUserId: number;
  permission: SharePermission;
  isAssigned: boolean;
}

export interface ShareTodoResponse {
  id: number;
  todoId: number;
  sharedWithUserId: number;
  sharedWithUserEmail: string;
  sharedWithUserName?: string;
  sharedByUserId: number;
  sharedByUserEmail: string;
  permission: SharePermission;
  isAssigned: boolean;
  createdAt: string;
}

export interface UpdateSharePermissionRequest {
  permission: SharePermission;
}

export interface TodoShareInfo {
  id: number;
  sharedWithUserId: number;
  sharedWithUserEmail: string;
  sharedWithUserName?: string;
  sharedByUserId: number;
  sharedByUserEmail: string;
  permission: SharePermission;
  isAssigned: boolean;
  createdAt: string;
}

export interface SharedTodo extends Todo {
  ownerUserId: number;
  ownerEmail: string;
  ownerName?: string;
  userPermission?: SharePermission;
  isAssignedToUser: boolean;
  sharedWith: TodoShareInfo[];
}

export interface Comment {
  id: number;
  todoId: number;
  userId: number;
  userEmail: string;
  userName?: string;
  comment: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateCommentRequest {
  todoId: number;
  comment: string;
}

export interface UpdateCommentRequest {
  comment: string;
}

export interface Activity {
  id: number;
  todoId: number;
  userId: number;
  userEmail: string;
  userName?: string;
  activityType: ActivityType;
  description?: string;
  relatedUserId?: number;
  relatedUserEmail?: string;
  relatedUserName?: string;
  createdAt: string;
}

// Import Todo type
import { Todo } from './todo';

