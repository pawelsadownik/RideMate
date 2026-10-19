import { Text } from 'react-native';
import { Tabs } from 'expo-router';
import { useThemeColors } from '@/design-system/useThemeColors';

export default function TabsLayout() {
  const colors = useThemeColors();
  return (
    <Tabs screenOptions={{ headerShown: false, tabBarActiveTintColor: colors.accent, tabBarStyle: { backgroundColor: colors.background, borderTopColor: colors.border }, tabBarLabelStyle: { fontSize: 11 } }}>
      <Tabs.Screen
        name="discover"
        options={{ title: 'Odkrywaj', tabBarIcon: ({ color }) => <Text style={{ color, fontSize: 25 }}>♨</Text> }}
      />
      <Tabs.Screen
        name="matches"
        options={{ title: 'Dopasowania', tabBarIcon: ({ color }) => <Text style={{ color, fontSize: 25 }}>♡</Text> }}
      />
      <Tabs.Screen
        name="chat"
        options={{ title: 'Czat', tabBarIcon: ({ color }) => <Text style={{ color, fontSize: 25 }}>☏</Text> }}
      />
      <Tabs.Screen
        name="profile"
        options={{ title: 'Profil', tabBarIcon: ({ color }) => <Text style={{ color, fontSize: 25 }}>♙</Text> }}
      />
    </Tabs>
  );
}
