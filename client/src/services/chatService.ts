import * as signalR from '@microsoft/signalr';
import type { Message } from '../types';

type MessageHandler = (message: Message) => void;
type UnreadHandler = (count: number) => void;
type TypingHandler = (conversationId: number, userName: string) => void;
type ReadHandler = (conversationId: number) => void;
type PresenceHandler = (userId: number) => void;
type StatusChangedHandler = (conversationId: number, newStatus: string) => void;

class ChatService {
  private connection: signalR.HubConnection | null = null;
  private startPromise: Promise<void> | null = null;
  private currentToken: string | null = null;
  private onMessageHandlers: MessageHandler[] = [];
  private onUnreadHandlers: UnreadHandler[] = [];
  private onTypingHandlers: TypingHandler[] = [];
  private onReadHandlers: ReadHandler[] = [];
  private onOnlineHandlers: PresenceHandler[] = [];
  private onOfflineHandlers: PresenceHandler[] = [];
  private onStatusChangedHandlers: StatusChangedHandler[] = [];

  async start(): Promise<void> {
    const token = localStorage.getItem('token');
    if (!token) return;

    // If token changed (account switch), force full reconnect
    if (this.currentToken && this.currentToken !== token) {
      if (this.connection) {
        try { await this.connection.stop(); } catch { /* ignore */ }
        this.connection = null;
      }
      this.startPromise = null;
      this.currentToken = null;
    }

    // Already connected with the same token — nothing to do
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;

    // Currently connecting — wait for it instead of creating orphaned connections
    if (this.startPromise) {
      try { await this.startPromise; } catch { /* ignore */ }
      return;
    }

    // Connection exists but in a bad state (Disconnected/Reconnecting) — stop it first
    if (this.connection) {
      try { await this.connection.stop(); } catch { /* ignore */ }
      this.connection = null;
    }

    this.currentToken = token;

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

    this.connection.on('UserOnline', (userId: number) => {
      this.onOnlineHandlers.forEach(h => h(userId));
    });

    this.connection.on('UserOffline', (userId: number) => {
      this.onOfflineHandlers.forEach(h => h(userId));
    });

    this.connection.on('ConversationStatusChanged', (conversationId: number, newStatus: string) => {
      this.onStatusChangedHandlers.forEach(h => h(conversationId, newStatus));
    });

    this.startPromise = this.connection.start();
    try {
      await this.startPromise;
    } catch (err) {
      console.error('SignalR connection failed:', err);
    } finally {
      this.startPromise = null;
    }
  }

  async stop(): Promise<void> {
    this.startPromise = null;
    this.currentToken = null;
    if (this.connection) {
      try { await this.connection.stop(); } catch { /* ignore */ }
      this.connection = null;
    }
    this.onMessageHandlers = [];
    this.onUnreadHandlers = [];
    this.onTypingHandlers = [];
    this.onReadHandlers = [];
    this.onOnlineHandlers = [];
    this.onOfflineHandlers = [];
    this.onStatusChangedHandlers = [];
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

  onOnline(handler: PresenceHandler): () => void {
    this.onOnlineHandlers.push(handler);
    return () => {
      this.onOnlineHandlers = this.onOnlineHandlers.filter(h => h !== handler);
    };
  }

  onOffline(handler: PresenceHandler): () => void {
    this.onOfflineHandlers.push(handler);
    return () => {
      this.onOfflineHandlers = this.onOfflineHandlers.filter(h => h !== handler);
    };
  }

  onStatusChanged(handler: StatusChangedHandler): () => void {
    this.onStatusChangedHandlers.push(handler);
    return () => {
      this.onStatusChangedHandlers = this.onStatusChangedHandlers.filter(h => h !== handler);
    };
  }

  get isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

// Singleton
export const chatService = new ChatService();
