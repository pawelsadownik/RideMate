import React, { useCallback } from 'react';
import {
  View, Text, Image, StyleSheet, Dimensions, TouchableOpacity, ActivityIndicator,
} from 'react-native';
import { PanGestureHandler, GestureHandlerRootView } from 'react-native-gesture-handler';
import Animated, {
  useSharedValue, useAnimatedGestureHandler, useAnimatedStyle,
  withSpring, withTiming, interpolate, runOnJS, cancelAnimation,
} from 'react-native-reanimated';

const AView = Animated.View as any;
import { useThemeColors } from '@/design-system/useThemeColors';
import { THEME } from '@/design-system/theme';
import { useTranslation } from 'react-i18next';

const { width: SCREEN_WIDTH } = Dimensions.get('window');
const CARD_WIDTH = SCREEN_WIDTH - 32;
const CARD_HEIGHT = SCREEN_WIDTH * 1.6;
const THRESHOLD = 100;

interface Photo {
  url: string;
  isPrimary: boolean;
}

interface SwipeCardProps {
  profile: {
    id: string;
    nickname: string;
    age: number;
    distanceKm: number;
    activityType: string;
    lookingFor: string;
    ridingStyle?: string | null;
    bio?: string | null;
    city?: string | null;
    photos: Photo[];
  };
  onLike: (id: string) => void;
  onPass: (id: string) => void;
  loading?: boolean;
}

