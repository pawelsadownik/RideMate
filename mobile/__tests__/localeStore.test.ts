import { useLocaleStore } from '@/stores/localeStore';

describe('localeStore', () => {
  beforeEach(() => {
    useLocaleStore.setState({ locale: 'pl' });
  });

  it('setLocale updates locale', () => {
    const store = useLocaleStore.getState();
    store.setLocale('en');
    expect(useLocaleStore.getState().locale).toBe('en');
  });
});
