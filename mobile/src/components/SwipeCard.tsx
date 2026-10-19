import React, { useRef } from 'react';
import { View, Text, Image, StyleSheet, TouchableOpacity, useWindowDimensions } from 'react-native';
import { Gesture, GestureDetector } from 'react-native-gesture-handler';
import Animated, { useSharedValue, useAnimatedStyle, withSpring } from 'react-native-reanimated';
import { useTranslation } from 'react-i18next';

interface Props {
  profile: { id: string; nickname: string; age: number; distanceKm: number; activityType: string; bio?: string | null; city?: string | null; photos: (string | { url: string; isPrimary?: boolean })[] };
  onLike: (id: string) => void | Promise<void>;
  onPass: (id: string) => void | Promise<void>;
  loading?: boolean;
}
export function SwipeCard({ profile, onLike, onPass, loading }: Props) {
  const { width, height } = useWindowDimensions();
  const { t } = useTranslation();
  const x = useSharedValue(0);
  const pending = useRef(false);
  const choose = async (like: boolean) => {
    if (pending.current || loading) return;
    pending.current = true;
    x.value = withSpring(like ? width : -width);
    try { await (like ? onLike(profile.id) : onPass(profile.id)); }
    finally { x.value = withSpring(0); pending.current = false; }
  };
  // Gesture callbacks execute on input events, never during React render.
  /* eslint-disable react-hooks/refs */
  const gesture = Gesture.Pan().runOnJS(true).activeOffsetX([-15, 15])
    .onUpdate(e => { if (!pending.current) x.value = e.translationX; })
    .onEnd(e => { if (Math.abs(e.translationX) > 100 || Math.abs(e.velocityX) > 600) void choose(e.translationX > 0); else x.value = withSpring(0); });
  /* eslint-enable react-hooks/refs */
  const animatedStyle = useAnimatedStyle(() => ({ transform: [{ translateX: x.value }, { rotate: (x.value / 25) + 'deg' }] }));
  const photo = profile.photos.find(p => typeof p !== 'string' && p.isPrimary) ?? profile.photos[0];
  const uri = typeof photo === 'string' ? photo : photo?.url;
  return <View style={s.root}>
    <GestureDetector gesture={gesture}>
      <Animated.View style={[s.card, { width: Math.min(width - 32, 480), height: Math.min(height * .59, 620) }, animatedStyle]}>
        {uri ? <Image source={{ uri }} style={StyleSheet.absoluteFill} resizeMode="cover" /> : <View style={s.placeholder}><Text style={s.placeholderText}>RideMate</Text></View>}
        <View style={s.caption}>
          <Text style={s.name}>{profile.nickname}, {profile.age} <Text style={{color:'#16d78d'}}>●</Text></Text>
          <Text style={s.meta}>{profile.activityType === 'Bicycle' ? '🚲' : '🏍'} {profile.activityType} · {profile.city}</Text>
          <Text style={s.meta}>{Number.isFinite(profile.distanceKm) && profile.distanceKm < 100000 ? t('discover.kmAway', { km: Math.round(profile.distanceKm) }) : '—'}</Text>
          {!!profile.bio && <Text style={s.bio} numberOfLines={3}>{profile.bio}</Text>}
        </View>
      </Animated.View>
    </GestureDetector>
    <View style={s.actions}>
      <TouchableOpacity accessibilityLabel={t('discover.pass')} disabled={loading} onPress={() => void choose(false)} style={[s.button, {backgroundColor:'#ff4564'}]}><Text style={s.cross}>×</Text></TouchableOpacity>
      <TouchableOpacity accessibilityLabel={t('discover.like')} disabled={loading} onPress={() => void choose(true)} style={s.button}><Text style={s.heart}>♥</Text></TouchableOpacity>
    </View>
  </View>;
}
const s = StyleSheet.create({
  root:{flex:1,alignItems:'center',justifyContent:'center'}, card:{borderRadius:22,overflow:'hidden',backgroundColor:'#20282d'},
  placeholder:{flex:1,alignItems:'center',justifyContent:'center',backgroundColor:'#27363c'},placeholderText:{fontSize:32,color:'#fff',fontWeight:'800'},
  caption:{position:'absolute',bottom:0,left:0,right:0,padding:20,backgroundColor:'rgba(0,0,0,.64)'},
  name:{color:'#fff',fontSize:27,fontWeight:'800'},meta:{color:'#fff',fontSize:13,marginTop:5},bio:{color:'#fff',fontSize:14,lineHeight:20,marginTop:10},
  actions:{flexDirection:'row',gap:28,paddingTop:16,paddingBottom:8},button:{width:66,height:66,borderRadius:33,backgroundColor:'#fff',alignItems:'center',justifyContent:'center',boxShadow:'0px 3px 12px #00000020'},
  cross:{color:'#fff',fontSize:45,lineHeight:50},heart:{color:'#13d593',fontSize:36}
});
