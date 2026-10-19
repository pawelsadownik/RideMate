import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import * as SecureStore from 'expo-secure-store';

export interface LocaleState {
  locale: 'pl' | 'en';
  setLocale: (locale: 'pl' | 'en') => void;
}

const secureStore = {
  getItem: async (name: string) => await SecureStore.getItemAsync(name),
  setItem: async (name: string, value: string) => await SecureStore.setItemAsync(name, value),
  removeItem: async (name: string) => await SecureStore.deleteItemAsync(name),
};

export const useLocaleStore = create<LocaleState>()(
  persist(
    (set) => ({
      locale: 'pl',
      setLocale: (locale) => set({ locale }),
    }),
    {
      name: 'ridemate-locale',
      storage: createJSONStorage(() => secureStore),
    }
  )
);
