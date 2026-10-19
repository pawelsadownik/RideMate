import { useState, useEffect } from 'react';
import {
  View, Text, StyleSheet, SafeAreaView, FlatList, TouchableOpacity, Image, ActivityIndicator,
} from 'react-native';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { useThemeColors } from '@/design-system/useThemeColors';
import { THEME } from '@/design-system/theme';
import { useTranslation } from 'react-i18next';

export default function Matches() {
  const colors = useThemeColors();
  const { t } = useTranslation();
  const styles = makeStyles(colors);

  const { data, isLoading } = useQuery({
    queryKey: ['matches'],
    queryFn: async () => {
      const res = await api.get('/api/matches');
      return res.data as any[];
    },
  });

  if (isLoading) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.accent} />
      </SafeAreaView>
    );
  }

  const matches = data ?? [];

  if (matches.length === 0) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <Text style={[styles.title, { color: colors.primary }]}>{t('matches.title')}</Text>
        <Text style={[styles.sub, { color: colors.secondary }]}>{t('matches.empty')}</Text>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
      <Text style={[styles.title, { color: colors.primary }]}>{t('matches.title')}</Text>
      <FlatList
        contentContainerStyle={styles.listContent}
        data={matches}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <TouchableOpacity style={styles.matchRow}>
            <Image source={{ uri: item.otherPrimaryPhotoUrl }} style={styles.avatar} />
            <View style={styles.matchInfo}>
              <Text style={styles.matchName}>{item.otherNickname}, {item.otherAge}</Text>
              <Text style={styles.matchStatus}>{item.unreadCount > 0 ? t('common.unread', { count: item.unreadCount }) : ''}</Text>
            </View>
          </TouchableOpacity>
        )}
      />
    </SafeAreaView>
  );
}

function makeStyles(colors: ReturnType<typeof useThemeColors>) {
  return StyleSheet.create({
    container: { flex: 1, padding: 16 },
    listContent: { paddingBottom: 20 },
    title: { fontSize: 24, fontWeight: '800', color: colors.primary },
    sub: { fontSize: 16, color: colors.secondary, marginTop: 8 },
    matchRow: {
      flexDirection: 'row', alignItems: 'center',
      paddingVertical: 14, borderBottomWidth: 1, borderBottomColor: colors.border,
    },
    avatar: { width: 56, height: 56, borderRadius: 28, backgroundColor: colors.border },
    matchInfo: { marginLeft: 14, flex: 1 },
    matchName: { fontSize: 17, fontWeight: '700', color: colors.primary },
    matchStatus: { fontSize: 14, color: colors.accent, fontWeight: '700' },
  });
}