export const SwipeCard: React.FC<SwipeCardProps> = ({
  profile, onLike, onPass, loading = false,
}) => {
  const colors = useThemeColors();
  const { t } = useTranslation();

  const pan = useSharedValue({ x: 0, y: 0 });
  const cardScale = useSharedValue(1);
  const cardRotation = useSharedValue(0);

  const gesture = useAnimatedGestureHandler({
    onStart: (_, ctx) => {
      (ctx as any).start = { x: pan.value.x, y: pan.value.y };
    },
    onActive: (e, ctx) => {
      pan.value.x = (ctx as any).start.x + e.translationX;
      pan.value.y = (ctx as any).start.y + e.translationY;
      cardRotation.value = pan.value.x / 20;
    },
    onEnd: (e) => {
      const velocityX = e.velocityX;
      const directionX = pan.value.x;

      if (directionX > THRESHOLD || velocityX > 600) {
        pan.value = withSpring({ x: SCREEN_WIDTH * 1.5, y: 0 });
        cardRotation.value = withSpring(30);
        cardScale.value = withSpring(0.6);
        runOnJS(onLike)(profile.id);
      } else if (directionX < -THRESHOLD || velocityX < -600) {
        pan.value = withSpring({ x: -SCREEN_WIDTH * 1.5, y: 0 });
        cardRotation.value = withSpring(-30);
        cardScale.value = withSpring(0.6);
        runOnJS(onPass)(profile.id);
      } else {
        pan.value = withSpring({ x: 0, y: 0 });
        cardRotation.value = withSpring(0);
        cardScale.value = withSpring(1);
      }
    },
  });

  const animatedStyle = useAnimatedStyle(() => ({
    transform: [
      { translateX: pan.value.x },
      { translateY: pan.value.y },
      { rotate: `${cardRotation.value}deg` },
      { scale: cardScale.value },
    ],
  }));

  const bgColor = interpolate(pan.value.x, [-SCREEN_WIDTH, 0, SCREEN_WIDTH], ['#EF4444', '#FFFFFF', '#22C55E'] as unknown as number[]);
  const likeOpacity = interpolate(pan.value.x, [-SCREEN_WIDTH, 0, SCREEN_WIDTH], [0, 0, 1] as unknown as number[]);
  const passOpacity = interpolate(pan.value.x, [-SCREEN_WIDTH, 0, SCREEN_WIDTH], [1, 0, 0] as unknown as number[]);

  const primaryPhoto = profile.photos.find(p => p.isPrimary) ?? profile.photos[0];

  const handleLike = useCallback(() => {
    pan.value = withSpring({ x: SCREEN_WIDTH * 1.5, y: 0 });
    cardRotation.value = withSpring(30);
    cardScale.value = withSpring(0.6);
    onLike(profile.id);
  }, [onLike, profile.id]);

  const handlePass = useCallback(() => {
    pan.value = withSpring({ x: -SCREEN_WIDTH * 1.5, y: 0 });
    cardRotation.value = withSpring(-30);
    cardScale.value = withSpring(0.6);
    onPass(profile.id);
  }, [onPass, profile.id]);

  if (loading) {
    return (
      <View style={[styles.container, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.accent} />
      </View>
    );
  }

  return (
    <GestureHandlerRootView style={styles.root}>
      <PanGestureHandler onGestureEvent={gesture} >
        <View style={[styles.card, { backgroundColor: colors.surface }]}>
          <AView style={animatedStyle}>
            {/* Tło reakcji (kolorowe przy swipe) */}
            <AView style={[styles.reactionBg, { backgroundColor: bgColor }]} />
            <AView style={[styles.passIcon, { opacity: passOpacity }]}>
              <Text style={styles.reactionText}>✕</Text>
            </AView>
            <AView style={[styles.likeIcon, { opacity: likeOpacity }]}>
              <Text style={styles.reactionText}>♥</Text>
            </AView>

            {/* Zdjęcie */}
            <View style={styles.imageContainer}>
              {primaryPhoto?.url ? (
                <Image
                  source={{ uri: primaryPhoto.url }}
                  style={styles.image}
                />
              ) : (
                <View style={[styles.image, { backgroundColor: colors.border }]}>
                  <Text style={[styles.imagePlaceholder, { color: colors.muted }]}>Brak zdjęcia</Text>
                </View>
              )}
              {profile.photos.length > 1 && (
                <View style={styles.photoCounter}>
                  <Text style={styles.photoCounterText}>{profile.photos.length}</Text>
                </View>
              )}
            </View>

            {/* Treść */}
            <View style={styles.content}>
              <View style={styles.headerRow}>
                <Text style={styles.name}>{profile.nickname}, {profile.age}</Text>
                <Text style={styles.distance}>{t('discover.kmAway', { km: Math.round(profile.distanceKm) })}</Text>
              </View>
              <View style={styles.tagsRow}>
                <Text style={styles.tag}>{profile.activityType}</Text>
                <Text style={styles.tag}>{profile.lookingFor}</Text>
                {profile.ridingStyle && <Text style={styles.tag}>{profile.ridingStyle}</Text>}
              </View>
              {profile.bio && (
                <View style={styles.bioContainer}>
                  <Text style={styles.bio}>{profile.bio}</Text>
                </View>
              )}
              {profile.city && (
                <Text style={styles.city}>{profile.city}</Text>
              )}
            </View>

            {/* Przyciski akcji (dla braku gestu / dostępność) */}
            <View style={styles.actions}>
              <TouchableOpacity
                style={[styles.actionBtn, styles.passBtn]}
                onPress={handlePass}
              >
                <Text style={styles.actionText}>{t('discover.pass')}</Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={[styles.actionBtn, styles.likeBtn]}
                onPress={handleLike}
              >
                <Text style={styles.actionText}>{t('discover.like')}</Text>
              </TouchableOpacity>
            </View>
          </AView>
        </View>
      </PanGestureHandler>
    </GestureHandlerRootView>
  );
};

const styles = StyleSheet.create({
  root: { flex: 1 },
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 16,
  },
  card: {
    width: CARD_WIDTH,
    height: CARD_HEIGHT,
    borderRadius: THEME.radius.lg,
    overflow: 'hidden',
    elevation: 8,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.16,
    shadowRadius: 16,
  },
  reactionBg: {
    position: 'absolute',
    top: 0, left: 0, right: 0, bottom: 0,
  },
  passIcon: {
    position: 'absolute',
    top: 40, left: 40,
    width: 80, height: 80,
    borderRadius: 40,
    backgroundColor: 'rgba(239, 68, 68, 0.2)',
    justifyContent: 'center', alignItems: 'center',
  },
  likeIcon: {
    position: 'absolute',
    top: 40, right: 40,
    width: 80, height: 80,
    borderRadius: 40,
    backgroundColor: 'rgba(34, 197, 94, 0.2)',
    justifyContent: 'center', alignItems: 'center',
  },
  reactionText: { fontSize: 40 },
  imageContainer: {
    ...StyleSheet.absoluteFill,
    justifyContent: 'flex-end',
  },
  image: {
    width: '100%',
    height: '100%',
  },
  photoCounter: {
    position: 'absolute',
    bottom: 12, left: 12,
    width: 32, height: 32,
    borderRadius: 16,
    backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'center', alignItems: 'center',
  },
  photoCounterText: { color: '#FFFFFF', fontSize: 14, fontWeight: '700' },
  imagePlaceholder: { color: '#9A9CA4', fontSize: 14 },
  content: {
    padding: 20,
    position: 'absolute',
    bottom: 0, left: 0, right: 0,
    paddingBottom: 80,
  },
  headerRow: {
    flexDirection: 'row', justifyContent: 'space-between', marginBottom: 4,
  },
  name: { fontSize: 24, fontWeight: '800', color: '#1A1A1A' },
  distance: { fontSize: 16, color: '#6B6B73' },
  tagsRow: { flexDirection: 'row', gap: 8, marginTop: 8, marginBottom: 12 },
  tag: {
    fontSize: 12, fontWeight: '700',
    backgroundColor: '#F5F6F8',
    color: '#6B6B73',
    paddingHorizontal: 8, paddingVertical: 4,
    borderRadius: 20,
  },
  bioContainer: { marginTop: 10 },
  bio: { fontSize: 15, color: '#1A1A1A', lineHeight: 22 },
  city: { fontSize: 14, color: '#6B6B73', marginTop: 6 },
  actions: {
    position: 'absolute',
    bottom: 30, left: 0, right: 0,
    flexDirection: 'row', justifyContent: 'center', gap: 20,
  },
  actionBtn: {
    width: 70, height: 70,
    borderRadius: 35,
    justifyContent: 'center', alignItems: 'center',
  },
  passBtn: { backgroundColor: '#EF4444' },
  likeBtn: { backgroundColor: '#22C55E' },
  actionText: { color: '#FFFFFF', fontSize: 14, fontWeight: '800' },
});
