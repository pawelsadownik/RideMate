import { useState } from 'react';
import {
  View, Text, StyleSheet, SafeAreaView, FlatList, TextInput, TouchableOpacity, ActivityIndicator,
} from 'react-native';
import { useRouter } from 'expo-router';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { useThemeColors } from '@/design-system/useThemeColors';
import { THEME } from '@/design-system/theme';
import { useTranslation } from 'react-i18next';

export default function Chat() {
  const router = useRouter();
  const colors = useThemeColors();
  const { t } = useTranslation();
  const styles = makeStyles(colors);

  const { data, isLoading } = useQuery({
    queryKey: ['conversations'],
    queryFn: async () => {
      const res = await api.get('/api/chat/conversations');
      return res.data as any[];
    },
    enabled: true,
    retry: 1,
  });

  const [text, setText] = useState('');

  if (isLoading) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.accent} />
      </SafeAreaView>
    );
  }

  const conversations = data ?? [];

  return (
    <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
      <Text style={[styles.title, { color: colors.primary }]}>{t('chat.title')}</Text>
      {conversations.length === 0 && (
        <Text style={[styles.sub, { color: colors.secondary }]}>{t('chat.empty')}</Text>
      )}
      <FlatList
        contentContainerStyle={styles.listContent}
        data={conversations}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <TouchableOpacity
            style={styles.row}
            onPress={() => router.push({
              pathname: '/(tabs)/chat/[conversationId]',
              params: { conversationId: item.id, otherNickname: item.otherNickname },
            })}
          >
            <View style={[styles.avatar, { backgroundColor: colors.border }]} />
            <View style={styles.info}>
              <Text style={[styles.name, { color: colors.primary }]}>{item.otherNickname ?? 'Użytkownik'}</Text>
              <Text style={[styles.preview, { color: colors.secondary }]}>{item.lastMessageText ?? ''}</Text>
            </View>
            {item.unreadCount > 0 && <Text style={[styles.time, { color: colors.accent }]}>{String(item.unreadCount)}</Text>}
          </TouchableOpacity>
        )}
      />
      <View style={styles.inputRow}>
        <TextInput
          style={[styles.input, { color: colors.primary, borderColor: colors.border }]}
          placeholder={t('chat.messagePlaceholder')}
          value={text}
          onChangeText={setText}
        />
        <TouchableOpacity style={[styles.sendBtn, { backgroundColor: colors.accent }]} onPress={() => {}}>
          <Text style={styles.sendText}>{t('common.send')}</Text>
        </TouchableOpacity>
      </View>
    </SafeAreaView>
  );
}

function makeStyles(colors: ReturnType<typeof useThemeColors>) {
  return StyleSheet.create({
    container: { flex: 1, padding: 16 },
    listContent: { paddingBottom: 20 },
    title: { fontSize: 24, fontWeight: '800', color: colors.primary },
    sub: { fontSize: 16, color: colors.secondary, marginTop: 8 },
    row: {
      flexDirection: 'row', alignItems: 'center',
      paddingVertical: 14, borderBottomWidth: 1, borderBottomColor: colors.border,
    },
    avatar: { width: 50, height: 50, borderRadius: 25, marginRight: 12 },
    info: { flex: 1 },
    name: { fontSize: 16, fontWeight: '700' },
    preview: { fontSize: 14 },
    time: { fontSize: 14, fontWeight: '700' },
    inputRow: { flexDirection: 'row', gap: 10, marginTop: 16 },
    input: {
      flex: 1, height: 48, borderWidth: 1, borderRadius: 12, paddingHorizontal: 14,
    },
    sendBtn: {
      width: 48, height: 48,
      borderRadius: 12, justifyContent: 'center', alignItems: 'center',
    },
    sendText: { fontWeight: '800' },
  });
}
