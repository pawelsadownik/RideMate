import React from 'react';
import {
  View, Text, Image, StyleSheet, TouchableOpacity, SafeAreaView, Platform,
} from 'react-native';
import { useThemeColors } from '@/design-system/useThemeColors';
import { useTranslation } from 'react-i18next';

interface ProfilePhotoModalProps {
  visible: boolean;
  onClose: () => void;
  nickname: string;
  age: number;
  photos: { url: string; isPrimary: boolean }[];
}

export const ProfilePhotoModal: React.FC<ProfilePhotoModalProps> = ({
  visible, onClose, nickname, age, photos,
}) => {
  const colors = useThemeColors();
  const { t } = useTranslation();
  const styles = makeStyles(colors);

  if (!visible) return null;

  return (
    <SafeAreaView style={[styles.modal, { backgroundColor: colors.background }]}>
      <TouchableOpacity style={[styles.closeBtn, { backgroundColor: 'rgba(0,0,0,0.3)' }]} onPress={onClose}>
        <Text style={[styles.closeText, { color: '#FFFFFF' }]}>✕</Text>
      </TouchableOpacity>

      <View style={styles.header}>
        <Text style={[styles.name, { color: colors.primary }]}>{nickname}, {age}</Text>
        <Text style={[styles.sub, { color: colors.secondary }]}>{t('profile.photos')}</Text>
      </View>

      <View style={styles.gallery}>
        {photos.map((p, i) => (
          <View key={i} style={[styles.photoItem, { backgroundColor: colors.surface }]}>
            <Image source={{ uri: p.url }} style={styles.photo} />
            {p.isPrimary && <View style={[styles.primaryBadge, { backgroundColor: '#22C55E' }]} />}
          </View>
        ))}
      </View>

      <View style={styles.bottom}>
        <TouchableOpacity style={[styles.closeBtn, { backgroundColor: 'rgba(0,0,0,0.3)' }]} onPress={onClose}>
          <Text style={[styles.closeText, { color: '#FFFFFF' }]}>{t('common.close')}</Text>
        </TouchableOpacity>
      </View>
    </SafeAreaView>
  );
};

function makeStyles(colors: ReturnType<typeof useThemeColors>) {
  return StyleSheet.create({
    modal: { flex: 1, padding: 20 },
    closeBtn: {
      position: 'absolute', top: 8, right: 12,
      width: 40, height: 40,
      borderRadius: 20,
      justifyContent: 'center', alignItems: 'center',
    },
    closeText: { fontSize: 22 },
    header: { marginBottom: 20 },
    name: { fontSize: 24, fontWeight: '800' },
    sub: { fontSize: 16 },
    gallery: {
      flexDirection: 'row', flexWrap: 'wrap', gap: 10,
    },
    photoItem: {
      width: 110, height: 110,
      borderRadius: 16,
      overflow: 'hidden',
    },
    photo: { width: '100%', height: '100%' },
    primaryBadge: {
      position: 'absolute', bottom: 4, right: 4,
      width: 20, height: 20,
      borderRadius: 10,
      justifyContent: 'center', alignItems: 'center',
    },
    bottom: { marginTop: 20, paddingBottom: Platform.OS === 'android' ? 20 : 0 },
  });
}
