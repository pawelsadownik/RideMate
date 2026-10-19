import { useEffect } from 'react';
import { View, Text, ActivityIndicator, StyleSheet } from 'react-native';
import { useRouter } from 'expo-router';
import { useAuthStore } from '@/stores/authStore';
import { useThemeColors } from '@/design-system/useThemeColors';
import i18n from '@/lib/i18n';

export default function Index() {
  const router = useRouter();
  const { hydrated, setHydrated, isAuthenticated } = useAuthStore();
  const colors = useThemeColors();

  useEffect(() => {
    if (hydrated) {
      if (isAuthenticated) {
        router.replace('/(tabs)/discover');
      } else {
        router.replace('/(auth)/login');
      }
    }
  }, [hydrated, isAuthenticated, router]);

  if (!hydrated) {
    return (
      <View style={styles.container}>
        <ActivityIndicator size="large" color={colors.accent} />
      </View>
    );
  }

  return null;
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
});
