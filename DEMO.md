# Testy manualne — baza w pamięci

Tryb developerski używa SQLite w pamięci procesu, bez pliku bazy i bez PostgreSQL. Obsługuje relacje i transakcje. Po zatrzymaniu API dane znikają; kolejny start odtwarza 20 kont.

## Uruchomienie

PowerShell, katalog projektu:
```powershell
cd <project-root>
.\scripts\start-demo.ps1 -LanIp YOUR_COMPUTER_LAN_IP
```
Adres podany w `-LanIp` powinien być bieżącym IPv4 komputera w Wi-Fi. Dla testów wyłącznie na komputerze można pominąć parametr (127.0.0.1).

W drugim terminalu:
```powershell
cd <project-root>\mobile
$env:EXPO_PUBLIC_API_URL='http://YOUR_COMPUTER_LAN_IP:5050'
npx expo start --go --lan --port 8082
```

Podgląd: http://localhost:8082. Swagger: http://localhost:5050/swagger.

Na iPhonie zainstaluj Expo Go, połącz z tym samym Wi-Fi i zeskanuj QR aparatem. W razie wymogu zaloguj Expo CLI (`npx expo login`) i Expo Go na to samo konto. Aktualny adres Metro: exp://YOUR_COMPUTER_LAN_IP:8082. Adres IP może się zmienić — wtedy zmień oba parametry. Jeśli system poprosi, zezwól Expo Go na sieć lokalną.

## Dane logowania

Wszystkie konta mają hasło: `RideMate123!`.

| Login | Profil |
|---|---|
| demo01@ridemate.test | Marek — główne konto do testów |
| demo02@ridemate.test | Kasia |
| demo03@ridemate.test | Tomek |
| demo04@ridemate.test | Anna |
| demo05@ridemate.test | Piotr |
| demo06@ridemate.test | Ola |
| demo07@ridemate.test | Michał |
| demo08@ridemate.test | Magda |
| demo09@ridemate.test | Paweł |
| demo10@ridemate.test | Natalia |
| demo11@ridemate.test | Kamil |
| demo12@ridemate.test | Ewa |
| demo13@ridemate.test | Adam |
| demo14@ridemate.test | Julia |
| demo15@ridemate.test | Bartek |
| demo16@ridemate.test | Zosia |
| demo17@ridemate.test | Wojtek |
| demo18@ridemate.test | Karolina |
| demo19@ridemate.test | Łukasz |
| demo20@ridemate.test | Monika |

Konta mają potwierdzony e-mail, wiek 24–38 lat, opisy, miasta w Trójmieście i okolicy, pobliskie współrzędne, różne aktywności oraz preferencje jazdy. Każde ma jedno lokalne zdjęcie demonstracyjne.

## Scenariusze

1. Zaloguj demo01: zobaczysz 18 profili do odkrycia. Łącznie jest 20 kont; własny profil i partnerka z istniejącego dopasowania nie pojawiają się w odkrywaniu.
2. Polub demo02..demo06: te konta już polubiły demo01, więc powstanie dopasowanie i można rozpocząć rozmowę.
3. Wejdź w Czat: gotowa rozmowa z demo20 zawiera dwie wiadomości. Możesz wysyłać następne.
4. Otwórz aplikację w drugim urządzeniu / sesji i zaloguj demo20, aby odpisać. Czat odświeża listę również przez REST co 5 sekund, bo pełne powiadamianie SignalR w backendzie pozostaje niedokończone.
5. Zrestartuj API, aby odtworzyć bazę. Konta utworzone ręcznie, polubienia i wiadomości dodane w trakcie testów zostaną utracone.

## Konfiguracja i izolacja

Skrypt ustawia ASPNETCORE_ENVIRONMENT=Development, Database__Provider=InMemory, Demo__Enabled=true i Demo__PublicApiUrl. Wybranie InMemory albo seedowania w środowisku innym niż Development przerywa start. Seedowanie PostgreSQL jest dopuszczone wyłącznie w osobnej bazie ridemate_demo, przy Demo__AllowPersistent=true i środowisku Development; konfiguracja wdrożenia jest opisana w deploy/UBUNTU-DEMO.md. W tym wariancie restart nie usuwa danych. Domyślne uruchomienie bez tych ustawień nadal korzysta z PostgreSQL.

Dwadzieścia zdjęć dorosłych osób z https://pravatar.cc/ (CC0 Avatar Placeholder) dobrano wizualnie do fikcyjnych profili i zapisano w backend/src/RideMate.Api/wwwroot/demo/01.jpg..20.jpg. Dane profili i źródła zdjęć zapisano w profiles.json w tym samym katalogu. Do ich wyświetlania nie potrzeba połączenia z Pravatar; aplikacja pobiera je z lokalnego API. Osoby na przykładowych zdjęciach nie są rzeczywistymi użytkownikami RideMate.

Tryb demo umożliwia testy manualne, ale nie zastępuje sprawdzenia zapytań i współbieżności na PostgreSQL ani nie kończy brakujących funkcji produkcyjnych, takich jak reset hasła i usuwanie konta.

## Wyniki weryfikacji (19.10.2026)

- Testy API: 7/7 poprawnych (w tym 3 nowe testy demo).
- Testy domeny: 14/14 poprawnych.
- Testy mobile: 11/11 poprawnych.
- TypeScript: bez błędów; lint: bez błędów, pozostają ostrzeżenia w istniejącym kodzie.
- Expo Doctor: 20/20 kontroli poprawnych.
- Eksport paczki JavaScript/Hermes dla iOS zakończony sukcesem; to nie jest podpisany build IPA ani test fizycznego iPhone'a.
- W podglądzie web 428 × 926 sprawdzono: ekran powitalny, logowanie demo01, zdjęcia w odkrywaniu, wzajemne polubienie, ekran dopasowania, otwarcie rozmowy i wysłanie wiadomości. Naprawiono ujawnioną nawigację do rozmowy, przenosząc ekran wątku do głównego stosu.
- API i Metro pozostawiono uruchomione na potrzeby testów. Baza może zawierać polubienie i wiadomość dodane podczas sprawdzenia; restart API przywraca dane początkowe.

Nowe tło ekranu powitalnego wygenerowano wbudowanym image_gen i zapisano w mobile/assets/welcome-road.png. Prompt zapisano obok w mobile/assets/welcome-road.prompt.txt.
