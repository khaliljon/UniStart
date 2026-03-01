import * as signalR from '@microsoft/signalr';
import type { Message } from '../types';

type MessageHandler = (message: Message) => void;
type UnreadHandler = (count: number) => void;
type TypingHandler = (conversationId: number, userName: string) => void;
type ReadHandler = (conversationId: number) => void;

class ChatService {
  private connection: signalR.HubConnection | null = null;
  private onMessageHandlers: MessageHandler[] = [];
  private onUnreadHandlers: UnreadHandler[] = [];
  private onTypingHandlers: TypingHandler[] = [];
  private onReadHandlers: ReadHandler[] = [];

  async start(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;

    const token = localStorage.getItem('token');
    if (!token) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/chat', { accessTokenFactory: () => token })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection.on('ReceiveMessage', (msg: Message) => {
      this.onMessageHandlers.forEach(h => h(msg));
    });

    this.connection.on('UnreadCountUpdate', (count: number) => {
      this.onUnreadHandlers.forEach(h => h(count));
    });

    this.connection.on('UserTyping', (conversationId: number, userName: string) => {
      this.onTypingHandlers.forEach(h => h(conversationId, userName));
    });

    this.connection.on('MessagesRead', (conversationId: number) => {
      this.onReadHandlers.forEach(h => h(conversationId));
    });

    try {
      await this.connection.start();
    } catch (err) {
      console.error('SignalR connection failed:', err);
    }
  }

  async stop(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
    this.onMessageHandlers = [];
    this.onUnreadHandlers = [];
    this.onTypingHandlers = [];
    this.onReadHandlers = [];
  }

  async sendMessage(conversationId: number, text: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('SendMessage', conversationId, text);
    }
  }

  async markAsRead(conversationId: number): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('MarkAsRead', conversationId);
    }
  }

  async sendTyping(conversationId: number): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('Typing', conversationId);
    }
  }

  onMessage(handler: MessageHandler): () => void {
    this.onMessageHandlers.push(handler);
    return () => {
      this.onMessageHandlers = this.onMessageHandlers.filter(h => h !== handler);
    };
  }

  onUnreadCount(handler: UnreadHandler): () => void {
    this.onUnreadHandlers.push(handler);
    return () => {
      this.onUnreadHandlers = this.onUnreadHandlers.filter(h => h !== handler);
    };
  }

  onTyping(handler: TypingHandler): () => void {
    this.onTypingHandlers.push(handler);
    return () => {
      this.onTypingHandlers = this.onTypingHandlers.filter(h => h !== handler);
    };
  }

  onRead(handler: ReadHandler): () => void {
    this.onReadHandlers.push(handler);
    return () => {
      this.onReadHandlers = this.onReadHandlers.filter(h => h !== handler);
    };
  }

  get isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

// Singleton
export const chatService = new ChatService();
