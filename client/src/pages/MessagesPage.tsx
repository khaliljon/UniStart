import { useState, useEffect, useRef, useCallback, Fragment } from 'react';
import { useSearchParams } from 'react-router-dom';
import { messageService } from '../services/messageService';
import { chatService } from '../services/chatService';
import { userService, type PresenceInfo } from '../services/userService';
import type { Conversation, Message } from '../types';
import { getDateLocale } from '../i18n';

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
  const [showArchived, setShowArchived] = useState(false);

  // Presence state: userId → PresenceInfo
  const [presenceMap, setPresenceMap] = useState<Record<number, PresenceInfo>>({});

  // Scroll-to-bottom + lazy-load
  const [showScrollBtn, setShowScrollBtn] = useState(false);
  const [loadingOlder, setLoadingOlder] = useState(false);
  const [hasMore, setHasMore] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);

  // Notification permission
  const [notifPermission, setNotifPermission] = useState<NotificationPermission>(
    typeof Notification !== 'undefined' ? Notification.permission : 'denied'
  );

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const messagesContainerRef = useRef<HTMLDivElement>(null);
  const typingTimeout = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  // Scroll-to-unread tracking
  const [firstUnreadMsgId, setFirstUnreadMsgId] = useState<number | null>(null);
  const firstUnreadRef = useRef<HTMLDivElement>(null);
  const isInitialLoadRef = useRef(false);

  // ─── Load conversations ────────────────────
  const loadConversations = useCallback(async () => {
    try {
      const data = await messageService.getConversations();
      setConversations(data);

      // Batch-load presence for all conversation partners
      const userIds = data.map(c => c.otherUserId);
      if (userIds.length > 0) {
        try {
          const batch = await userService.getPresenceBatch(userIds);
          const map: Record<number, PresenceInfo> = {};
          batch.forEach(p => { map[p.userId] = { isOnline: p.isOnline, lastSeenAt: null }; });
          setPresenceMap(prev => ({ ...prev, ...map }));
        } catch { /* ignore */ }
      }
    } catch (err) {
      console.error('Failed to load conversations:', err);
    } finally {
      setLoadingConvs(false);
    }
  }, []);

  // ─── Load messages for active conversation ──
  const loadMessages = useCallback(async (convId: number, unreadCount = 0) => {
    setLoadingMsgs(true);
    setCurrentPage(1);
    isInitialLoadRef.current = true;
    try {
      const data = await messageService.getMessages(convId, 1, 50);
      const sorted = data.items.reverse();
      setMessages(sorted);

      // Track first unread message for scroll positioning
      if (unreadCount > 0 && sorted.length >= unreadCount) {
        setFirstUnreadMsgId(sorted[sorted.length - unreadCount].id);
      } else {
        setFirstUnreadMsgId(null);
      }

      setHasMore(data.hasMore);
      await messageService.markAsRead(convId);
      // Also notify via SignalR so ProfileDropdown badge updates
      chatService.markAsRead(convId).catch(() => {});
      setConversations(prev => prev.map(c =>
        c.id === convId ? { ...c, unreadCount: 0 } : c
      ));
    } catch (err) {
      console.error('Failed to load messages:', err);
    } finally {
      setLoadingMsgs(false);
    }
  }, []);

  // ─── Lazy-load older messages ───────────────
  const loadOlderMessages = useCallback(async () => {
    if (!activeId || loadingOlder || !hasMore) return;
    setLoadingOlder(true);
    const nextPage = currentPage + 1;
    try {
      const data = await messageService.getMessages(activeId, nextPage, 50);
      const older = data.items.reverse();
      setMessages(prev => [...older, ...prev]);
      setHasMore(data.hasMore);
      setCurrentPage(nextPage);
    } catch (err) {
      console.error('Failed to load older messages:', err);
    } finally {
      setLoadingOlder(false);
    }
  }, [activeId, loadingOlder, hasMore, currentPage]);

  // ─── Load presence for active conversation partner ──
  const loadPresence = useCallback(async (userId: number) => {
    try {
      const info = await userService.getPresence(userId);
      setPresenceMap(prev => ({ ...prev, [userId]: info }));
    } catch { /* ignore */ }
  }, []);

  // ─── Initialize SignalR ─────────────────────
  useEffect(() => {
    chatService.start();

    const unsubMsg = chatService.onMessage((msg: Message) => {
      setActiveId(currentActiveId => {
        if (msg.conversationId === currentActiveId) {
          setMessages(prev => {
            // Deduplicate by message ID to prevent duplicates from orphaned connections
            if (prev.some(m => m.id === msg.id)) return prev;
            return [...prev, msg];
          });
        }
        return currentActiveId;
      });
      setConversations(prev => prev.map(c => {
        if (c.id === msg.conversationId) {
          return {
            ...c,
            lastMessagePreview: msg.text.slice(0, 100),
            lastMessageAt: msg.sentAt,
            unreadCount: msg.isMine ? c.unreadCount : c.unreadCount + 1,
          };
        }
        return c;
      }));

      // Web Notification for incoming messages
      if (!msg.isMine && notifPermission === 'granted' && document.hidden) {
        try {
          new Notification(`${msg.senderName}`, {
            body: msg.text.slice(0, 80),
            icon: '/favicon.ico',
            tag: `msg-${msg.conversationId}`,
          });
        } catch { /* ignore */ }
      }
    });

    const unsubTyping = chatService.onTyping((convId, userName) => {
      setActiveId(currentActiveId => {
        if (convId === currentActiveId) {
          setTypingUser(userName);
          if (typingTimeout.current) clearTimeout(typingTimeout.current);
          typingTimeout.current = setTimeout(() => setTypingUser(''), 3000);
        }
        return currentActiveId;
      });
    });

    const unsubRead = chatService.onRead((_convId) => {
      setMessages(prev => prev.map(m =>
        m.isMine && !m.readAt ? { ...m, readAt: new Date().toISOString() } : m
      ));
    });

    // Presence events
    const unsubOnline = chatService.onOnline((userId) => {
      setPresenceMap(prev => ({ ...prev, [userId]: { isOnline: true, lastSeenAt: new Date().toISOString() } }));
    });

    const unsubOffline = chatService.onOffline((userId) => {
      setPresenceMap(prev => ({ ...prev, [userId]: { isOnline: false, lastSeenAt: new Date().toISOString() } }));
    });

    // Conversation status changes (enrollment accept/decline)
    const unsubStatus = chatService.onStatusChanged((conversationId, newStatus) => {
      setConversations(prev => prev.map(c =>
        c.id === conversationId ? { ...c, status: newStatus } : c
      ));
    });

    return () => {
      unsubMsg();
      unsubTyping();
      unsubRead();
      unsubOnline();
      unsubOffline();
      unsubStatus();
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [notifPermission]);

  useEffect(() => { loadConversations(); }, [loadConversations]);

  useEffect(() => {
    if (activeId) {
      const conv = conversations.find(c => c.id === activeId);
      loadMessages(activeId, conv?.unreadCount ?? 0);
      setSearchParams({ c: String(activeId) }, { replace: true });
      if (conv) loadPresence(conv.otherUserId);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeId, loadMessages, setSearchParams]);

  // Auto scroll: on initial load → first unread; on new messages → bottom if near bottom
  useEffect(() => {
    if (loadingMsgs || messages.length === 0) return;

    if (isInitialLoadRef.current) {
      isInitialLoadRef.current = false;
      requestAnimationFrame(() => {
        if (firstUnreadRef.current) {
          firstUnreadRef.current.scrollIntoView({ behavior: 'instant', block: 'start' });
        } else {
          messagesEndRef.current?.scrollIntoView({ behavior: 'instant' });
        }
      });
      return;
    }

    // New incoming messages: scroll to bottom only if already near bottom
    if (!showScrollBtn) {
      messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }
  }, [messages, showScrollBtn, loadingMsgs]);

  // ─── Scroll detection for scroll-to-bottom button ──
  const handleScroll = useCallback(() => {
    const el = messagesContainerRef.current;
    if (!el) return;
    const nearBottom = el.scrollHeight - el.scrollTop - el.clientHeight < 150;
    setShowScrollBtn(!nearBottom);

    // Lazy-load trigger: near top
    if (el.scrollTop < 80 && hasMore && !loadingOlder) {
      loadOlderMessages();
    }
  }, [hasMore, loadingOlder, loadOlderMessages]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    setShowScrollBtn(false);
  };

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!messageText.trim() || !activeId) return;
    setSending(true);
    try {
      await chatService.sendMessage(activeId, messageText.trim());
      setMessageText('');
    } catch {
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
    if (activeId) chatService.sendTyping(activeId);
  };

  const selectConversation = (id: number) => {
    setActiveId(id);
    setMobileShowChat(true);
  };

  const handleArchive = async (convId: number) => {
    if (!confirm('Архивировать этот диалог?')) return;
    try {
      await messageService.archiveConversation(convId);
      setConversations(prev => prev.map(c =>
        c.id === convId ? { ...c, status: 'Archived' } : c
      ));
      if (activeId === convId) setActiveId(null);
    } catch { alert('Не удалось архивировать'); }
  };

  const requestNotificationPermission = async () => {
    if (typeof Notification === 'undefined') return;
    const perm = await Notification.requestPermission();
    setNotifPermission(perm);
  };

  // ─── Helpers ────────────────────────────────
  const formatTime = (dateStr: string) => {
    const d = new Date(dateStr);
    const now = new Date();
    const isToday = d.toDateString() === now.toDateString();
    const dl = getDateLocale();
    if (isToday) return d.toLocaleTimeString(dl, { hour: '2-digit', minute: '2-digit' });
    return d.toLocaleDateString(dl, { day: 'numeric', month: 'short' }) + ' ' +
           d.toLocaleTimeString(dl, { hour: '2-digit', minute: '2-digit' });
  };

  const formatLastSeen = (dateStr: string | null): string => {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    if (diffMin < 1) return 'только что';
    if (diffMin < 60) return `${diffMin} мин назад`;
    const diffH = Math.floor(diffMin / 60);
    if (diffH < 24) return `${diffH} ч назад`;
    const diffD = Math.floor(diffH / 24);
    return `${diffD} д назад`;
  };

  const getInitials = (name: string) =>
    name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);

  // ─── Filtered conversations ─────────────────
  const filteredConvs = conversations.filter(c =>
    showArchived ? c.status === 'Archived' : c.status !== 'Archived'
  );

  // ─── Presence dot component ─────────────────
  const PresenceDot = ({ userId, size = 10 }: { userId: number; size?: number }) => {
    const p = presenceMap[userId];
    const online = p?.isOnline ?? false;
    return (
      <span style={{
        display: 'inline-block',
        width: `${size}px`, height: `${size}px`,
        borderRadius: '50%',
        background: online ? '#22c55e' : '#9ca3af',
        border: '2px solid var(--bg-primary)',
        position: 'absolute',
        bottom: 0, right: 0,
      }} title={online ? 'Онлайн' : 'Офлайн'} />
    );
  };

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
        padding: '0.75rem 1rem', borderBottom: '1px solid var(--border-color)',
        display: 'flex', justifyContent: 'space-between', alignItems: 'center',
      }}>
        <span style={{ fontWeight: 700, fontSize: '1.1rem' }}>Сообщения</span>
        <div style={{ display: 'flex', gap: '0.25rem' }}>
          {notifPermission !== 'granted' && typeof Notification !== 'undefined' && (
            <button
              onClick={requestNotificationPermission}
              className="btn"
              style={{ padding: '0.2rem 0.5rem', fontSize: '0.75rem' }}
              title="Включить уведомления"
            >
              Уведомления
            </button>
          )}
          <button
            onClick={() => setShowArchived(!showArchived)}
            className="btn"
            style={{
              padding: '0.2rem 0.5rem', fontSize: '0.75rem',
              background: showArchived ? 'var(--primary-color)' : undefined,
              color: showArchived ? '#fff' : undefined,
            }}
            title={showArchived ? 'Показать активные' : 'Показать архив'}
          >
            Архив
          </button>
        </div>
      </div>
      <div style={{ flex: 1, overflowY: 'auto' }}>
        {loadingConvs ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>Загрузка...</div>
        ) : filteredConvs.length === 0 ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
            <div style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>{showArchived ? '' : ''}</div>
            {showArchived ? 'Нет архивных диалогов' : 'Нет диалогов'}
          </div>
        ) : (
          filteredConvs.map(conv => (
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
              <div style={{ position: 'relative', flexShrink: 0 }}>
                <div style={{
                  width: '42px', height: '42px', borderRadius: '50%',
                  background: 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  color: '#fff', fontWeight: 700, fontSize: '0.85rem',
                }}>
                  {getInitials(conv.otherUserName)}
                </div>
                <PresenceDot userId={conv.otherUserId} size={10} />
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
                    {conv.status === 'Pending' ? 'Ожидает ответа'
                      : conv.status === 'Declined' ? 'Отклонено'
                      : conv.status === 'Archived' ? 'Архив'
                      : conv.lastMessagePreview || 'Начните диалог'}
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
          <div style={{ fontSize: '3rem' }}></div>
          <p>Выберите диалог для начала общения</p>
        </div>
      );
    }

    const activeConv = conversations.find(c => c.id === activeId);
    const otherPresence = activeConv ? presenceMap[activeConv.otherUserId] : null;
    const isOtherOnline = otherPresence?.isOnline ?? false;

    return (
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', height: '100%', minWidth: 0, overflow: 'hidden' }}>
        {/* Chat header — fixed, never scrolls */}
        <div style={{
          padding: '0.75rem 1rem', borderBottom: '1px solid var(--border-color)',
          display: 'flex', alignItems: 'center', gap: '0.75rem',
          background: 'var(--bg-primary)', flexShrink: 0, zIndex: 5,
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
              <div style={{ position: 'relative', flexShrink: 0 }}>
                <div style={{
                  width: '36px', height: '36px', borderRadius: '50%',
                  background: 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  color: '#fff', fontWeight: 700, fontSize: '0.8rem',
                }}>
                  {getInitials(activeConv.otherUserName)}
                </div>
                <PresenceDot userId={activeConv.otherUserId} size={9} />
              </div>
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 600, fontSize: '0.95rem' }}>{activeConv.otherUserName}</div>
                {typingUser ? (
                  <div style={{ fontSize: '0.78rem', color: 'var(--primary-color)' }}>печатает...</div>
                ) : isOtherOnline ? (
                  <div style={{ fontSize: '0.78rem', color: '#22c55e' }}>Онлайн</div>
                ) : otherPresence?.lastSeenAt ? (
                  <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                    Был(а) в сети {formatLastSeen(otherPresence.lastSeenAt)}
                  </div>
                ) : (
                  <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                    {activeConv.otherUserRole === 'Tutor' ? 'Тьютор' : 'Студент'}
                  </div>
                )}
              </div>
              {/* Archive button */}
              {activeConv.status === 'Active' && (
                <button
                  onClick={() => handleArchive(activeConv.id)}
                  className="btn"
                  style={{ padding: '0.3rem 0.6rem', fontSize: '0.8rem' }}
                  title="Архивировать диалог"
                >
                  Архив
                </button>
              )}
            </>
          )}
        </div>

        {/* Messages */}
        <div
          ref={messagesContainerRef}
          onScroll={handleScroll}
          style={{ flex: 1, overflowY: 'auto', padding: '1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem', position: 'relative', minHeight: 0 }}
        >
          {/* Load older button */}
          {hasMore && (
            <div style={{ textAlign: 'center', padding: '0.5rem' }}>
              <button
                onClick={loadOlderMessages}
                disabled={loadingOlder}
                className="btn"
                style={{ padding: '0.3rem 0.8rem', fontSize: '0.8rem' }}
              >
                {loadingOlder ? 'Загрузка...' : 'Загрузить старые сообщения'}
              </button>
            </div>
          )}

          {loadingMsgs ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
          ) : messages.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
              Начните диалог — напишите первое сообщение
            </div>
          ) : (
            messages.map(msg => (
              <Fragment key={msg.id}>
                {msg.id === firstUnreadMsgId && (
                  <div ref={firstUnreadRef} style={{
                    display: 'flex', alignItems: 'center', gap: '0.75rem',
                    padding: '0.5rem 0', margin: '0.25rem 0',
                  }}>
                    <div style={{ flex: 1, height: '1px', background: 'var(--primary-color)', opacity: 0.5 }} />
                    <span style={{ fontSize: '0.75rem', color: 'var(--primary-color)', fontWeight: 600, whiteSpace: 'nowrap' }}>
                      Новые сообщения
                    </span>
                    <div style={{ flex: 1, height: '1px', background: 'var(--primary-color)', opacity: 0.5 }} />
                  </div>
                )}
                <div
                  style={{
                    display: 'flex',
                    justifyContent: msg.type === 'System' ? 'center' : msg.isMine ? 'flex-end' : 'flex-start',
                  }}
                >
                <div style={{
                  maxWidth: '70%',
                  padding: '0.6rem 0.9rem',
                  borderRadius: msg.type === 'System' ? '12px' : msg.isMine ? '16px 16px 4px 16px' : '16px 16px 16px 4px',
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
              </Fragment>
            ))
          )}
          <div ref={messagesEndRef} />
        </div>

        {/* Scroll-to-bottom button */}
        {showScrollBtn && (
          <div style={{ position: 'relative' }}>
            <button
              onClick={scrollToBottom}
              style={{
                position: 'absolute', bottom: '0.5rem', right: '1rem',
                width: '36px', height: '36px', borderRadius: '50%',
                background: 'var(--primary-color)', color: '#fff',
                border: 'none', cursor: 'pointer', fontSize: '1.1rem',
                boxShadow: '0 2px 8px rgba(0,0,0,0.2)',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                zIndex: 10,
              }}
              title="Прокрутить вниз"
            >
              ↓
            </button>
          </div>
        )}

        {/* Input */}
        {activeConv && (activeConv.status === 'Pending' || activeConv.status === 'Declined' || activeConv.status === 'Archived') ? (
          <div style={{
            padding: '1rem', borderTop: '1px solid var(--border-color)',
            textAlign: 'center', color: 'var(--text-secondary)', fontSize: '0.85rem',
            background: 'var(--bg-secondary)',
          }}>
            {activeConv.status === 'Pending'
              ? 'Тьютор ещё не принял заявку. Сообщения будут доступны после принятия.'
              : activeConv.status === 'Declined'
                ? 'Заявка отклонена. Отправка сообщений невозможна.'
                : 'Диалог архивирован.'}
          </div>
        ) : (
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
        )}
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
