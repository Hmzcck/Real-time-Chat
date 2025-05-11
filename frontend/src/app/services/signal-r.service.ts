import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { Message, MessageSend } from '../models/message.model';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private readonly hubConnection: HubConnection;
  private readonly messageReceivedSource = new Subject<Message>();
  private readonly userStatusSource = new BehaviorSubject<{
    [key: string]: boolean;
  }>({});
  private readonly userLeftChatSource = new Subject<{
    chatId: string;
    userId: string;
    username: string;
  }>();

  messageReceived$ = this.messageReceivedSource.asObservable();
  userStatus$ = this.userStatusSource.asObservable();
  userLeftChat$ = this.userLeftChatSource.asObservable();

  constructor() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/chatHub`, {
        accessTokenFactory: () => {
          const user = localStorage.getItem('currentUser');
          if (!user) return '';
          try {
            return JSON.parse(user).token;
          } catch {
            return '';
          }
        },
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveMessage', (message: Message) => {
      console.log('Message received:', message);
      this.messageReceivedSource.next(message);
    });

    this.hubConnection.on('UserOnline', (userId: string) => {
      const currentStatus = this.userStatusSource.value;
      this.userStatusSource.next({ ...currentStatus, [userId]: true });
    });

    this.hubConnection.on('UserOffline', (userId: string) => {
      const currentStatus = this.userStatusSource.value;
      this.userStatusSource.next({ ...currentStatus, [userId]: false });
    });

    this.hubConnection.on('OnlineUsersList', (userIds: string[]) => {
      const statusMap: { [key: string]: boolean } = {};
      userIds.forEach(id => statusMap[id] = true);
      this.userStatusSource.next(statusMap);
    });

    this.hubConnection.on('UserLeftChat', (data: { chatId: string; userId: string; username: string }) => {
      this.userLeftChatSource.next(data);
    });

    this.hubConnection.on('ChatError', (data: { chatId: string; error: string }) => {
      console.error('Chat error:', data);
    });
  }


  async start(): Promise<void> {
    try {
      await this.hubConnection.start();
      console.log('SignalR connection started');
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      setTimeout(() => this.start(), 5000);
    }
  }

  async stop(): Promise<void> {
    try {
      await this.hubConnection.stop();
      console.log('SignalR connection stopped');
    } catch (error) {
      console.error('Error stopping SignalR connection:', error);
    }
  }

  async sendMessage(message: MessageSend): Promise<void> {
    try {
      await this.hubConnection.invoke('SendMessage', message.chatId, message.content);
    } catch (error) {
      console.error('Error sending message:', error);
    }
  }

  async leaveChat(chatId: string): Promise<void> {
    try {
      await this.hubConnection.invoke('LeaveChat', chatId);
    } catch (error) {
      console.error('Error leaving chat:', error);
    }
  }
}
