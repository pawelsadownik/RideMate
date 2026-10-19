import { Platform } from 'react-native';
import * as SecureStore from 'expo-secure-store';

// Browser preview stores preferences only for the lifetime of the tab.
const memory = new Map<string, string>();
export const sessionStorage = {
  getItem: async (name: string) => Platform.OS === 'web' ? memory.get(name) ?? null : SecureStore.getItemAsync(name),
  setItem: async (name: string, value: string) => { if (Platform.OS === 'web') memory.set(name, value); else await SecureStore.setItemAsync(name, value); },
  removeItem: async (name: string) => { if (Platform.OS === 'web') memory.delete(name); else await SecureStore.deleteItemAsync(name); },
};
