import { useEffect, useState, useCallback } from 'react';
import {
  View, Text, StyleSheet, SafeAreaView, FlatList, TextInput, TouchableOpacity, ActivityIndicator, Platform, ScrollView,
} from 'react-native';
import { useRouter, useLocalSearchParams } from 'expo-router';
import { useQuery, useMutation, QueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import {
  connect, disconnect, joinConversation, onMessage, offMessage, onConnectionStatus, getConnection,
} from '@/lib/signalr';
import { useThemeColors } from '@/design-system/useThemeColors';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/stores/authStore';

interface MessageItem {
  id: string;
  senderUserId: string;
  senderNickname: string;
  text: string;
  sentAt: string;
  isRead: boolean;
}

export default function ChatThread() {
  const router = useRouter();
  const params = useLocalSearchParams<{ conversationId?: string; otherNickname?: string }>();
  const conversationId = (params.conversationId as string) ?? '';
  const otherNickname = (params.otherNickname as string) ?? 'Użytkownik';
  const colors = useThemeColors();
  const { t } = useTranslation();
  const { token } = useAuthStore();
  const styles = makeStyles(colors);
  const [inputText, setInputText] = useState('');
  const [connected, setConnected] = useState(false);

  const { data: messages, isLoading: messagesLoading } = useQuery<MessageItem[]>({
    queryKey: ['messages', conversationId],
    queryFn: async () => {
      const res = await api.get(`/api/chat/conversations/${conversationId}/messages?page=1&pageSize=100`);
      return res.data as MessageItem[];
    },
    enabled: !!conversationId,
  });

  const sendMutation = useMutation({
    mutationFn: async (text: string) => {
      const res = await api.post(`/api/chat/conversations/${conversationId}/messages`, { text });
      return res.data as MessageItem;
    },
    onSuccess: () => {
      setInputText('');
    },
  });

  useEffect(() => {
    (async () => {
      if (token) {
        await connect(token);
        onConnectionStatus(setConnected);
        await joinConversation(conversationId);
      }
    })();

    const unsubMessage = () => {
      onMessage((msg: any) => {
        // sygnał o nowej wiadomości – odśwież listę
        queryClientRef.current.invalidateQueries({ queryKey: ['messages', conversationId] });
      });
    };

    return () => {
      offMessage();
      disconnect();
    };
  }, [token, conversationId]);

  const queryClientRef = { current: new QueryClient() };

  const handleSend = () => {
    if (!inputText.trim()) return;
    sendMutation.mutate(inputText);
  };

  const renderItem = ({ item }: { item: MessageItem }) => {
    const isMine = item.senderUserId === (useAuthStore.getState().userId ?? '');
    return (
      <View style={[styles.messageWrapper, isMine ? styles.mine : styles.theirs]}>
        <View style={[styles.bubble, isMine ? styles.mineBubble : styles.theirsBubble]}>
          <Text style={[styles.bubbleText, isMine ? styles.mineText : styles.theirsText]}>{item.text}</Text>
        </View>
        <Text style={[styles.time, isMine ? styles.mineTime : styles.theirsTime]}>
          {new Date(item.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
        </Text>
      </View>
    );
  };

  if (messagesLoading) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.accent} />
      </SafeAreaView>
    );
  }

  const msgList = messages ?? [];

  return (
    <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => router.back()}>
          <Text style={styles.backText}>‹</Text>
        </TouchableOpacity>
        <Text style={styles.title}>{otherNickname}</Text>
        <View style={styles.spacer} />
        <Text style={[styles.status, { color: connected ? colors.success : colors.muted }]}>
          {connected ? 'Połączono' : 'Łączenie...'}
        </Text>
      </View>

      <FlatList
        contentContainerStyle={styles.listContent}
        data={msgList}
        renderItem={renderItem}
        keyExtractor={(item) => item.id}
        inverted={false}
        keyboardShouldPersistTaps="handled"
      />

      <View style={styles.inputContainer}>
        <View style={styles.inputRow}>
          <TextInput
            style={[styles.input, { color: colors.primary, borderColor: colors.border }]}
            placeholder={t('chat.messagePlaceholder')}
            value={inputText}
            onChangeText={setInputText}
            onSubmitEditing={handleSend}
            multiline={false}
          />
          <TouchableOpacity style={[styles.sendBtn, { backgroundColor: colors.accent }]} onPress={handleSend} disabled={sendMutation.isPending}>
            <Text style={styles.sendText}>{t('common.send')}</Text>
          </TouchableOpacity>
        </View>
      </View>
    </SafeAreaView>
  );
}

function makeStyles(colors: ReturnType<typeof useThemeColors>) {
  return StyleSheet.create({
    container: { flex: 1 },
    header: {
      flexDirection: 'row', alignItems: 'center', paddingHorizontal: 12, paddingVertical: 10,
      borderBottomWidth: 1,
    },
    backBtn: { padding: 6 },
    backText: { fontSize: 22 },
    title: { flex: 1, fontSize: 18, fontWeight: '800', marginHorizontal: 8 },
    spacer: { width: 36 },
    status: { fontSize: 12, fontWeight: '700' },
    listContent: { padding: 10, paddingBottom: 80 },
    messageWrapper: { flexDirection: 'row', marginVertical: 6 },
    mine: { justifyContent: 'flex-end', alignItems: 'flex-end' },
    theirs: { justifyContent: 'flex-start', alignItems: 'flex-start' },
    bubble: { maxWidth: '75%', padding: 12, borderRadius: 16 },
    mineBubble: { backgroundColor: colors.accent, alignSelf: 'flex-end' },
    theirsBubble: { backgroundColor: '#F5F6F8', alignSelf: 'flex-start' },
    bubbleText: { fontSize: 15 },
    mineText: { color: '#FFFFFF' },
    theirsText: { color: '#1A1A1A' },
    time: { fontSize: 11, color: '#9A9CA4', marginTop: 2 },
    mineTime: { alignSelf: 'flex-end' },
    theirsTime: { alignSelf: 'flex-start' },
    inputContainer: { padding: 10, borderTopWidth: 1, borderColor: colors.border },
    inputRow: { flexDirection: 'row', gap: 8, alignItems: 'center' },
    input: {
      flex: 1, height: 44, borderWidth: 1, borderRadius: 22, paddingHorizontal: 14, color: '#1A1A1A',
    },
    sendBtn: { width: 44, height: 44, borderRadius: 22, justifyContent: 'center', alignItems: 'center' },
    sendText: { color: '#FFFFFF', fontWeight: '800' },
  });
}
