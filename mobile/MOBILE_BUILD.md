# RideMate — Budowanie i publikacja mobilna

## Wymagania środowiska

- Node.js 20 LTS+, npm
- Expo CLI: `npm install -g @expo/cli` lub `npx expo`
- EAS CLI: `npm install -g eas-cli` lub `npx eas-cli@latest`
- Konto Expo (darmowe): https://expo.dev/accounts
- Konto Google Play Console (jednorazowo $25)
- Konto Apple Developer Program ($99/rok) — wymagane dla iOS/TestFlight

Zmienne środowiskowe (`.env` lub `export`):
```
EXPO_PUBLIC_API_URL=http://10.0.2.2:5050    # emulator Android (localhost hosta)
EXPO_PUBLIC_API_URL=https://api.ridemate.appcity.pl  # production
```

## EAS Build (zalecane — chmura, brak lokalnego Xcode)

### 1. Inicjalizacja projektu Expo
```bash
cd mobile
npx expo register        # tylko raz, jeśli nie masz konta Expo
npx eas-cli@latest init  # powiąż lokalny projekt z Expo
```

### 2. Build Android (AAB) — Google Play
```bash
# Development build (z dev clientem, do testów na emulatorze/urządzeniu)
npx eas-cli@latest build --platform android --profile development

# Preview build (internal sharing)
npx eas-cli@latest build --platform android --profile android-preview

# Production release (AAB do Google Play)
npx eas-cli@latest build --platform android --profile production
```
Efekt: plik `.aab` w katalogu artefaktów EAS. Wgraj go do Google Play Console → Wydania → Produkcja.

### 3. Build iOS (IPA) — App Store / TestFlight
iOS build erfolgt wyłącznie w chmurze EAS (nie potrzebujesz lokalnego Maca):
```bash
# Development build (do testowania na fizycznym iPhone przez Expo Dev Client)
npx eas-cli@latest build --platform ios --profile development

# Preview (TestFlight-like internal)
npx eas-cli@latest build --platform ios --profile ios-preview

# Production release (IPA do App Store Connect)
npx eas-cli@latest build --platform ios --profile production
```

### 4. Submit do sklepów (EAS Submit)
```bash
# iOS → App Store Connect (TestFlight / Produkcja)
npx eas-cli@latest submit --platform ios --profile production

# Android → Google Play Console
npx eas-cli@latest submit --platform android --profile production
```

Wymagane w `eas.json` → `submit.production`: `ascAppId`, `appleTeamId` (iOS) oraz `serviceAccountJsonPath` (Android).

## Lokalny build Androida (emulator, bez chmury)

Wymaga: Android Studio, Android SDK, JDK 17, zmienna `ANDROID_HOME`.

```bash
cd mobile
npx expo prebuild --platform android   # wygeneruje katalog android/ (CNG)
npx expo run:android                   # buduje i instaluje na emulatorze
```

Dla emulatora API używa `http://10.0.2.2:5050` (alias do localhost komputera hosta).

## Powiadomienia push (Expo)

1. W `app.json` dodano plugin `expo-notifications` z kanałem `ridemate`.
2. W kodzie (mobile): po zalogowaniu wywołaj `Notifications.registerForPushNotificationsAsync()` i wyślij token na `POST /api/push-tokens`.
3. Backend: `NotificationService` — w trybie Development loguje push do konsoli; w produkcji wysyła przez Expo Push API (wymaga konta Expo i skonfigurowanych poświadczeń).
4. Rzeczywiste push w produkcji aktywuj po skonfigurowaniu konta Expo i dodaniu poświadczeń do EAS.

## Subskrypcje In-App Purchase (Premium, 19,99 PLN/m-c)

1. **Apple App Store Connect**: utwórz aplikację → Subskrypcje → produkt odnawialny miesięcznie "ridemate.premium.monthly", cena 19,99 PLN.
2. **Google Play Console**: produkty subskrypcyjne w aplikacji → subskrypcja "ridemate_premium_monthly", cena 19,99 PLN.
3. **RevenueCat (opcjonalnie)**: jeśli używasz RevenueCat, dodaj klucze API do zmiennych środowiskowych i zamień lokalną walidację entylementów na RevenueCat.
4. W kodzie mobile: po zakupie wyślij `POST /api/subscription` z `platform`, `productId`, `transactionId`, `expiresAt`.
5. Backend źródłem prawdy: `SubscriptionEntitlement.IsActive()`.

