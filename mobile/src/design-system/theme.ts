import { COLORS } from './colors';

export const THEME = {
  colors: {
    light: {
      background: COLORS.backgroundLight,
      surface: COLORS.surfaceLight,
      primary: COLORS.textPrimaryLight,
      secondary: COLORS.textSecondaryLight,
      muted: COLORS.textMutedLight,
      border: COLORS.borderLight,
      accent: COLORS.accent,
      accentDark: COLORS.accentDark,
      accentLight: COLORS.accentLight,
      success: COLORS.success,
      error: COLORS.error,
      warning: COLORS.warning,
      info: COLORS.info,
    },
    dark: {
      background: COLORS.backgroundDark,
      surface: COLORS.surfaceDark,
      primary: COLORS.textPrimaryDark,
      secondary: COLORS.textSecondaryDark,
      muted: COLORS.textMutedDark,
      border: COLORS.borderDark,
      accent: COLORS.accent,
      accentDark: COLORS.accentDark,
      accentLight: COLORS.accentLight,
      success: COLORS.success,
      error: COLORS.error,
      warning: COLORS.warning,
      info: COLORS.info,
    },
  },
  spacing: {
    xs: 4,
    sm: 8,
    md: 16,
    lg: 24,
    xl: 32,
  },
  radius: {
    sm: 8,
    md: 16,
    lg: 24,
    xl: 32,
    full: 9999,
  },
  fontSize: {
    xs: 12,
    sm: 14,
    md: 16,
    lg: 18,
    xl: 22,
    xxl: 28,
    xxxl: 36,
  },
  shadow: {
    sm: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 1 },
      shadowOpacity: 0.05,
      shadowRadius: 2,
      elevation: 1,
    },
    md: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 4 },
      shadowOpacity: 0.1,
      shadowRadius: 8,
      elevation: 4,
    },
  },
} as const;

export type ThemeName = 'light' | 'dark';
