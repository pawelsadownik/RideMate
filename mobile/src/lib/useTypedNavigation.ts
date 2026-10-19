import { useRouter, useLocalSearchParams } from 'expo-router';

export const useTypedNavigation = () => useRouter();
export const useTypedSearchParams = () => useLocalSearchParams();