**W trybie deweloperskim**: endpoint `POST /api/subscription/dev/premium` aktywuje Premium bez płatności. Jest zabezpieczony przed wywołaniem w produkcji (`isProduction` musi być `false`).

## Publikacja — Google Play (aplikacja społecznościowa)

Google Play ma dodatkowe wymagania dla aplikacji społecznościowych:

- **Formularz deklaracji aplikacji społecznościowej** (Google Play Policy) — opis sposobu działania, weryfikacji użytkowników, moderacji.
- **Zgłaszanie nadużyć w aplikacji** — musi być dostępne z poziomu aplikacji (zaimplementowane: ekran profil → Zgłoś).
- **Polityka prywatności** — publiczny URL, opisuje jakie dane są zbierane (e-mail, lokalizacja przybliżona, zdjęcia, wiadomości) i jak są przetwarzane zgodnie z RODO.
- **Usuwanie konta i danych** — zaimplementowane (`DELETE /api/auth/account`).
- **Subskrypcje** — jeśli aplikacja oferuje zakupy w aplikacji, muszą korzystać z systemu rozliczeniowego Google Play Billing (nie własny system kart).
- **Klasyfikacja wiekowa** — wypełnij ankietę w Google Play Console.
- **Oświadczenie o governoe danych** (Data Safety section) — zbieramy: e-mail, lokalizacja, zdjęcia, wiadomości. Uzyskaj zgodę użytkownika.

## Publikacja — Apple App Store

- **App Review Guidelines 5.1.1** — aplikacja musi mieć funkcję zgłaszania nadużyć i blokowania użytkowników (zaimplementowane).
- **App Review Guidelines 3.1.1** — subskrypcje Premium muszą używać Apple In-App Purchase (RevenueCat lub bezpośrednio StoreKit), nie własny system płatności.
- **App Review Guidelines 5.1.2** — zawartość générowana przez użytkowników musi mieć mechanizm moderacji i zgłoszeń (zaimplementowane: panel zgłoszeń, blokada).
- **TestFlight** — wewnętrzne testowanie przed recenzją (do 100 testerów wewnętrznych).
- **Polityka prywatności i Regulamin** — wymagane w metadanych aplikacji.
- **Usuwanie konta** — musi być możliwe z poziomu aplikacji (zaimplementowane).

## Checklista przed submit

- [ ] Aplikacja kompiluje się jako release (AAB / IPA)
- [ ] Wszystkie ekrany mają polski i angielski język (i18n)
- [ ] Rejestracja, logowanie, profil, swipe, dopasowanie, czat działają
- [ ] Blokowanie i zgłaszanie użytkowników dostępne z poziomu aplikacji
- [ ] Usuwanie konta działa
- [ ] Premium (dev mode) działa tylko w Development
- [ ] Polityka prywatności i Regulamin przygotowane (dokumenty do wypełnienia)
- [ ] Ikona aplikacji i ekran startowy skonfigurowane w `app.json`
- [ ] Powiadomienia push skonfigurowane (Expo)

## Koszty orientacyjne

| Usługa | Koszt miesięczny |
|--------|-----------------|
| Google Play Console | $25 jednorazowo |
| Apple Developer Program | $99/rok |
| EAS Build (darmowy plan) | $0 (limitowane minuty budowania) |
| Expo Push Notifications | darmowy poziom początkowy |
| Apple In-App Purchase / Google Play Billing | 15% prowizja od subskrypcji |

## Instrukcje dla recenzenta App Store

W sekcji "Review Information" w App Store Connect podaj:
- Testowe konto użytkownika (e-mail/hasło)
- Opis flow: rejestracja → profil → swipe → dopasowanie → czat
- Informację, że aplikacja nie jest platformą płatnych przewozów (brak rezerwacji, brak płatności za przejazdy)
- Informację, że subskrypcja Premium jest opcjonalna i dotyczy tylko dodatkowych funkcji (nieograniczone polubienia, zaawansowane filtry)
