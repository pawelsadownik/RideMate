import { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, StyleSheet, KeyboardAvoidingView, Platform } from 'react-native';
import { useRouter } from 'expo-router';
import { useAuthStore } from '@/stores/authStore';
import { useThemeColors } from '@/design-system/useThemeColors';
import i18n from '@/lib/i18n';
import { useTranslation } from 'react-i18next';
import { api } from '@/lib/api';

export default function Login() {
  const router = useRouter();
  const { t } = useTranslation();
  const { setSession } = useAuthStore();
  const colors = useThemeColors();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleLogin = async () => {
    setError(null);
    setLoading(true);
    try {
      const res = await api.post('/api/auth/login', {
        email,
        password,
        dateOfBirth: '1990-01-01', // TODO: z ekranu rejestracji; login wymaga DOB w MVP uproszczony
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
        <Text style={styles.subtitle}>{t('auth.welcomeSub')}</Text>
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

        {error && <Text style={styles.error}>{error}</Text>}

        <TouchableOpacity style={styles.button} onPress={handleLogin} disabled={loading}>
          <Text style={styles.buttonText}>{loading ? t('common.loading') : t('auth.signIn')}</Text>
        </TouchableOpacity>
      </View>

      <View style={styles.footer}>
        <Text style={styles.footerText}>{t('auth.dontHaveAccount')} </Text>
        <TouchableOpacity style={styles.link} onPress={() => router.push('/(auth)/register')}>
          <Text style={styles.linkText}>{t('auth.signUp')}</Text>
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
