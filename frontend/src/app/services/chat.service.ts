import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Chat, ChatCreation } from '../models/chat.model';
import { Message, MessageSend } from '../models/message.model';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  getChats(): Observable<Chat[]> {
    return this.http.get<Chat[]>(`${this.apiUrl}/chats`);
  }

  getChatDetails(chatId: string): Observable<Chat> {
    return this.http.get<Chat>(`${this.apiUrl}/chats/${chatId}`);
  }

  getPrivateChatWithUser(userId: string): Observable<Chat> {
    return this.http.get<Chat>(`${this.apiUrl}/chats/private/${userId}`);
  }

  createChat(request: ChatCreation): Observable<Chat> {
    console.log('Creating chat with request:', request);

    return this.http.post<Chat>(`${this.apiUrl}/chats`, request);
  }

  addUserToChat(chatId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/chats/${chatId}/users`, {
      chatId,
      userId,
    });
  }

  getChatMessages(chatId: string): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.apiUrl}/messages/chat/${chatId}`);
  }

  sendMessage(request: MessageSend): Observable<Message> {
    return this.http.post<Message>(`${this.apiUrl}/messages`, request);
  }

  leaveChat(chatId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/chats/${chatId}/leave`);
  }
}
