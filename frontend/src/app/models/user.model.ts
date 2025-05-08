// TODO change names according to backend

export interface User {
  id: string;
  userName: string;
  email: string;
  avatarPath: string;
  isOnline: boolean;
}

export interface UserDetails extends User {
  chats: string[];
}

export interface OnlineStatusChange {
  userId: string;
  isOnline: boolean;
}
