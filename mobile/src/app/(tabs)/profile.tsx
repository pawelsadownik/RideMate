import { useEffect, useState } from 'react';
import {
  View, Text, StyleSheet, SafeAreaView, ScrollView, TouchableOpacity, Image, ActivityIndicator, Platform, Alert,
} from 'react-native';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { useThemeColors } from '@/design-system/useThemeColors';
import { THEME } from '@/design-system/theme';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/stores/authStore';

export default function ProfileScreen() {
  const colors = useThemeColors();
  const { t } = useTranslation();
  const { isPremium, logout } = useAuthStore();
  const styles = makeStyles(colors);

  const { data, isLoading } = useQuery({
    queryKey: ['myProfile'],
    queryFn: async () => {
      const res = await api.get('/api/profiles/me');
      return res.data as any;
    },
  });

  const handleDeleteAccount = async () => {
    const confirmed = await Alert.alert(
      t('auth.deleteConfirm'),
      '',
      [
        { text: t('common.cancel'), style: 'cancel' },
        { text: t('auth.deleteAccount'), style: 'destructive', onPress: async () => {
          try {
            await api.delete('/api/auth/account');
            logout();
          } catch {
            Alert.alert(t('common.error'), t('common.unknownError'));
          }
        } },
      ]
    );
  };

  if (isLoading) {
    return (
      <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.accent} />
      </SafeAreaView>
    );
  }

  const p = data;

  return (
    <SafeAreaView style={[styles.container, { backgroundColor: colors.background }]}>
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <View style={styles.header}>
          <Text style={[styles.title, { color: colors.primary }]}>{t('profile.title')}</Text>
          <Text style={[styles.subtitle, { color: colors.secondary }]}>{t('profile.editProfile')}</Text>
        </View>

        <View style={[styles.card, { backgroundColor: colors.surface }]}>
          <Image
            source={{ uri: p?.photos?.[0]?.url }}
            style={[styles.avatar, { backgroundColor: colors.border }]}
          />
          <View style={styles.nameAge}>
            <Text style={styles.name}>{p?.nickname}, {p?.age}</Text>
            <Text style={[styles.city, { color: colors.secondary }]}>{p?.city}</Text>
          </View>
          <Text style={[styles.bio, { color: colors.primary }]}>{p?.bio ?? 'Brak opisu.'}</Text>

          <View style={styles.sectionTitle}>{t('profile.activityType')}</View>
          <View style={styles.tagsRow}>
            <Text style={[styles.tag, { backgroundColor: '#F5F6F8', color: colors.secondary }]}>{p?.activityType}</Text>
            <Text style={[styles.tag, { backgroundColor: '#F5F6F8', color: colors.secondary }]}>{p?.lookingFor}</Text>
          </View>

          <View style={styles.sectionTitle}>{t('profile.photos')}</View>
          <View style={styles.photosRow}>
            {p?.photos?.length ? (
              p.photos.map((photo: any, i: number) => (
                <Image key={i} source={{ uri: photo.url }} style={[styles.photoThumb, { backgroundColor: colors.border }]} />
              ))
            ) : (
              <View style={[styles.photoThumbPlaceholder, { backgroundColor: colors.border }]} />
            )}
          </View>

          <View style={styles.sectionTitle}>{t('profile.ridingPreferences')}</View>
          <View style={styles.prefRow}>
            <Text style={styles.prefText}>{p?.ridingPreferences?.motorcycleType ?? '—'}</Text>
            <Text style={styles.prefText}>{p?.ridingPreferences?.ridesOwnMotorcycle ? 'Kierowca własnego' : '—'}</Text>
          </View>

          <View style={styles.sectionTitle}>{t('settings.darkMode')}</View>
          <TouchableOpacity style={styles.prefRow} onPress={() => {}}>
            <Text style={styles.prefText}>Przełącz motyw</Text>
          </TouchableOpacity>

          <View style={styles.sectionTitle}>{t('settings.notifications')}</View>
          <TouchableOpacity style={styles.prefRow} onPress={() => {}}>
            <Text style={styles.prefText}>{t('settings.notificationsDesc')}</Text>
          </TouchableOpacity>

          <View style={styles.sectionTitle}>{t('premium.title')}</View>
          <TouchableOpacity style={styles.prefRow} onPress={() => {}}>
            <Text style={styles.prefText}>{isPremium ? 'Aktywne' : 'Aktywuj Premium'}</Text>
          </TouchableOpacity>
          <Text style={styles.premiumHint}>{t('premium.devNote')}</Text>

          <TouchableOpacity style={[styles.prefRow, { backgroundColor: '#FEE2E2' }]} onPress={() => {
            Alert.alert(t('safety.block'), t('safety.blockReason'), [
              { text: t('common.cancel'), style: 'cancel' },
              { text: t('safety.block'), style: 'destructive', onPress: () => Alert.alert(t('safety.blocked')) },
            ]);
          }}>
            <Text style={[styles.prefText, { color: '#EF4444' }]}>{t('safety.block')}</Text>
          </TouchableOpacity>

          <TouchableOpacity style={styles.prefRow} onPress={() => {
            Alert.alert(t('safety.report'), t('safety.reportReason'), [
              { text: t('common.cancel'), style: 'cancel' },
              { text: t('safety.report'), style: 'destructive', onPress: () => Alert.alert(t('safety.success')) },
            ]);
          }}>
            <Text style={[styles.prefText, { color: colors.accent }]}>{t('safety.report')}</Text>
          </TouchableOpacity>

          <TouchableOpacity style={[styles.logoutBtn, { backgroundColor: '#FEE2E2' }]} onPress={handleDeleteAccount}>
            <Text style={[styles.logoutText, { color: '#EF4444' }]}>{t('auth.deleteAccount')}</Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

function makeStyles(colors: ReturnType<typeof useThemeColors>) {
  return StyleSheet.create({
    container: { flex: 1 },
    scrollContent: { padding: 16 },
    header: { marginBottom: 16 },
    title: { fontSize: 24, fontWeight: '800', color: colors.primary },
    subtitle: { fontSize: 14, color: colors.secondary },
    card: {
      borderRadius: THEME.radius.lg,
      padding: 20,
      shadowColor: '#000', shadowOffset: { width: 0, height: 2 }, shadowOpacity: 0.06, shadowRadius: 4,
      elevation: 2,
    },
    avatar: { width: 80, height: 80, borderRadius: 40 },
    nameAge: { marginTop: 12, alignItems: 'center' },
    name: { fontSize: 22, fontWeight: '800', color: colors.primary },
    city: { fontSize: 15 },
    bio: { fontSize: 15, marginTop: 10, lineHeight: 22 },
    sectionTitle: {
      fontSize: 14, fontWeight: '700',
      marginTop: 16, marginBottom: 8,
    },
    tagsRow: { flexDirection: 'row', gap: 8 },
    tag: { fontSize: 12, fontWeight: '700', paddingHorizontal: 8, paddingVertical: 4, borderRadius: 20 },
    photosRow: { flexDirection: 'row', gap: 8 },
    photoThumb: { width: 70, height: 70, borderRadius: 12 },
    photoThumbPlaceholder: { width: 70, height: 70, borderRadius: 12 },
    prefRow: {
      flexDirection: 'row', justifyContent: 'space-between',
      paddingVertical: 10, borderBottomWidth: 1, borderBottomColor: colors.border,
    },
    prefText: { fontSize: 15 },
    premiumHint: { fontSize: 12, color: colors.muted, marginTop: 4 },
    logoutBtn: {
      marginTop: 24,
      paddingVertical: 14,
      borderRadius: 14,
      alignItems: 'center',
    },
    logoutText: { fontSize: 15, fontWeight: '700' },
  });
}
