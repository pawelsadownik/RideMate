# RideMate obok Atlasa — środowisko testowe

Projekt Compose: ridemate-demo. Dwa oddzielne kontenery, sieć ridemate-demo_demo_private i wolumen ridemate-demo_demo_database. PostgreSQL nie publikuje portu na hoście. API publikuje wyłącznie 127.0.0.1:8050 na Ubuntu. Port 8050 trzeba potwierdzić jako wolny przed uruchomieniem. Limity: API 512 MB / 1 CPU, baza 256 MB / 0.5 CPU. Nie używać docker system prune ani docker compose down -v.

## Weryfikacja przed wdrożeniem

Odczytać docker ps, docker compose ls, ss -ltn, df -h / oraz free -m. Zapisać identyfikatory i stan kontenerów Atlasa. Nie modyfikować projektu Compose, sieci ani wolumenów Atlasa. Przy braku zasobów przerwać wdrożenie.

## Ubuntu

Umieścić backend/ i deploy/ w osobnym katalogu użytkownika, np. ~/ridemate-demo. Utworzyć deploy/.env z prawami 600. Ustawić POSTGRES_PASSWORD i JWT_KEY na niezależne losowe sekrety (minimum 32 bajty dla JWT), API_PORT=8050 oraz PUBLIC_API_URL na adres osiągalny z telefonu. Nie zapisywać sekretów w repozytorium ani logach.

Uruchomić z katalogu projektu:

~~~sh
docker compose --env-file deploy/.env -f deploy/compose.demo.yml config --quiet
docker compose --env-file deploy/.env -f deploy/compose.demo.yml up -d --build
curl --fail http://127.0.0.1:8050/health
~~~

Ponownie sprawdzić stan kontenerów Atlasa. Testy RideMate: login demo01, 18 profili do odkrycia w nowej bazie, zdjęcia z /demo/01.jpg, dopasowanie i czat. Konto demo01@ridemate.test, hasło RideMate123!; konta demo01..demo20.

## Dostęp z Windows i iPhone

Ubuntu jest w prywatnej sieci MacBooka (UBUNTU_VM_IP), więc adresu tego nie wpisujemy w Expo na iPhonie. Użyć osobnego tunelu SSH na Windows (8050); nie uruchamiać ani nie zastępować tunelu Atlasa 8088:

~~~powershell
ssh -F <project-root>/deploy/ridemate-ssh.config -N ridemate-ubuntu
~~~

Plik ridemate-ssh.config definiuje wyłącznie tunel 8050; nie zmienia ani nie uruchamia tunelu Atlasa 8088. Przed testami telefonu sprawdzić nasłuch na porcie 8050.

Ustawić mobile/.env.local: EXPO_PUBLIC_API_URL=http://YOUR_COMPUTER_LAN_IP:8050 i zrestartować Metro. PUBLIC_API_URL na Ubuntu musi mieć ten sam adres, aby zdjęcia działały w telefonie. Windows i iPhone muszą być w tej samej zaufanej sieci Wi-Fi; Windows i tunel muszą pozostać uruchomione. Adres Wi-Fi może się zmienić.

## Dane demo i ograniczenia

Seedowanie PostgreSQL wymaga trzech jawnych ustawień: środowiska Development, Demo:AllowPersistent=true i nazwy bazy ridemate_demo. Pierwszy start pustej bazy tworzy 20 kont. Restarty zachowują dane w wolumenie; nie resetują polubień i czatu. Lokalny tryb InMemory nadal resetuje dane po zakończeniu procesu.

Zdjęcia i wiek testowych profili dobrano wizualnie; imiona i wiek są fikcyjnymi danymi demonstracyjnymi, a nie danymi osób na zdjęciach. Źródła zapisano w backend/src/RideMate.Api/wwwroot/demo/profiles.json. To nadal środowisko testowe ze znanymi danymi logowania; nie publikować go w Internecie jako produkcji.

## Wdrożenie zweryfikowane 19.10.2026

Backend i PostgreSQL uruchomiono z commita d4104b21f8ed94edc912fe3fa15745c70c045d61 w osobnym projekcie ridemate-demo. Testy po wdrożeniu potwierdziły healthy API, login, 20 kont demo, 18 profili do odkrycia, jedną konwersację i zdjęcie serwowane lokalnie. Kontenery Atlasa zachowały identyczne ID, stan running i czasy uruchomienia.

Kod aplikacji, Compose i skrypt wdrożenia należą do repozytorium RideMate. W repozytorium Atlasa jest jedynie ręcznie wywoływany workflow pobierający dokładny commit RideMate na współdzielonym runnerze Ubuntu. Kontenery, obrazy, sieć, wolumen, sekrety i katalog runtime są oddzielne. Aby przenieść RideMate na inny host, należy zmienić dostęp do runnera i kontrolę sąsiednich kontenerów w skrypcie wdrożenia; kod aplikacji i baza nie zależą od schematu ani API Atlasa.

Po otwarciu osobnego tunelu SSH w drugim terminalu można uruchomić .\scripts\start-remote-mobile.ps1 -LanIp YOUR_COMPUTER_LAN_IP. Najpierw zakończ poprzedni serwer Expo na porcie 8082 (Ctrl+C). Skrypt sprawdza dostępność zdalnego API przez tunel przed zmianą adresu klienta. Hasła SSH wpisuje się wyłącznie w terminalu. Na fizycznym iPhonie nie wykonano jeszcze testu.
