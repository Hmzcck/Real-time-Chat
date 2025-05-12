export interface Chat {
  id: string;
  name: string;
  isPrivate: boolean;
  members: string[];
  lastMessage?: string;
  lastMessageAt?: string;
  lastMessageSender?: string;
  memberCount: number;
  imagePath?: string;
}

export interface ChatParticipant {
  userId: string;
  chatId: string;
}

export interface ChatCreation {
  name: string;
  isPrivate: boolean;
  imagePath?: string;
  initialMemberIds: string[];
}

export interface ChatUpdate {
  chatId: string;
  name?: string;
  addedParticipantIds?: string[];
  removedParticipantIds?: string[];
}

// SignalR related interfaces
export interface UserTypingEvent {
  chatId: string;
  username: string;
}

export interface JoinChatEvent {
  chatId: string;
}

export interface LeaveChatEvent {
  chatId: string;
}

// Import necessary types
import { User } from './user.model';
import { Message } from './message.model';
