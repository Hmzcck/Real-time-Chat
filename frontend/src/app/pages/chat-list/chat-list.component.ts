import { Component, OnDestroy, OnInit } from '@angular/core';
import { ChatService } from '../../services/chat.service';
import { UserService } from '../../services/user.service';
import { SignalRService } from '../../services/signal-r.service';
import { User } from '../../models/user.model';
import { Chat, ChatCreation } from '../../models/chat.model';
import { Message, MessageSend } from '../../models/message.model';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule } from '@angular/material/list';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-chat-list',
  templateUrl: './chat-list.component.html',
  styleUrls: ['./chat-list.component.css'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    FormsModule,
    MatListModule,
  ],
})
export class ChatListComponent implements OnInit, OnDestroy {
  users: User[] = [];
  currentUserId = '';
  selectedChat: Chat | null = null;
  messages: Message[] = [];
  newMessage = '';

  constructor(
    private readonly userService: UserService,
    private readonly chatService: ChatService,
    private readonly signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    this.signalRService.start();

    const currentUser = localStorage.getItem('currentUser');
    if (currentUser) {
      this.currentUserId = JSON.parse(currentUser).userId;
    }

    this.userService.getUsers().subscribe((users) => {
      this.users = users.filter((u) => u.id !== this.currentUserId);
    });

    // Subscribe to real-time message updates
    this.signalRService.messageReceived$.subscribe((message) => {
      if (this.selectedChat && message.chatId === this.selectedChat.id) {
        this.messages.push(message);
        this.messages.sort((a, b) => 
          new Date(a.sendAt).getTime() - new Date(b.sendAt).getTime()
        );
      }
    });
  }

  ngOnDestroy(): void {
    this.signalRService.stop();
  }

  onUserClick(user: User) {
    this.chatService.getPrivateChatWithUser(user.id).subscribe({
      next: (chat) => {
        this.openChat(chat);
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 404) {
          const request: ChatCreation = {
            name: '',
            isPrivate: true,
            initialMemberIds: [user.id],
          };
          this.chatService.createChat(request).subscribe((newChat) => {
            this.openChat(newChat);
          });
        } else {
          console.error('Chat fetch error:', err);
        }
      },
    });
  }

  openChat(chat: Chat) {
    this.selectedChat = chat;
    this.chatService.getChatMessages(chat.id).subscribe((messages) => {
      this.messages = messages.sort((a, b) => 
        new Date(a.sendAt).getTime() - new Date(b.sendAt).getTime()
      );
    });
  }

  sendMessage() {
    if (!this.newMessage.trim() || !this.selectedChat) return;

    const request: MessageSend = {
      chatId: this.selectedChat.id,
      content: this.newMessage,
    };

    this.signalRService.sendMessage(request);
    this.newMessage = '';
  }
}
