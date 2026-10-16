# RideMate

Aplikacja społecznościowa MVP (iOS + Android) dla osób zainteresowanych wspólną jazdą na motocyklach i rowerach.
Inspiracja: Tinder (swipe, dopasowania), Bumble (prostota estetyki).

## Technologie

- **Frontend:** React Native, Expo (SDK 58), Expo Router, TypeScript (strict), TanStack Query, Zustand, Reanimated + Gesture Handler, Expo SecureStore, i18next (pl/en)
- **Backend:** ASP.NET Core Web API (.NET 9), PostgreSQL, Entity Framework Core, JWT, SignalR (czat), Swagger (dev)
- **Infrastruktura:** Docker Compose (backend + PostgreSQL), GitHub Actions CI, Expo EAS Build

## Struktura repozytorium

```
RideMate/
├── backend/          # ASP.NET Core Web API (Domain, Application, Infrastructure, Api)
├── mobile/           # Expo / React Native (src/app - Expo Router)
├── docker-compose.yml
├── .github/workflows/ci.yml
└── README.md
```

## Szybki start (lokalnie, Windows)

### 1. Backend (API + PostgreSQL)
```bash
# Uruchom bazę i API przez Docker
docker compose up -d postgres
# API natywnie (Port 5434 mapuje Postgres do hosta Windows)
cd backend
$env:ConnectionStrings__DefaultConnection="Host=127.0.0.1;Port=5434;Database=ridemate;Username=ridemate;Password=ridemate"
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project src/RideMate.Api
```
API nasłuchuje na `http://localhost:5050`, Swagger: `http://localhost:5050/swagger`

### 2. Mobile (Expo)
```bash
cd mobile
npm install
npx expo start
```
- **iPhone:** zainstaluj **Expo Go** z App Store, zeskanuj QR kod z terminala (iPhone i komputer muszą być w tej samej sieci Wi-Fi).
- **Android:** emulator lub Expo Go na urządzeniu.

Adres API dla emulatora Androida: `http://10.0.2.2:5050` (alias do localhost hosta).

## Testowanie na iPhone (Expo Go)

1. Zainstaluj **Expo Go** z App Store na swoim iPhonie.
2. Upewnij się, że iPhone i komputer są w **tej samej sieci Wi-Fi**.
3. W katalogu `mobile/` uruchom: `npx expo start`
4. Zeskanuj wyświetlony QR kod aparatem iPhona → otworzy się Expo Go z aplikacją RideMate.
5. Jeśli zmieniałeś adres IP komputera, ustaw go w `mobile/app.json` w `extra.apiUrl` (np. `http://192.168.1.50:5050`).

> Uwaga: aplikacja korzysta z modułów natywnych (secure-store, notifications, reanimated, gesture-handler). Wszystkie są dostępne w Expo Go. Nie potrzebujesz Maca ani Xcode do testowania na iPhone.

## Testy

```bash
# Backend (.NET)
cd backend
dotnet test

# Mobile (Jest)
cd mobile
npm test
```

## EAS Build (produkcja / sklepy)

```bash
cd mobile
npx eas-cli@latest init
npx eas-cli@latest build --platform android   # AAB dla Google Play
npx eas-cli@latest build --platform ios       # IPA dla App Store / TestFlight
npx eas-cli@latest submit --platform ios      # App Store Connect
```

Szczegóły: `mobile/MOBILE_BUILD.md`.

## Wymagania sklepów (checklista)

- [ ] Funkcja zgłaszania nadużyć i blokowania użytkowników (zaimplementowane)
- [ ] Usuwanie konta i danych z poziomu aplikacji (zaimplementowane)
- [ ] Polityka prywatności i Regulamin (dokumenty do uzupełnienia)
- [ ] Subskrypcje Premium przez Apple IAP / Google Play Billing (nie własny system płatności)
- [ ] Klasyfikacja wiekowa i deklaracja aplikacji społecznościowej (Google Play)

## Koszty utrzymania (orientacyjne, kilka tys. użytkowników)

- VPS (2 vCPU / 4 GB RAM): ~15–25 EUR/m-c
- PostgreSQL (na VPS): 0 EUR
- Object Storage (zdjęcia): ~2–5 EUR/m-c
- Push (Expo/FCM): darmowy poziom początkowy
- Razem: ok. **20–40 EUR/m-c**
