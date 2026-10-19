import { act } from 'react';
import { useAuthStore } from '@/stores/authStore';

describe('authStore', () => {
  beforeEach(() => {
    useAuthStore.setState({
      token: null,
      userId: null,
      email: null,
      isPremium: false,
      isAuthenticated: false,
    });
  });

  it('setSession sets authenticated user', () => {
    let result: any;
    act(() => {
      const store = useAuthStore.getState();
      store.setSession('token-123', 'user-1', 'a@b.pl');
      result = useAuthStore.getState();
    });

    expect(result.token).toBe('token-123');
    expect(result.userId).toBe('user-1');
    expect(result.email).toBe('a@b.pl');
    expect(result.isAuthenticated).toBe(true);
  });

  it('logout clears session', () => {
    act(() => {
      const store = useAuthStore.getState();
      store.setSession('token-123', 'user-1', 'a@b.pl');
      store.logout();
    });

    const state = useAuthStore.getState();
    expect(state.token).toBeNull();
    expect(state.userId).toBeNull();
    expect(state.isAuthenticated).toBe(false);
    expect(state.isPremium).toBe(false);
  });

  it('setPremium toggles premium flag', () => {
    act(() => {
      const store = useAuthStore.getState();
      store.setPremium(true);
    });
    expect(useAuthStore.getState().isPremium).toBe(true);

    act(() => {
      const store = useAuthStore.getState();
      store.setPremium(false);
    });
    expect(useAuthStore.getState().isPremium).toBe(false);
  });
});
