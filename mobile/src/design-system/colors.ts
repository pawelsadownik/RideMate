export const COLORS = {
  // Akcent koralowy/czerwony (brand)
  accent: '#FF5E50',
  accentDark: '#E04B3E',
  accentLight: '#FFE7E3',

  // Tła
  backgroundLight: '#FFFFFF',
  backgroundDark: '#0B0C10',
  surfaceLight: '#F5F6F8',
  surfaceDark: '#14151A',

  // Tekst
  textPrimaryLight: '#1A1A1A',
  textPrimaryDark: '#F5F6F8',
  textSecondaryLight: '#6B6B73',
  textSecondaryDark: '#8A8C93',
  textMutedLight: '#9A9CA4',
  textMutedDark: '#5B5C63',

  // Obsada
  borderLight: '#E2E4E8',
  borderDark: '#2A2B31',

  // Statusy
  success: '#22C55E',
  error: '#EF4444',
  warning: '#F59E0B',
  info: '#3B82F6',

  white: '#FFFFFF',
  black: '#000000',
  transparent: 'transparent',
} as const;

export type ThemeColors = keyof typeof COLORS;
