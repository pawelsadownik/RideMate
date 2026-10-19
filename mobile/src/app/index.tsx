import { Redirect } from 'expo-router';
import { useAuthStore } from '@/stores/authStore';
export default function Index() {
  const hydrated = useAuthStore(s => s.hydrated);
  const authenticated = useAuthStore(s => s.isAuthenticated);
  if (!hydrated) return null;
  return <Redirect href={authenticated ? '/(tabs)/discover' : '/(auth)/login'} />;
}
