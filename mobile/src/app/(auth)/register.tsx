import { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, StyleSheet, KeyboardAvoidingView, Platform } from 'react-native';
import { useRouter } from 'expo-router';
import { useAuthStore } from '@/stores/authStore';
import { useThemeColors } from '@/design-system/useThemeColors';
import i18n from '@/lib/i18n';
import { useTranslation } from 'react-i18next';
import { api } from '@/lib/api';

export default function Register() {
  const router = useRouter();
  const { t } = useTranslation();
  const { setSession } = useAuthStore();
  const colors = useThemeColors();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [dob, setDob] = useState<Date | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleRegister = async () => {
    setError(null);
    if (!email || !password || !dob) {
      setError('Wypełnij wszystkie pola.');
      return;
    }
    if (password !== confirmPassword) {
      setError('Hasła nie są identyczne.');
      return;
    }
    if (password.length < 8) {
      setError('Hasło musi mieć co najmniej 8 znaków.');
      return;
    }
    const age = new Date().getFullYear() - dob.getFullYear();
    if (age < 18) {
      setError('Musisz być pełnoletni (18+).');
      return;
    }

    setLoading(true);
    try {
      const res = await api.post('/api/auth/register', {
        email,
        password,
        dateOfBirth: dob.toISOString(),
      });
      const { token, userId } = res.data;
      setSession(token, userId, email);
      router.replace('/(tabs)/discover');
    } catch (e: any) {
      setError(e.response?.data?.error || t('common.unknownError'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.title}>RideMate</Text>
        <Text style={styles.subtitle}>{t('auth.welcome')}</Text>
      </View>

      <View style={styles.form}>
        <Text style={styles.label}>{t('auth.email')}</Text>
        <TextInput
          style={styles.input}
          placeholder={t('auth.emailPlaceholder')}
          value={email}
          onChangeText={setEmail}
          autoCapitalize="none"
          keyboardType="email-address"
        />

        <Text style={styles.label}>{t('auth.password')}</Text>
        <TextInput
          style={styles.input}
          placeholder={t('auth.passwordPlaceholder')}
          value={password}
          onChangeText={setPassword}
          secureTextEntry
        />

        <Text style={styles.label}>{t('auth.confirmPassword')}</Text>
        <TextInput
          style={styles.input}
          placeholder={t('auth.passwordPlaceholder')}
          value={confirmPassword}
          onChangeText={setConfirmPassword}
          secureTextEntry
        />

        <Text style={styles.label}>{t('auth.dateOfBirth')}</Text>
        <View style={styles.input}>
          <Text style={styles.inputValue}>{
            dob ? dob.toLocaleDateString(undefined, { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'
          }</Text>
        </View>
        <TouchableOpacity style={styles.input} onPress={() => {}} disabled>
          <Text style={styles.inputValue}>{t('auth.dateOfBirth')} (wybierz datę)</Text>
        </TouchableOpacity>

        {error && <Text style={styles.error}>{error}</Text>}

        <TouchableOpacity style={styles.button} onPress={handleRegister} disabled={loading}>
          <Text style={styles.buttonText}>{loading ? t('common.loading') : t('auth.signUp')}</Text>
        </TouchableOpacity>
      </View>

      <View style={styles.footer}>
        <Text style={styles.footerText}>{t('auth.alreadyHaveAccount')} </Text>
        <TouchableOpacity style={styles.link} onPress={() => router.push('/(auth)/login')}>
          <Text style={styles.linkText}>{t('auth.signIn')}</Text>
        </TouchableOpacity>
      </View>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 24, justifyContent: 'center' },
  header: { alignItems: 'center', marginBottom: 40 },
  title: { fontSize: 36, fontWeight: '800' },
  subtitle: { fontSize: 16, color: '#6B6B73', marginTop: 6, textAlign: 'center' },
  form: { gap: 16 },
  label: { fontSize: 14, fontWeight: '600', marginBottom: 6 },
  input: {
    borderWidth: 1,
    borderColor: '#E2E4E8',
    borderRadius: 12,
    paddingHorizontal: 14,
    paddingVertical: 14,
    fontSize: 16,
  },
  inputValue: { fontSize: 16, color: '#6B6B73' },
  error: { color: '#EF4444', fontSize: 14, marginBottom: 8 },
  button: {
    backgroundColor: '#FF5E50',
    borderRadius: 14,
    paddingVertical: 16,
    alignItems: 'center',
    marginTop: 8,
  },
  buttonText: { color: '#FFFFFF', fontSize: 16, fontWeight: '700' },
  footer: { flexDirection: 'row', justifyContent: 'center', marginTop: 24, gap: 6 },
  footerText: { fontSize: 14, color: '#6B6B73' },
  link: { padding: 4 },
  linkText: { fontSize: 14, fontWeight: '700', color: '#FF5E50' },
});
