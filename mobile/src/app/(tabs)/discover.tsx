import { useRouter } from 'expo-router';
import { useState, useCallback, useEffect } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, SafeAreaView, Platform, Alert, Modal, Image, ActivityIndicator,
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
  const router = useRouter();
  const [match, setMatch] = useState<{ id: string; nickname: string; photo?: string } | null>(null);
  const [openingChat, setOpeningChat] = useState(false);
  const colors = useThemeColors();
  const { t } = useTranslation();
  const { isPremium } = useAuthStore();
  const styles = makeStyles(colors);

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
      return res.data.map((p: any) => ({ ...p, photos: (p.photoUrls ?? []).map((url: string, i: number) => ({ url, isPrimary: i === 0 })) })) as any[];
    },
    enabled: true,
    retry: 1,
    refetchOnWindowFocus: false,
  });

  const profiles = data ?? [];

  const currentProfile = profiles[index] ?? (match ? profiles[index - 1] : undefined);

  const handleLike = useCallback(async (id: string) => {
    try {
      const res = await api.post('/api/discovery/like', { toUserId: id, like: true });
      if (res.data.isMatch) {
        setMatch({ id: res.data.matchId, nickname: currentProfile.nickname, photo: currentProfile.primaryPhotoUrl });
      }
      setIndex((i) => i + 1);
    } catch (e: any) {
      if (e.response?.status === 400) {
        Alert.alert(t('common.error'), t('discover.dailyLimitReached'));
      } else {
        Alert.alert(t('common.error'), t('common.unknownError'));
      }
    }
  }, [currentProfile, t]);

  const handlePass = useCallback(async (id: string) => {
    try {
      await api.post('/api/discovery/like', { toUserId: id, like: false });
      setIndex((i) => i + 1);
    } catch {
      Alert.alert(t('common.error'), t('common.networkError'));
    }
  }, []);

  const startChat = async () => {
    if (!match || openingChat) return;
    setOpeningChat(true);
    try {
      const response = await api.get('/api/chat/conversations');
      const conversation = response.data.find((c: { matchId: string }) => c.matchId === match.id);
      if (!conversation) throw new Error('Conversation unavailable');
      setMatch(null);
      router.push({ pathname: '/chat/[conversationId]', params: { conversationId: conversation.id, otherNickname: match.nickname } });
    } catch { Alert.alert(t('common.error'), t('common.networkError')); }
    finally { setOpeningChat(false); }
  };

  if (error) return <SafeAreaView style={styles.container}><Text style={styles.title}>RideMate</Text><Text>{t('common.networkError')}</Text><TouchableOpacity onPress={() => void refetch()} style={styles.filterBtn}><Text>{t('common.retry', { defaultValue: 'Spróbuj ponownie' })}</Text></TouchableOpacity></SafeAreaView>;

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
      <Modal visible={!!match} transparent animationType="fade" onRequestClose={() => setMatch(null)}>
        <View style={{flex:1,backgroundColor:'rgba(12,13,16,.96)',justifyContent:'center',alignItems:'center',padding:28}}>
          <Text style={{fontSize:34,color:'#fff',fontWeight:'800',textAlign:'center'}}>{t('discover.match')}</Text>
          <Text style={{color:'#ddd',fontSize:16,textAlign:'center',marginTop:16}}>{t('discover.matchSub', {name:match?.nickname})}</Text>
          {!!match?.photo && <Image source={{uri:match.photo}} style={{width:150,height:150,borderRadius:75,marginVertical:30,borderWidth:3,borderColor:'#fff'}} />}
          <Text style={{fontSize:50,color:'#ff4564',marginVertical:20}}>♥</Text>
          <TouchableOpacity onPress={() => void startChat()} disabled={openingChat} style={{backgroundColor:'#ff4564',padding:18,borderRadius:28,width:'100%',alignItems:'center'}}><Text style={{color:'#fff',fontWeight:'700'}}>{t('discover.startChat')}</Text></TouchableOpacity>
          <TouchableOpacity onPress={() => setMatch(null)} style={{padding:18,marginTop:12}}><Text style={{color:'#fff'}}>{t('discover.keepSwiping')}</Text></TouchableOpacity>
        </View>
      </Modal>
      <View style={styles.header}>
        <Text style={styles.title}>{t('discover.title')}</Text>

      </View>

      <SwipeCard
        key={currentProfile.id}
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
        <TouchableOpacity style={[styles.filterChip, { backgroundColor: '#F5F6F8' }]} onPress={() => { setIndex(0); setFilters({ ...filters, activityType: filters.activityType === 'Motorcycle' ? '' : 'Motorcycle' }); }}>
          <Text style={styles.filterChipText}>🏍️ {t('profile.motorcycle')}</Text>
        </TouchableOpacity>
        <TouchableOpacity style={[styles.filterChip, { backgroundColor: '#F5F6F8' }]} onPress={() => { setIndex(0); setFilters({ ...filters, activityType: filters.activityType === 'Bicycle' ? '' : 'Bicycle' }); }}>
          <Text style={styles.filterChipText}>🚲 {t('profile.bicycle')}</Text>
        </TouchableOpacity>
        <TouchableOpacity style={[styles.filterChip, { backgroundColor: '#F5F6F8' }]} onPress={() => { setIndex(0); setFilters({ ...filters, maxDistanceKm: filters.maxDistanceKm === 50 ? 100 : 50 }); }}>
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
