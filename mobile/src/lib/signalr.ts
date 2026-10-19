import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';
import { useAuthStore } from '../stores/authStore';
import { API_URL } from './api';
import { Platform } from 'react-native';


let connection: HubConnection | null = null;
let currentUserId: string | null = null;
let currentConversationId: string | null = null;
let onMessageReceived: ((message: any) => void) | null = null;
let onConnectionChange: ((connected: boolean) => void) | null = null;

export async function getToken(): Promise<string | null> {
  return useAuthStore.getState().token;
}

export async function connect(userId: string): Promise<void> {
  if (connection) return;

  const token = await getToken();
  connection = new HubConnectionBuilder()
    .withUrl(`${API_URL}/chatHub`, {
      accessTokenFactory: () => useAuthStore.getState().token ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();

  connection.onreconnecting(() => {
    onConnectionChange?.(false);
  });

  connection.onreconnected(() => {
    onConnectionChange?.(true);
  });

  connection.onclose(() => {
    onConnectionChange?.(false);
  });

  try { await connection.start(); } catch (error) { connection = null; throw error; }
  if (onMessageReceived) connection.on('ReceiveMessage', onMessageReceived);
  onConnectionChange?.(true);
  currentUserId = userId;
}

export async function disconnect(): Promise<void> {
  if (!connection) return;
  try {
    if (currentConversationId) {
      await connection.invoke('LeaveConversation', currentConversationId);
    }
    await connection.stop();
  } catch {
    // ignore
  }
  connection = null;
  currentUserId = null;
  currentConversationId = null;
  onMessageReceived = null;
  onConnectionChange = null;
}

export async function joinConversation(conversationId: string): Promise<void> {
  if (!connection) return;
  if (currentConversationId) {
    await connection.invoke('LeaveConversation', currentConversationId);
  }
  await connection.invoke('JoinConversation', conversationId);
  currentConversationId = conversationId;
}

export async function sendMessage(conversationId: string, text: string, senderId: string): Promise<void> {
  if (!connection) return;
  // Wiadomość jest wysyłane przez REST; SignalR służy do push w czasie rzeczywistym
  // (odbieranie od drugiej strony przez grupę conversation:)
  return;
}

export function onMessage(callback: (message: any) => void): void {
  onMessageReceived = callback;
  if (connection) {
    connection.on('ReceiveMessage', callback);
  }
}

export function offMessage(): void {
  if (connection && onMessageReceived) {
    connection.off('ReceiveMessage', onMessageReceived);
    onMessageReceived = null;
  }
}

export function onConnectionStatus(callback: (connected: boolean) => void): void {
  onConnectionChange = callback;
}

export function getConnection(): HubConnection | null {
  return connection;
}
