import { useState, useCallback, useEffect } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, SafeAreaView, Platform, Alert,
} from 'react-native';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { SwipeCard } from '@/components/SwipeCard';
import { ProfilePhotoModal } from '@/components/ProfilePhotoModal';
import { useThemeColors } from '@/design-system/useThemeColors';
import { THEME } from '@/design-system/theme';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/stores/authStore';

export default function Discover() {
  const colors = useThemeColors();
  const { t } = useTranslation();
  const { isPremium } = useAuthStore();
  const styles = makeStyles(colors);

  const [profiles, setProfiles] = useState<any[]>([]);
  const [index, setIndex] = useState(0);
  const [showPhotoModal, setShowPhotoModal] = useState(false);
  const [filters, setFilters] = useState({
    activityType: '',
    maxDistanceKm: 50,
    minAge: 18,
    maxAge: 60,
    lookingFor: '',
    ridingStyle: '',
  });
  const [refreshKey, setRefreshKey] = useState(0);

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['discovery', refreshKey, filters],
    queryFn: async () => {
      const params: Record<string, string> = {};
      if (filters.activityType) params.activityType = filters.activityType;
      if (filters.maxDistanceKm != null) params.maxDistanceKm = String(filters.maxDistanceKm);
      if (filters.minAge != null) params.minAge = String(filters.minAge);
      if (filters.maxAge != null) params.maxAge = String(filters.maxAge);
      if (filters.lookingFor) params.lookingFor = filters.lookingFor;
      if (filters.ridingStyle) params.ridingStyle = filters.ridingStyle;

      const queryString = Object.entries(params)
        .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`)
        .join('&');

      const res = await api.get(`/api/discovery?${queryString}`);
      return res.data as any[];
    },
    enabled: true,
    retry: 1,
    refetchOnWindowFocus: false,
  });

  useEffect(() => {
    if (data) {
      setProfiles(data);
      setIndex(0);
    }
  }, [data]);

  const currentProfile = profiles[index];

  const handleLike = useCallback(async (id: string) => {
    try {
      const res = await api.post('/api/discovery/like', { toUserId: id, like: true });
      if (res.data.isMatch) {
        Alert.alert(t('discover.match'), t('discover.matchSub', { name: currentProfile?.nickname }), [
          { text: t('discover.startChat'), onPress: () => { /* TODO: navigate to chat */ } },
          { text: t('discover.keepSwiping'), style: 'cancel' },
        ]);
      }
      setIndex((i) => i + 1);
    } catch (e: any) {
      if (e.response?.status === 400) {
        Alert.alert(t('common.error'), t('discover.dailyLimitReached'));
      } else {
        Alert.alert(t('common.error'), t('common.unknownError'));
      }
    }
  }, [currentProfile?.nickname, index]);

  const handlePass = useCallback(async (id: string) => {
    try {
      await api.post('/api/discovery/like', { toUserId: id, like: false });
      setIndex((i) => i + 1);
    } catch {
      setIndex((i) => i + 1);
    }
  }, []);

  if (isLoading) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <Text style={[styles.title, { color: colors.primary }]}>{t('discover.title')}</Text>
      </SafeAreaView>
    );
  }

  if (!currentProfile) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <Text style={[styles.title, { color: colors.primary }]}>{t('discover.title')}</Text>
        <Text style={[styles.sub, { color: colors.secondary }]}>{t('discover.noResults')}</Text>
        <TouchableOpacity
          style={[styles.filterBtn, { backgroundColor: colors.border }]}
          onPress={() => setFilters({ ...filters, maxDistanceKm: 100 })}
        >
          <Text style={[styles.filterText, { color: colors.accent }]}>{t('discover.filters')}</Text>
        </TouchableOpacity>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
      <View style={styles.header}>
        <Text style={styles.title}>{t('discover.title')}</Text>
        {!isPremium && (
          <View style={[styles.limitBadge, { backgroundColor: '#FEE2E2' }]}>
            <Text style={[styles.limitText, { color: '#EF4444' }]}>{t('discover.dailyLimitReached')}</Text>
          </View>
        )}
      </View>

      <SwipeCard
        profile={currentProfile}
        onLike={handleLike}
        onPass={handlePass}
        loading={isLoading}
      />

      <ProfilePhotoModal
        visible={showPhotoModal}
        onClose={() => setShowPhotoModal(false)}
        nickname={currentProfile.nickname}
        age={currentProfile.age}
        photos={currentProfile.photos}
      />

      <TouchableOpacity
        style={[styles.viewProfileBtn, { backgroundColor: colors.accentLight }]}
        onPress={() => { setShowPhotoModal(true); }}
      >
        <Text style={[styles.viewProfileText, { color: colors.accentDark }]}>{t('discover.viewProfile')}</Text>
      </TouchableOpacity>

      <View style={styles.filtersRow}>
        <TouchableOpacity style={[styles.filterChip, { backgroundColor: '#F5F6F8' }]} onPress={() => {}}>
          <Text style={styles.filterChipText}>🏍️ {t('profile.motorcycle')}</Text>
        </TouchableOpacity>
        <TouchableOpacity style={[styles.filterChip, { backgroundColor: '#F5F6F8' }]} onPress={() => {}}>
          <Text style={styles.filterChipText}>🚲 {t('profile.bicycle')}</Text>
        </TouchableOpacity>
        <TouchableOpacity style={[styles.filterChip, { backgroundColor: '#F5F6F8' }]} onPress={() => {}}>
          <Text style={styles.filterChipText}>⚙️ {t('discover.filters')}</Text>
        </TouchableOpacity>
      </View>

      {error && (
        <Text style={[styles.error, { color: colors.error }]}>{t('common.networkError')}</Text>
      )}
    </SafeAreaView>
  );
}

function makeStyles(colors: ReturnType<typeof useThemeColors>) {
  return StyleSheet.create({
    container: { flex: 1, padding: 16 },
    header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 },
    title: { fontSize: 24, fontWeight: '800' },
    limitBadge: {
      marginLeft: 12,
      paddingHorizontal: 10, paddingVertical: 5,
      borderRadius: 12,
    },
    limitText: { fontSize: 12, fontWeight: '700' },
    sub: { fontSize: 16, color: colors.secondary },
    filterBtn: {
      marginTop: 12,
      paddingHorizontal: 16, paddingVertical: 8,
      borderRadius: 20,
    },
    filterText: { fontSize: 14 },
    viewProfileBtn: {
      marginTop: 12,
      paddingVertical: 14,
      borderRadius: 14,
      alignItems: 'center',
    },
    viewProfileText: { fontSize: 15, fontWeight: '700' },
    filtersRow: { flexDirection: 'row', gap: 10, marginTop: 16 },
    filterChip: {
      paddingHorizontal: 14, paddingVertical: 8,
      borderRadius: 20,
    },
    filterChipText: { fontSize: 14 },
    error: { marginTop: 12, textAlign: 'center' },
  });
}
