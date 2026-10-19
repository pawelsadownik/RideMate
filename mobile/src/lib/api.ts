import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { Platform } from 'react-native';
import { Constants } from 'expo-constants';
import { useAuthStore } from '../stores/authStore';

const API_URL = (process.env.EXPO_PUBLIC_API_URL || Constants.expoConfig?.extra?.apiUrl || 'http://10.0.2.2:5050').replace(/\/$/, '');

export const api = axios.create({
  baseURL: API_URL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    'X-Platform': Platform.OS,
  },
});

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const { token } = useAuthStore.getState();
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (res) => res,
  (err: AxiosError) => {
    if (err.response?.status === 401) {
      // Sesja wygasła – wylogowanie zostanie obsłużone przez komponenty React
      useAuthStore.getState().logout();
    }
    return Promise.reject(err);
  }
);

export type { AxiosError };
