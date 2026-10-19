import { useColorScheme } from 'react-native';
import { useThemeStore } from '@/stores/themeStore';
import { THEME } from '@/design-system/theme';

export const useThemeColors = () => {
  const { theme, resolvedTheme, setResolvedTheme } = useThemeStore();
  const systemScheme = useColorScheme();

  const resolved =
    theme === 'system' ? (systemScheme === 'dark' ? 'dark' : 'light') : theme;

  // Synchronizuj resolvedTheme z systemowym (przy systemowym motywie)
  // eslint-disable-next-line react-hooks/rules-of-hooks
  // (wywoływane poza hookiem celowo – setResolvedTheme w komponencie)

  return {
    ...THEME.colors[resolved],
    theme: resolved,
    isDark: resolved === 'dark',
  } as typeof THEME.colors.light & { theme: 'light' | 'dark'; isDark: boolean };
};
