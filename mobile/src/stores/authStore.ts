import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { sessionStorage } from '../lib/storage';

export interface AuthState {
  token: string | null;
  userId: string | null;
  email: string | null;
  isPremium: boolean;
  isAuthenticated: boolean;
  setSession: (token: string, userId: string, email: string) => void;
  setPremium: (isPremium: boolean) => void;
  logout: () => void;
  hydrated: boolean;
  setHydrated: () => void;
}

const secureStore = sessionStorage;

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      userId: null,
      email: null,
      isPremium: false,
      isAuthenticated: false,
      hydrated: false,
      setSession: (token, userId, email) =>
        set({ token, userId, email, isAuthenticated: true }),
      setPremium: (isPremium) => set({ isPremium }),
      logout: () =>
        set({
          token: null,
          userId: null,
          email: null,
          isAuthenticated: false,
          isPremium: false,
        }),
      setHydrated: () => set({ hydrated: true }),
    }),
    {
      name: 'ridemate-auth',
      onRehydrateStorage: (state) => () => state.setHydrated(),
      storage: createJSONStorage(() => secureStore),
      partialize: (state) => ({
        token: state.token,
        userId: state.userId,
        email: state.email,
        isPremium: state.isPremium,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
