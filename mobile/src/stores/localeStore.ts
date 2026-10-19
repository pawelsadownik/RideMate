import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { sessionStorage } from '../lib/storage';

export interface LocaleState {
  locale: 'pl' | 'en';
  setLocale: (locale: 'pl' | 'en') => void;
}

const secureStore = sessionStorage;

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
