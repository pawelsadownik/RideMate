import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import * as SecureStore from 'expo-secure-store';

export interface ThemeState {
  theme: 'light' | 'dark' | 'system';
  resolvedTheme: 'light' | 'dark';
  setTheme: (theme: 'light' | 'dark' | 'system') => void;
  setResolvedTheme: (theme: 'light' | 'dark') => void;
}

const secureStore = {
  getItem: async (name: string) => await SecureStore.getItemAsync(name),
  setItem: async (name: string, value: string) => await SecureStore.setItemAsync(name, value),
  removeItem: async (name: string) => await SecureStore.deleteItemAsync(name),
};

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
