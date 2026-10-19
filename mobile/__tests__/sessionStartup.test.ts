import { useAuthStore } from '../src/stores/authStore';
import * as SecureStore from 'expo-secure-store';

describe('session startup', () => {
  it('finishes hydration when no saved session exists', async () => {
    useAuthStore.setState({ hydrated: false });
    await useAuthStore.persist.rehydrate();
    expect(useAuthStore.getState().hydrated).toBe(true);
  });

  it('finishes hydration when secure storage fails', async () => {
    useAuthStore.setState({ hydrated: false });
    jest.mocked(SecureStore.getItemAsync).mockRejectedValueOnce(new Error('unavailable'));
    await useAuthStore.persist.rehydrate();
    expect(useAuthStore.getState().hydrated).toBe(true);
  });
});
