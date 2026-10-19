import { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity, StyleSheet, KeyboardAvoidingView, Platform, ImageBackground, ScrollView } from 'react-native';
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
  const [showForm, setShowForm] = useState(false);
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

  if (!showForm) return (
    <ImageBackground source={require('../../../assets/welcome-road.png')} style={styles.welcome} resizeMode="cover">
      <View style={styles.shade} />
      <View style={styles.brand}>
        <Text style={styles.brandMark}>⌁</Text>
        <Text style={styles.brandTitle}>RideMate</Text>
        <Text style={styles.brandTagline}>{i18n.language === 'en' ? 'People. Roads. More adventure.' : 'Ludzie. Drogi. Więcej przygód.'}</Text>
      </View>
      <View style={styles.welcomeActions}>
        <Text style={styles.welcomeIntro}>{t('auth.welcomeSub')}</Text>
        <TouchableOpacity accessibilityRole="button" style={styles.createButton} onPress={() => router.push('/(auth)/register')}>
          <Text style={styles.buttonText}>{i18n.language === 'en' ? 'Create account' : 'Utwórz konto'}</Text>
        </TouchableOpacity>
        <TouchableOpacity accessibilityRole="button" style={styles.signInButton} onPress={() => setShowForm(true)}>
          <Text style={styles.signInText}>{t('auth.signIn')}</Text>
        </TouchableOpacity>
        <Text style={styles.welcomeFootnote}>{i18n.language === 'en' ? 'For adults who share a passion for riding.' : 'Dla pełnoletnich osób, które łączy pasja do jazdy.'}</Text>
      </View>
    </ImageBackground>
  );

  return (
    <KeyboardAvoidingView behavior={Platform.OS === 'ios' ? 'padding' : undefined} style={styles.container}>
      <TouchableOpacity accessibilityRole="button" onPress={() => setShowForm(false)}><Text style={{color: colors.accent, fontSize:16, marginBottom:24}}>‹ {i18n.language === "en" ? "Back" : "Wróć"}</Text></TouchableOpacity>
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
  container: { flex: 1, padding: 24, justifyContent: 'center', width: '100%', maxWidth: 480, alignSelf: 'center' },
  welcome: { flex: 1, backgroundColor:'#111', width:'100%', maxWidth:520, alignSelf:'center' },
  shade: { ...StyleSheet.absoluteFill, backgroundColor:'rgba(0,0,0,.25)' },
  brand: { alignItems:'center', paddingTop:90, paddingHorizontal:20 },
  brandMark: { fontSize:74, lineHeight:80, color:'#ff4965', fontWeight:'900' },
  brandTitle: { color:'#fff', fontSize:48, fontWeight:'800', letterSpacing:-2 },
  brandTagline: { color:'#fff', fontSize:16, marginTop:4 },
  welcomeActions: { marginTop:'auto', padding:28, paddingTop:35, paddingBottom:40, backgroundColor:'rgba(0,0,0,.55)' },
  welcomeIntro: { color:'#fff', textAlign:'center', fontSize:16, lineHeight:23, marginBottom:22 },
  createButton: { backgroundColor:'#ff4564', borderRadius:28, paddingVertical:17, alignItems:'center' },
  signInButton: { backgroundColor:'#fff', borderRadius:28, paddingVertical:17, alignItems:'center', marginTop:12 },
  signInText: { color:'#17191c', fontSize:16, fontWeight:'700' },
  welcomeFootnote: { color:'#ddd', textAlign:'center', fontSize:11, lineHeight:16, marginTop:22 },
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
