import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { sessionStorage } from '../lib/storage';

export interface ThemeState {
  theme: 'light' | 'dark' | 'system';
  resolvedTheme: 'light' | 'dark';
  setTheme: (theme: 'light' | 'dark' | 'system') => void;
  setResolvedTheme: (theme: 'light' | 'dark') => void;
}

const secureStore = sessionStorage;

export const useThemeStore = create<ThemeState>()(
  persist(
    (set) => ({
      theme: 'system',
      resolvedTheme: 'light',
      setTheme: (theme) => set({ theme }),
      setResolvedTheme: (theme) => set({ resolvedTheme: theme }),
    }),
    {
      name: 'ridemate-theme',
      storage: createJSONStorage(() => secureStore),
      partialize: (state) => ({ theme: state.theme }),
    }
  )
);
