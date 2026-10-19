import { useThemeStore } from '@/stores/themeStore';

describe('themeStore', () => {
  beforeEach(() => {
    useThemeStore.setState({ theme: 'system', resolvedTheme: 'light' });
  });

  it('setTheme updates theme', () => {
    const store = useThemeStore.getState();
    store.setTheme('dark');
    expect(useThemeStore.getState().theme).toBe('dark');
  });

  it('setResolvedTheme updates resolved theme', () => {
    const store = useThemeStore.getState();
    store.setResolvedTheme('dark');
    expect(useThemeStore.getState().resolvedTheme).toBe('dark');
  });
});
