import { Tabs } from 'expo-router';
import { useThemeColors } from '@/design-system/useThemeColors';

export default function TabsLayout() {
  const colors = useThemeColors();
  return (
    <Tabs>
      <Tabs.Screen
        name="(tabs)/discover"
        options={{ title: 'Odkrywaj', tabBarIcon: () => null }}
      />
      <Tabs.Screen
        name="(tabs)/matches"
        options={{ title: 'Dopasowania', tabBarIcon: () => null }}
      />
      <Tabs.Screen
        name="(tabs)/chat"
        options={{ title: 'Czat', tabBarIcon: () => null }}
      />
      <Tabs.Screen
        name="(tabs)/profile"
        options={{ title: 'Profil', tabBarIcon: () => null }}
      />
    </Tabs>
  );
}
