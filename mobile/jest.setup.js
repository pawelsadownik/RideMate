jest.mock('expo-secure-store', () => ({
  getItemAsync: jest.fn(async () => null),
  setItemAsync: jest.fn(async () => {}),
  deleteItemAsync: jest.fn(async () => {}),
}));

jest.mock('expo-notifications', () => ({
  getExpoPushTokenAsync: jest.fn(async () => ({ data: 'test-token' })),
  requestPermissionsAsync: jest.fn(async () => ({ granted: true })),
  addNotificationReceivedListener: jest.fn(() => ({ remove: () => {} })),
  addNotificationResponseReceivedListener: jest.fn(() => ({ remove: () => {} })),
  setNotificationChannelAsync: jest.fn(async () => {}),
  presentNotificationAsync: jest.fn(async () => {}),
}));

jest.mock('expo-location', () => ({
  getCurrentPositionAsync: jest.fn(async () => ({ coords: { latitude: 52.23, longitude: 21.01 } })),
  requestForegroundPermissionsAsync: jest.fn(async () => ({ granted: true })),
}));

jest.mock('react-native-gesture-handler', () => {
  const View = require('react-native').View;
  return {
    GestureHandlerRootView: View,
    PanGestureHandler: View,
    State: {},
    Direction: {},
  };
});

jest.mock('react-native-reanimated', () => {
  const View = require('react-native').View;
  const Animated = require('react-native').Animated;
  return {
    ...Animated,
    useSharedValue: (v) => ({ value: v }),
    useAnimatedGestureHandler: () => ({}),
    useAnimatedStyle: (s) => s || {},
    withSpring: (v) => v,
    withTiming: (v) => v,
    interpolate: () => (x) => x,
    runOnJS: (f) => f,
    cancelAnimation: () => {},
  };
});
