// TODO Change names according to backend
export interface Message {
  id: string;
  content: string;
  sendAt: string;
  senderId: string;
  senderUserName?: string;
  chatId: string;
}

export interface MessageSend {
  chatId: string;
  content: string;
}

export interface MessageReceive extends Message {
  senderAvatar?: string;
}

export interface MessagesQuery {
  chatId: string;
  page?: number;
  pageSize?: number;
}
