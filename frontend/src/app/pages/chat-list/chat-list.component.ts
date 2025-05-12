import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../../services/authentication.service';
import { ChatService } from '../../services/chat.service';
import { UserService } from '../../services/user.service';
import { SignalRService } from '../../services/signal-r.service';
import { User } from '../../models/user.model';
import { Chat, ChatCreation } from '../../models/chat.model';
import { Message, MessageSend } from '../../models/message.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule } from '@angular/material/list';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { HttpErrorResponse } from '@angular/common/http';
import { CreateGroupChatDialogComponent } from '../../components/create-group-chat-dialog/create-group-chat-dialog.component';

@Component({
  selector: 'app-chat-list',
  templateUrl: './chat-list.component.html',
  styleUrls: ['./chat-list.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatListModule,
    MatTooltipModule,
    MatDialogModule,
  ],
})
export class ChatListComponent implements OnInit, OnDestroy {
  users: User[] = [];
  allChats: Chat[] = [];
  currentUserId = '';
  currentUser: any;
  selectedChat: Chat | null = null;
  messages: Message[] = [];
  newMessage = '';

  getGroupMembers(chat: Chat): string {
    return chat.members?.join(', ') || '';
  }

  constructor(
    private userService: UserService,
    private chatService: ChatService,
    private signalRService: SignalRService,
    private authService: AuthenticationService,
    private router: Router,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.signalRService.start();

    const currentUserStr = localStorage.getItem('currentUser');
    if (currentUserStr) {
      this.currentUser = JSON.parse(currentUserStr);
      this.currentUserId = this.currentUser.userId;
    }

    // Load users and chats
    this.userService.getUsers().subscribe((users) => {
      this.users = users.filter((u) => u.id !== this.currentUserId);
    });

    this.chatService.getChats().subscribe((chats) => {
      this.allChats = chats.sort((a, b) => {
        // Sort by last message time, handling nulls
        const timeA = a.lastMessageAt ? new Date(a.lastMessageAt).getTime() : 0;
        const timeB = b.lastMessageAt ? new Date(b.lastMessageAt).getTime() : 0;
        return timeB - timeA;
      });
    });

    // Subscribe to real-time message updates
    this.signalRService.messageReceived$.subscribe((message) => {
      if (this.selectedChat && message.chatId === this.selectedChat.id) {
        this.messages.push(message);
        this.messages.sort(
          (a, b) => new Date(a.sendAt).getTime() - new Date(b.sendAt).getTime()
        );
      }

      // Last message update for the chat
      const chatToUpdate = this.allChats.find((c: Chat) => c.id === message.chatId);
      if (chatToUpdate) {
        chatToUpdate.lastMessage = message.content;
        chatToUpdate.lastMessageSender = message.senderUserName;
        chatToUpdate.lastMessageAt = message.sendAt;
        
        // Re-sort chats to move the updated chat to top
        this.allChats.sort((a, b) => {
          const timeA = a.lastMessageAt ? new Date(a.lastMessageAt).getTime() : 0;
          const timeB = b.lastMessageAt ? new Date(b.lastMessageAt).getTime() : 0;
          return timeB - timeA;
        });
      }
    });

    this.signalRService.userStatus$.subscribe((statusMap) => {
      this.users = this.users.map((user) => ({
        ...user,
        isOnline: statusMap[user.id] || false,
      }));
    });

    // Subscribe to user left chat notifications
    this.signalRService.userLeftChat$.subscribe((data) => {
      if (this.selectedChat && data.chatId === this.selectedChat.id) {
        // If current user left, close the chat
        if (data.userId === this.currentUserId) {
          this.selectedChat = null;
          this.messages = [];
        // Remove from chats list
        this.allChats = this.allChats.filter(
          (chat) => chat.id !== data.chatId
        );
        } else {
          // Update the selected chat's members list if available
          this.chatService
            .getChatDetails(data.chatId)
            .subscribe((updatedChat) => {
              this.selectedChat = updatedChat;
            });
        }
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

  leaveChat(chat: Chat) {
    if (chat.isPrivate) {
      return;
    }

    this.chatService.leaveChat(chat.id).subscribe({
      next: () => {
        // Remove from chats list immediately
        this.allChats = this.allChats.filter((c) => c.id !== chat.id);
        // If this is the currently selected chat, clear it
        if (this.selectedChat && this.selectedChat.id === chat.id) {
          this.selectedChat = null;
          this.messages = [];
        }
        this.signalRService.leaveChat(chat.id);
      },
      error: (error) => {
        console.error('Error leaving chat:', error);
      },
    });
  }

  openChat(chat: Chat) {
    this.selectedChat = chat;
    this.chatService.getChatMessages(chat.id).subscribe((messages) => {
      this.messages = messages.sort(
        (a, b) => new Date(a.sendAt).getTime() - new Date(b.sendAt).getTime()
      );
    });
  }

  createGroupChat() {
    const dialogRef = this.dialog.open(CreateGroupChatDialogComponent, {
      width: '500px',
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        const request: ChatCreation = {
          name: result.name,
          isPrivate: false,
          initialMemberIds: result.users.map((user: User) => user.id),
          imagePath: result.imagePath
        };

        this.chatService.createChat(request).subscribe((newChat) => {
          // Add new chat to the list and open it
          this.allChats.unshift(newChat);
          this.openChat(newChat);
        });
      }
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

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
