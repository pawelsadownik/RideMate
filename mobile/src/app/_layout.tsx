import { useEffect, useState } from 'react';
import { Platform, StatusBar, View } from 'react-native';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { Stack } from 'expo-router';
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from '@/lib/queryClient';
import { useThemeStore } from '@/stores/themeStore';
import { useThemeColors } from '@/design-system/useThemeColors';
import i18n from '@/lib/i18n';
import { useLocaleStore } from '@/stores/localeStore';
import { useAuthStore } from '@/stores/authStore';

export default function RootLayout() {
  const { hydrated: authHydrated, setHydrated: setAuthHydrated } = useAuthStore();
  const { theme, setResolvedTheme } = useThemeStore();
  const { locale, setLocale } = useLocaleStore();
  const colors = useThemeColors();

  useEffect(() => {
    // Ustawienia języka z store
    i18n.changeLanguage(locale);
  }, [locale]);

  if (!authHydrated) {
    return (
      <View style={{ flex: 1, backgroundColor: colors.background, justifyContent: 'center', alignItems: 'center' }}>
        <StatusBar barStyle={theme === 'dark' ? 'light-content' : 'dark-content'} />
      </View>
    );
  }

  return (
    <GestureHandlerRootView style={{ flex: 1 }}><QueryClientProvider client={queryClient}>
      <StatusBar barStyle={theme === 'dark' ? 'light-content' : 'dark-content'} />
      <View style={{ flex: 1, backgroundColor: colors.background }}>
        <Stack>
          <Stack.Screen
            name="(auth)"
            options={{ headerShown: false, animation: 'fade' }}
          />
          <Stack.Screen
            name="(tabs)"
            options={{ headerShown: false, animation: 'slide_from_right' }}
          />
          <Stack.Screen name="chat/[conversationId]" options={{ headerShown: false }} />
          <Stack.Screen name="index" options={{ headerShown: false }} />
        </Stack>
      </View>
    </QueryClientProvider></GestureHandlerRootView>
  );
}
