import { useState, useEffect, useRef, useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { messageService } from '../services/messageService';
import { chatService } from '../services/chatService';
import type { Conversation, Message } from '../types';

function MessagesPage() {
  const [searchParams, setSearchParams] = useSearchParams();

  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [activeId, setActiveId] = useState<number | null>(Number(searchParams.get('c')) || null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [loadingConvs, setLoadingConvs] = useState(true);
  const [loadingMsgs, setLoadingMsgs] = useState(false);
  const [messageText, setMessageText] = useState('');
  const [sending, setSending] = useState(false);
  const [typingUser, setTypingUser] = useState('');
  const [mobileShowChat, setMobileShowChat] = useState(!!searchParams.get('c'));

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const typingTimeout = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  // Load conversations
  const loadConversations = useCallback(async () => {
    try {
      const data = await messageService.getConversations();
      setConversations(data);
    } catch (err) {
      console.error('Failed to load conversations:', err);
    } finally {
      setLoadingConvs(false);
    }
  }, []);

  // Load messages for active conversation
  const loadMessages = useCallback(async (convId: number) => {
    setLoadingMsgs(true);
    try {
      const data = await messageService.getMessages(convId);
      setMessages(data.items.reverse()); // API returns newest first, we display oldest first
      // Mark as read
      await messageService.markAsRead(convId);
      setConversations(prev => prev.map(c =>
        c.id === convId ? { ...c, unreadCount: 0 } : c
      ));
    } catch (err) {
      console.error('Failed to load messages:', err);
    } finally {
      setLoadingMsgs(false);
    }
  }, []);

  // Initialize SignalR
  useEffect(() => {
    chatService.start();

    const unsubMsg = chatService.onMessage((msg: Message) => {
      // Add message if it belongs to active conversation
      setMessages(prev => {
        // Check if this message is for the current conversation by sender
        // We include all incoming messages and update conversations list
        return [...prev, msg];
      });
      // Update conversation preview
      setConversations(prev => prev.map(c => {
        if (msg.isMine || c.otherUserId === msg.senderId) {
          return {
            ...c,
            lastMessagePreview: msg.text.slice(0, 100),
            lastMessageAt: msg.sentAt,
            unreadCount: msg.isMine ? c.unreadCount : c.unreadCount + 1,
          };
        }
        return c;
      }));
    });

    const unsubTyping = chatService.onTyping((_convId, userName) => {
      setTypingUser(userName);
      if (typingTimeout.current) clearTimeout(typingTimeout.current);
      typingTimeout.current = setTimeout(() => setTypingUser(''), 3000);
    });

    const unsubRead = chatService.onRead((_convId) => {
      setMessages(prev => prev.map(m =>
        m.isMine && !m.readAt ? { ...m, readAt: new Date().toISOString() } : m
      ));
    });

    return () => {
      unsubMsg();
      unsubTyping();
      unsubRead();
    };
  }, []);

  useEffect(() => {
    loadConversations();
  }, [loadConversations]);

  useEffect(() => {
    if (activeId) {
      loadMessages(activeId);
      setSearchParams({ c: String(activeId) }, { replace: true });
    }
  }, [activeId, loadMessages, setSearchParams]);

  // Auto scroll to bottom
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!messageText.trim() || !activeId) return;
    setSending(true);
    try {
      await chatService.sendMessage(activeId, messageText.trim());
      setMessageText('');
    } catch {
      // Fallback to REST
      try {
        await messageService.sendMessage(activeId, messageText.trim());
        setMessageText('');
        loadMessages(activeId);
      } catch {
        alert('Не удалось отправить сообщение');
      }
    } finally {
      setSending(false);
    }
  };

  const handleTyping = () => {
    if (activeId) {
      chatService.sendTyping(activeId);
    }
  };

  const selectConversation = (id: number) => {
    setActiveId(id);
    setMobileShowChat(true);
  };

  const formatTime = (dateStr: string) => {
    const d = new Date(dateStr);
    const now = new Date();
    const isToday = d.toDateString() === now.toDateString();
    if (isToday) return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
    return d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' }) + ' ' +
           d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
  };

  const getInitials = (name: string) =>
    name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);

  // ─── Conversation list sidebar ──────────────
  const renderConversationList = () => (
    <div style={{
      width: '320px', minWidth: '280px', borderRight: '1px solid var(--border-color)',
      display: 'flex', flexDirection: 'column', height: '100%',
      ...(mobileShowChat ? { display: 'none' } : {}),
    }}
    className="chat-sidebar"
    >
      <div style={{
        padding: '1rem', borderBottom: '1px solid var(--border-color)',
        fontWeight: 700, fontSize: '1.1rem',
      }}>
        💬 Сообщения
      </div>
      <div style={{ flex: 1, overflowY: 'auto' }}>
        {loadingConvs ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>Загрузка...</div>
        ) : conversations.length === 0 ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
            <div style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>📭</div>
            Нет диалогов
          </div>
        ) : (
          conversations.map(conv => (
            <div
              key={conv.id}
              onClick={() => selectConversation(conv.id)}
              style={{
                padding: '0.75rem 1rem',
                cursor: 'pointer',
                display: 'flex', gap: '0.75rem', alignItems: 'center',
                background: activeId === conv.id ? 'var(--bg-secondary)' : 'transparent',
                borderLeft: activeId === conv.id ? '3px solid var(--primary-color)' : '3px solid transparent',
                transition: 'background 0.15s',
              }}
              onMouseEnter={(e) => { if (activeId !== conv.id) (e.currentTarget as HTMLElement).style.background = 'var(--bg-secondary)'; }}
              onMouseLeave={(e) => { if (activeId !== conv.id) (e.currentTarget as HTMLElement).style.background = 'transparent'; }}
            >
              <div style={{
                width: '42px', height: '42px', borderRadius: '50%', flexShrink: 0,
                background: 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff', fontWeight: 700, fontSize: '0.85rem',
              }}>
                {getInitials(conv.otherUserName)}
              </div>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontWeight: 600, fontSize: '0.9rem' }}>{conv.otherUserName}</span>
                  {conv.lastMessageAt && (
                    <span style={{ fontSize: '0.72rem', color: 'var(--text-secondary)', flexShrink: 0, marginLeft: '0.5rem' }}>
                      {formatTime(conv.lastMessageAt)}
                    </span>
                  )}
                </div>
                <div style={{
                  fontSize: '0.82rem', color: 'var(--text-secondary)',
                  overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
                  display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                }}>
                  <span style={{ overflow: 'hidden', textOverflow: 'ellipsis' }}>
                    {conv.lastMessagePreview || 'Начните диалог'}
                  </span>
                  {conv.unreadCount > 0 && (
                    <span style={{
                      background: 'var(--primary-color)', color: '#fff',
                      borderRadius: '999px', padding: '0.1rem 0.45rem',
                      fontSize: '0.7rem', fontWeight: 700, flexShrink: 0, marginLeft: '0.5rem',
                    }}>
                      {conv.unreadCount}
                    </span>
                  )}
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );

  // ─── Chat area ──────────────────────────────
  const renderChatArea = () => {
    if (!activeId) {
      return (
        <div style={{
          flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center',
          flexDirection: 'column', color: 'var(--text-secondary)', gap: '0.5rem',
        }}>
          <div style={{ fontSize: '3rem' }}>💬</div>
          <p>Выберите диалог для начала общения</p>
        </div>
      );
    }

    const activeConv = conversations.find(c => c.id === activeId);

    return (
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', height: '100%', minWidth: 0 }}>
        {/* Chat header */}
        <div style={{
          padding: '0.75rem 1rem', borderBottom: '1px solid var(--border-color)',
          display: 'flex', alignItems: 'center', gap: '0.75rem',
        }}>
          <button
            onClick={() => setMobileShowChat(false)}
            className="btn chat-back-btn"
            style={{ padding: '0.3rem 0.6rem', fontSize: '0.85rem', display: 'none' }}
          >
            ←
          </button>
          {activeConv && (
            <>
              <div style={{
                width: '36px', height: '36px', borderRadius: '50%',
                background: 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff', fontWeight: 700, fontSize: '0.8rem', flexShrink: 0,
              }}>
                {getInitials(activeConv.otherUserName)}
              </div>
              <div>
                <div style={{ fontWeight: 600, fontSize: '0.95rem' }}>{activeConv.otherUserName}</div>
                {typingUser ? (
                  <div style={{ fontSize: '0.78rem', color: 'var(--primary-color)' }}>печатает...</div>
                ) : (
                  <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>{activeConv.otherUserRole === 'Tutor' ? 'Тьютор' : 'Студент'}</div>
                )}
              </div>
            </>
          )}
        </div>

        {/* Messages */}
        <div style={{ flex: 1, overflowY: 'auto', padding: '1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
          {loadingMsgs ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
          ) : messages.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
              Начните диалог — напишите первое сообщение 👋
            </div>
          ) : (
            messages.map(msg => (
              <div
                key={msg.id}
                style={{
                  display: 'flex',
                  justifyContent: msg.isMine ? 'flex-end' : 'flex-start',
                }}
              >
                <div style={{
                  maxWidth: '70%',
                  padding: '0.6rem 0.9rem',
                  borderRadius: msg.isMine ? '16px 16px 4px 16px' : '16px 16px 16px 4px',
                  background: msg.type === 'System'
                    ? 'var(--bg-secondary)'
                    : msg.isMine
                      ? 'var(--primary-color)'
                      : 'var(--bg-secondary)',
                  color: msg.type === 'System'
                    ? 'var(--text-secondary)'
                    : msg.isMine ? '#fff' : 'var(--text-primary)',
                  fontSize: msg.type === 'System' ? '0.78rem' : '0.9rem',
                  textAlign: msg.type === 'System' ? 'center' as const : undefined,
                  fontStyle: msg.type === 'System' ? 'italic' : undefined,
                }}>
                  {!msg.isMine && msg.type !== 'System' && (
                    <div style={{ fontSize: '0.75rem', fontWeight: 600, marginBottom: '0.2rem', opacity: 0.8 }}>
                      {msg.senderName}
                    </div>
                  )}
                  <div style={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>{msg.text}</div>
                  <div style={{
                    fontSize: '0.68rem', marginTop: '0.2rem',
                    opacity: 0.6, textAlign: 'right',
                  }}>
                    {formatTime(msg.sentAt)}
                    {msg.isMine && (
                      <span style={{ marginLeft: '0.3rem' }}>
                        {msg.readAt ? '✓✓' : '✓'}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            ))
          )}
          <div ref={messagesEndRef} />
        </div>

        {/* Input */}
        <form
          onSubmit={handleSend}
          style={{
            padding: '0.75rem 1rem', borderTop: '1px solid var(--border-color)',
            display: 'flex', gap: '0.5rem',
          }}
        >
          <input
            type="text"
            value={messageText}
            onChange={(e) => setMessageText(e.target.value)}
            onKeyDown={handleTyping}
            placeholder="Введите сообщение..."
            className="form-input"
            style={{ flex: 1, padding: '0.6rem 0.85rem', fontSize: '0.9rem' }}
            autoFocus
          />
          <button
            type="submit"
            className="btn btn-primary"
            disabled={sending || !messageText.trim()}
            style={{ padding: '0.6rem 1.25rem', fontSize: '0.9rem' }}
          >
            {sending ? '...' : '➤'}
          </button>
        </form>
      </div>
    );
  };

  return (
    <div className="animate-fade-in" style={{ height: 'calc(100vh - 120px)' }}>
      <div className="card" style={{
        height: '100%', padding: 0, display: 'flex', overflow: 'hidden',
      }}>
        {renderConversationList()}
        {renderChatArea()}
      </div>

      {/* Responsive overrides */}
      <style>{`
        @media (max-width: 768px) {
          .chat-sidebar { width: 100% !important; min-width: 100% !important; border-right: none !important; display: ${mobileShowChat ? 'none' : 'flex'} !important; }
          .chat-back-btn { display: inline-flex !important; }
        }
      `}</style>
    </div>
  );
}

export default MessagesPage;
