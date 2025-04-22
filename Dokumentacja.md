## 1. Tytuł projektu

* **Nazwa aplikacji:** Zielarnia
* **Krótki opis:** Aplikacja konsolowa w C# (.NET 8) do zarządzania podstawowymi operacjami w sklepie zielarskim, z uwzględnieniem ról użytkowników i logowania zdarzeń.

## 2. Opis projektu

* **Cel projektu:** Stworzenie funkcjonalnej aplikacji konsolowej demonstrującej praktyczne zastosowanie zasad programowania obiektowego (OOP) w języku C#. Aplikacja symuluje system zarządzania dla małego sklepu zielarskiego, obejmując logowanie użytkowników, zarządzanie danymi (produkty, klienci, zamówienia, dostawcy, rachunki) w bazie danych MariaDB oraz logowanie zdarzeń systemowych. Projekt realizuje wymagania zadania projektowego, w tym zastosowanie klas, dziedziczenia, interfejsów, delegatów i zdarzeń.
* **Co robi aplikacja:** Umożliwia użytkownikom (Client, Herbalist, Admin) logowanie się na podstawie danych z pliku tekstowego. W zależności od przypisanej roli, użytkownik otrzymuje dostęp do różnych funkcji: przeglądanie oferty, składanie zamówień, zarządzanie asortymentem, kategoriami, danymi klientów i dostawców, przetwarzanie zamówień oraz zarządzanie podstawową dokumentacją finansową (typy rachunków, rachunki). Wszystkie istotne akcje i błędy są rejestrowane w pliku logów.
* **Dla kogo jest przeznaczona:** Aplikacja jest przeznaczona jako projekt edukacyjny oraz jako prototyp prostego systemu informatycznego dla małego sklepu specjalistycznego, obsługującego różne poziomy dostępu użytkowników.

## 3. Technologie

* **Język programowania:** C#
* **Platforma / Środowisko:** .NET 8.0 (aplikacja konsolowa)
* **IDE:** Visual Studio 2022 (lub nowsze)
* **Baza Danych:** MariaDB (lub kompatybilna, np. MySQL)
* **Główne Biblioteki NuGet:**
    * `MySqlConnector`: Biblioteka do komunikacji z bazą danych MariaDB/MySQL.
    * `BCrypt.Net-Next`: Biblioteka do bezpiecznego hashowania i weryfikacji haseł (implementacja algorytmu BCrypt).
    * `Microsoft.Extensions.Configuration`: Do wczytywania i obsługi konfiguracji aplikacji.
    * `Microsoft.Extensions.Configuration.Binder`: Umożliwia powiązanie konfiguracji z obiektami C#.
    * `Microsoft.Extensions.Configuration.Json`: Umożliwia odczyt konfiguracji z plików `.json` (np. `appsettings.json`).

## 4. Struktura katalogów

Projekt jest zorganizowany w następujący sposób, aby oddzielić warstwy aplikacji:

```plaintext
Zielarnia/ (Folder główny projektu, zawiera plik .sln)
│
└── Zielarnia/ (Folder projektu C#)
    │
    ├── Zielarnia.csproj        # Plik projektu C# (zawiera listę pakietów NuGet)
    ├── Program.cs              # Główny punkt wejścia aplikacji, inicjalizacja
    ├── appsettings.json        # Plik konfiguracyjny (połączenie DB, ścieżki plików)
    ├── users.txt               # Plik z danymi logowania użytkowników (tworzony ręcznie)
    ├── baza.sql                # Skrypt SQL do utworzenia struktury bazy danych i danych startowych
    │
    ├── Core/                   # Rdzeń logiki biznesowej
    │   ├── Events/ UserActionEventArgs.cs
    │   ├── Interfaces/ ILoggerService.cs
    │   ├── Models/
    │   │   ├── Base/ EntityBase.cs
    │   │   └── Enums/ OrderStatus.cs, UserRole.cs
    │   │   └── User.cs
    │   └── Services/
    │       ├── AuthenticationService.cs
    │       ├── FileLoggerService.cs
    │       └── MenuService.cs
    │
    ├── Data/                   # Warstwa dostępu do danych
    │   ├── Models/             # Modele = Tabele DB
    │   │   ├── Address.cs, Bill.cs, BillType.cs, Category.cs, Client.cs,
    │   │   │   Delivery.cs, Order.cs, OrderDetail.cs, Product.cs, Supplier.cs
    │   └── Services/
    │       └── DatabaseService.cs
    │
    └── UI/                     # Interfejs użytkownika (konsola)
        └── ConsoleHelper.cs
```

## 5. Instrukcja instalacji i uruchomienia (Visual Studio 2022)

### Wymagania systemowe

- Zainstalowane **Visual Studio 2022** (dowolna edycja, np. Community) z zainstalowanym obciążeniem (workload) **".NET desktop development"**.
- Zainstalowany **.NET 8.0 SDK** (zazwyczaj instaluje się razem z odpowiednim obciążeniem VS 2022).
- Działający serwer **MariaDB** (wersja 10.x lub nowsza) lub kompatybilny **MySQL**.

### Kroki instalacji

1. **Pobranie Projektu:** Skopiuj cały folder projektu `Zielarnia` (zawierający plik `.sln` i podfolder z projektem `.csproj`) na swój komputer.
    
2. **Konfiguracja Bazy Danych:**
    
    - Uruchom swój serwer MariaDB/MySQL.
    - Połącz się z serwerem używając narzędzia do zarządzania bazą danych (np. HeidiSQL, DBeaver, MySQL Workbench lub nawet SQL Server Object Explorer w Visual Studio, jeśli masz zainstalowane odpowiednie rozszerzenia/narzędzia).
    - Utwórz nową bazę danych o nazwie `zielarnia` z kodowaniem `utf8mb4_polish_ci`. Przykładowe polecenie SQL:
        
        ```sql
        CREATE DATABASE zielarnia COLLATE 'utf8mb4_polish_ci';
        ```
        
    - Wybierz nowo utworzoną bazę (`USE zielarnia;`).
    - Wykonaj **cały** skrypt z pliku `baza.sql` (znajdującego się w folderze projektu C# `Zielarnia/`) na bazie `zielarnia`. Skrypt utworzy tabele i wstawi dane.
3. **Otwarcie Projektu w Visual Studio:**
    
    - Uruchom Visual Studio 2022.
    - Wybierz `File` -> `Open` -> `Project/Solution...` i wskaż plik `.sln` w głównym folderze projektu.
4. **Konfiguracja Aplikacji (`appsettings.json`):**
    
    - W oknie **Solution Explorer** znajdź plik `appsettings.json` w projekcie `Zielarnia`.
    - Otwórz go.
    - W sekcji `ConnectionStrings` -> `DefaultConnection` **dostosuj wartości** `Server`, `Database` (powinno być `zielarnia`), `User` i `Password` do ustawień Twojego serwera MariaDB.
    - Upewnij się, że sekcja `UserSettings` zawiera wpisy `UsersFilePath` i `LogsFilePath`:
        
        ```json
        {
          "ConnectionStrings": {
            "DefaultConnection": "Server=localhost;Database=zielarnia;User=root;Password=root;" // <-- ZMIEŃ!
          },
          "UserSettings": {
            "UsersFilePath": "users.txt",
            "LogsFilePath": "logs.txt"
          }
        }
        ```
        
5. **Instalacja Pakietów NuGet:**
    
    - Kliknij prawym przyciskiem myszy na projekt `Zielarnia` w **Solution Explorer**.
    - Wybierz `Manage NuGet Packages...`.
    - Przejdź do zakładki `Browse`.
    - Wyszukaj i zainstaluj kolejno następujące pakiety (jeśli nie są już zainstalowane):
        - `MySqlConnector`
        - `BCrypt.Net-Next`
        - `Microsoft.Extensions.Configuration`
        - `Microsoft.Extensions.Configuration.Binder`
        - `Microsoft.Extensions.Configuration.Json`
    - Alternatywnie, otwórz `Package Manager Console` (`Tools` -> `NuGet Package Manager` -> `Package Manager Console`) i wykonaj polecenia:
        
        ```
        Install-Package MySqlConnector
        Install-Package BCrypt.Net-Next
        Install-Package Microsoft.Extensions.Configuration
        Install-Package Microsoft.Extensions.Configuration.Binder
        Install-Package Microsoft.Extensions.Configuration.Json
        ```
        
6. **Zaznajomienie się z plikiem users.txt**:

	```
	login: admin, hasło: admin123, rola: Admin
	login: zielarz, hasło zielarz123, rola: Herbalist
	login: kajetan52@example.net, hasło: kajetan123, rola: Client
	```
### Uruchomienie Aplikacji

1. Upewnij się, że projekt `Zielarnia` jest ustawiony jako projekt startowy (nazwa projektu powinna być pogrubiona w Solution Explorer. Jeśli nie jest, kliknij na nią prawym przyciskiem i wybierz `Set as Startup Project`).
2. Naciśnij klawisz **F5** lub kliknij zielony przycisk "Start" (zazwyczaj z nazwą projektu obok) na pasku narzędzi Visual Studio.
3. Aplikacja powinna się skompilować i uruchomić w nowym oknie konsoli.

## 6. Opis działania aplikacji

### Przepływ programu

Po uruchomieniu aplikacja wita użytkownika i prosi o podanie loginu i hasła. Dane te są weryfikowane z zawartością pliku `users.txt`. Po pomyślnym zalogowaniu wyświetlane jest menu główne dostosowane do roli zalogowanego użytkownika. Użytkownik nawiguje po menu, wybierając numerycznie opcje. Wybór opcji skutkuje wykonaniem określonej akcji (np. wyświetlenie listy, przejście do podmenu zarządzania, dodanie rekordu do bazy danych). Wiele opcji zarządzania posiada własne podmenu (np. Lista, Dodaj, Edytuj, Usuń). Po wykonaniu akcji lub wyjściu z podmenu, aplikacja zwykle czeka na naciśnięcie klawisza przez użytkownika, po czym wraca do odpowiedniego menu. Wybranie opcji '0' w menu głównym kończy działanie aplikacji. W tle, kluczowe akcje i błędy są zapisywane do pliku `logs.txt`.

### Funkcjonalności wg Ról

#### Klient (`Client`)

- **1. Wyświetl produkty:** Pokazuje listę wszystkich produktów (ID, Nazwa, Cena, Kat.ID, Opis).
- **2. Złóż nowe zamówienie:** Wyświetla listę produktów. Użytkownik może dodawać produkty do zamówienia, podając ich ID i ilość. Na koniec wyświetla podsumowanie. Po potwierdzeniu (`T`), zamówienie jest zapisywane w bazie danych (status 'Nowe').
- **3. Wyświetl moje zamówienia:** Pokazuje listę zamówień (ID, Data, Status) złożonych przez zalogowanego klienta. Umożliwia wyświetlenie szczegółów wybranego zamówienia.
- **0. Wyloguj i zakończ:** Wyjście z aplikacji.

#### Zielarz (`Herbalist`)

Posiada wszystkie uprawnienia Klienta oraz dodatkowo:

- **10. Zarządzaj produktami:** Podmenu _Lista_, _Dodaj_, _Edytuj_, _Usuń_.
- **11. Zarządzaj kategoriami:** Podmenu _Lista_, _Dodaj_, _Edytuj_, _Usuń_.
- **12. Zarządzaj klientami:** Podmenu _Lista_, _Dodaj_, _Edytuj_, _Usuń_, _Pokaż adres_.
- **13. Zarządzaj zamówieniami:** Wyświetla zamówienia 'Nowe'/'W trakcie', pozwala zmienić status.
- **14. Zarządzaj dostawcami:** Podmenu _Lista_, _Dodaj_, _Edytuj_, _Usuń_, _Pokaż adres_.
- **15. Zarządzaj powiązaniami dostawców:** Podmenu _Lista_, _Dodaj_, _Usuń_ powiązań Dostawca-Produkt.
- **16. Wyświetl wszystkie zamówienia:** Lista wszystkich zamówień, umożliwia podgląd szczegółów.

#### Administrator (`Admin`)

Posiada wszystkie uprawnienia Zielarza oraz dodatkowo:

- **20. Zarządzaj typami rachunków:** Podmenu _Lista_, _Dodaj_, _Edytuj_, _Usuń_.
- **21. Zarządzaj rachunkami:** Podmenu _Lista_, _Dodaj_, _Edytuj_.

## 7. Przykłady użycia

Poniższe przykłady pokazują, jak wykonywać typowe zadania w aplikacji Zielarnia, krok po kroku, dla różnych ról użytkowników.

### 7.1. Logowanie do systemu

1.  Uruchom aplikację (np. poleceniem `dotnet run` w terminalu w folderze projektu).
2.  Gdy pojawi się prośba `Podaj login:`, wpisz swój login (np. `admin`, `zielarz` lub `kajetan52@example.net`) i naciśnij Enter.
3.  Gdy pojawi się prośba `Podaj hasło:`, wpisz swoje hasło (np. `admin123`, `zielarz123` lub `kajetan123` - znaki nie będą widoczne) i naciśnij Enter.
4.  Jeśli dane są poprawne, zobaczysz komunikat o pomyślnym zalogowaniu i po naciśnięciu klawisza przejdziesz do Menu Głównego odpowiedniego dla Twojej roli.

### 7.2. Klient: Przeglądanie produktów

1.  Zaloguj się jako `kajetan52@example.net` (rola `Client`).
2.  W Menu Głównym wybierz opcję `1` (Wyświetl produkty) i naciśnij Enter.
3.  Na ekranie pojawi się lista dostępnych produktów z ich ID, nazwą, ceną, ID kategorii i opisem.
4.  Naciśnij dowolny klawisz, aby wrócić do Menu Głównego.

### 73. Klient: Składanie zamówienia

1.  Zaloguj się jako `kajetan52@example.net` (rola `Client`).
2.  W Menu Głównym wybierz opcję `2` (Złóż nowe zamówienie).
3.  Aplikacja wyświetli listę dostępnych produktów z ich ID.
4.  Gdy pojawi się prośba `Podaj ID produktu (lub 0 aby zakończyć):`, wpisz ID produktu, który chcesz dodać (np. `1` dla Rumianku) i naciśnij Enter.
5.  Gdy pojawi się prośba `Podaj ilość dla 'Rumianek lekarski':`, wpisz żądaną ilość (np. `2`) i naciśnij Enter. Zobaczysz potwierdzenie dodania.
6.  Powtórz kroki 4-5 dla innych produktów lub wpisz `0`, aby przejść do podsumowania.
7.  Aplikacja wyświetli podsumowanie zamówienia z listą produktów, ilościami, cenami i sumą całkowitą.
8.  Gdy pojawi się pytanie `Czy chcesz złożyć to zamówienie? (T/N):`, wpisz `T` i naciśnij Enter, aby potwierdzić.
9.  Zobaczysz komunikat o pomyślnym złożeniu zamówienia wraz z jego numerem ID.
10. Naciśnij dowolny klawisz, aby wrócić do Menu Głównego.

### 7.4. Klient: Wyświetlanie szczegółów zamówienia

1.  Zaloguj się jako `kajetan52@example.net` (rola `Client`).
2.  W Menu Głównym wybierz opcję `3` (Wyświetl moje zamówienia).
3.  Aplikacja pokaże listę Twoich poprzednich zamówień (ID, Data, Status).
4.  Gdy pojawi się prośba `Podaj ID zamówienia, aby zobaczyć szczegóły (lub 0):`, wpisz ID zamówienia z listy, którego szczegóły chcesz zobaczyć, i naciśnij Enter.
5.  Aplikacja wyświetli listę produktów w tym zamówieniu, ich ilości, ceny jednostkowe i sumę całkowitą.
6.  Naciśnij dowolny klawisz, aby wrócić do Menu Głównego.

### 7.5. Zielarz/Admin: Dodawanie nowego produktu

1.  Zaloguj się jako `zielarz` lub `admin` (role `Herbalist` lub `Admin`).
2.  W Menu Głównym wybierz opcję `10` (Zarządzaj produktami).
3.  W podmenu zarządzania produktami wybierz opcję `2` (Dodaj nowy produkt).
4.  Podaj kolejno żądane informacje:
    * `Nazwa produktu:` (np. `Lawenda lekarska`)
    * `Opis (opcjonalnie):` (np. `Suszone kwiaty lawendy, działanie uspokajające.` lub naciśnij Enter)
    * `Cena (np. 10.99):` (np. `16.50`)
    * Wybierz ID kategorii z wyświetlonej listy (np. `1` dla "Zioła lecznicze" lub `0` dla braku).
5.  Aplikacja potwierdzi dodanie produktu.
6.  Naciśnij dowolny klawisz, aby wrócić do podmenu zarządzania produktami. Naciśnij `0`, aby wrócić do Menu Głównego.

### 7.6. Zielarz/Admin: Edycja produktu

1.  Zaloguj się jako `zielarz` lub `admin`.
2.  Wybierz opcję `10` (Zarządzaj produktami).
3.  Wybierz opcję `3` (Edytuj produkt).
4.  Aplikacja wyświetli listę produktów.
5.  Podaj `ID produktu do edycji (0=anuluj):`.
6.  Aplikacja pokaże obecne dane produktu. Podaj nowe wartości lub naciśnij Enter, aby zostawić stare:
    * `Nowa nazwa (...)`
    * `Nowy opis (...)`
    * `Nowa cena (...)`
    * Wybierz nowe ID kategorii z listy.
7.  Aplikacja potwierdzi aktualizację (jeśli wprowadzono zmiany) lub poinformuje o braku zmian.
8.  Naciśnij klawisz, aby wrócić do podmenu.

### 7.7. Zielarz/Admin: Zmiana statusu zamówienia

1.  Zaloguj się jako `zielarz` lub `admin`.
2.  Wybierz opcję `13` (Zarządzaj zamówieniami).
3.  Aplikacja wyświetli listę zamówień o statusie 'Nowe' lub 'W trakcie'.
4.  Podaj `ID zamówienia do zmiany statusu (lub 0 aby wrócić):`.
5.  Aplikacja wyświetli listę możliwych nowych statusów (ponumerowaną).
6.  Wpisz `Numer nowego statusu:` (np. `3` dla 'Zrealizowane').
7.  Potwierdź zmianę, wpisując `T`.
8.  Aplikacja potwierdzi zmianę statusu.
9.  Naciśnij klawisz, aby wrócić do podmenu.

### 7.8. Admin: Dodawanie nowego typu rachunku

1.  Zaloguj się jako `admin`.
2.  Wybierz opcję `20` (Zarządzaj typami rachunków).
3.  W podmenu wybierz opcję `2` (Dodaj typ).
4.  Podaj `Nazwa typu:` (np. `Koszty Marketingowe`).
5.  Podaj `Opis (opcjonalnie):` (np. `Wydatki na reklamę i promocję.` lub Enter).
6.  Aplikacja potwierdzi dodanie typu.
7.  Naciśnij klawisz, aby wrócić do podmenu.

### 7.9. Admin: Dodawanie nowego rachunku

1.  Zaloguj się jako `admin`.
2.  Wybierz opcję `21` (Zarządzaj rachunkami).
3.  W podmenu wybierz opcję `2` (Dodaj nowy rachunek).
4.  Aplikacja wyświetli listę dostępnych typów rachunków.
5.  Podaj `Wybierz ID Typu rachunku:` na podstawie listy.
6.  Podaj `Data rachunku (RRRR-MM-DD lub Enter dla dzisiaj):` (np. `2025-04-19` lub Enter).
7.  Podaj `Kwota rachunku:` (np. `150.75`).
8.  Aplikacja potwierdzi dodanie rachunku.
9.  Naciśnij klawisz, aby wrócić do podmenu.

### 7.10. Wylogowanie

1.  Będąc w Menu Głównym (niezależnie od roli), wybierz opcję `0` (Wyloguj i zakończ).
2.  Aplikacja wyświetli komunikat pożegnalny i zakończy działanie (lub w przypadku uruchomienia z VS, poprosi o naciśnięcie klawisza przed zamknięciem okna konsoli).

## 8. Struktury danych i klasy

Aplikacja została zbudowana z wykorzystaniem podstawowych zasad programowania obiektowego, dzieląc logikę na współpracujące ze sobą klasy:

* **Modele (`Data/Models`)**: Zestaw klas (`Product`, `Client`, `Order`, `Category`, `Address`, `Supplier`, `Delivery`, `OrderDetail`, `Bill`, `BillType`) odzwierciedlających strukturę tabel w bazie danych MariaDB. Służą do przechowywania i przekazywania danych między warstwami aplikacji.
* **Dziedziczenie (`Core/Models/Base/EntityBase.cs`)**: Większość modeli danych dziedziczy po abstrakcyjnej klasie `EntityBase`, która dostarcza wspólną właściwość `Id` (klucz główny). Spełnia to wymóg wykorzystania dziedziczenia w projekcie.
* **Interfejsy (`Core/Interfaces/ILoggerService.cs`)**: Zdefiniowano interfejs `ILoggerService`, który określa kontrakt dla mechanizmu logowania zdarzeń w aplikacji. Zapewnia to elastyczność i możliwość łatwej podmiany implementacji logowania w przyszłości.
* **Implementacja Interfejsu (`Core/Services/FileLoggerService.cs`)**: Klasa `FileLoggerService` implementuje interfejs `ILoggerService`, realizując funkcjonalność zapisywania logów do pliku tekstowego (`logs.txt`).
* **Delegaty i Zdarzenia (`Core/Services/AuthenticationService.cs`, `Core/Events/UserActionEventArgs.cs`)**: W serwisie `AuthenticationService` zdefiniowano delegat `UserActionEventHandler` oraz zdarzenia `UserLoggedIn` i `LoginAttempt`. Umożliwiają one powiadamianie innych części systemu (w tym przypadku `Program.cs`, który subskrybuje te zdarzenia na potrzeby logowania) o istotnych akcjach związanych z procesem uwierzytelniania, bez ścisłego powiązania tych komponentów. Klasa `UserActionEventArgs` przenosi dane związane ze zdarzeniem.
* **Serwisy (`Core/Services`, `Data/Services`)**: Centralne miejsce logiki aplikacji:
    * `DatabaseService`: Odpowiada za wszystkie interakcje z bazą danych (tworzenie połączenia, wykonywanie zapytań SQL, podstawowe mapowanie wyników). Używa `MySqlConnector` i dba o parametryzację zapytań.
    * `AuthenticationService`: Zarządza procesem logowania – odczytuje plik `users.txt`, weryfikuje hasła za pomocą BCrypt, tworzy obiekt `User` i wywołuje odpowiednie zdarzenia.
    * `MenuService`: Serce aplikacji – steruje przepływem, wyświetla odpowiednie menu dla zalogowanego użytkownika, obsługuje jego wybory, koordynuje wywołania innych serwisów (głównie `DatabaseService` i `ConsoleHelper`) oraz implementuje logikę poszczególnych funkcji zarządzania.
* **Klasy Pomocnicze (`UI/ConsoleHelper.cs`)**: Dostarcza statycznych metod ułatwiających pracę z konsolą – wyświetlanie nagłówków, komunikatów (sukcesu, ostrzeżenia, błędu) w kolorach, bezpieczne wczytywanie danych od użytkownika (string, int, decimal, hasło, potwierdzenie T/N) wraz z podstawową walidacją formatu i pętlami ponawiania w razie błędu. Implementuje też obejście problemu czyszczenia bufora konsoli.
* **Punkt wejścia (`Program.cs`)**: Inicjalizuje konfigurację, tworzy instancje serwisów (przekazując zależności, np. logger do innych serwisów), subskrybuje obsługę zdarzeń logowania, obsługuje główną pętlę logowania użytkownika i uruchamia `MenuService` po udanym logowaniu.

## 9. Obsługa błędów

Aplikacja została wyposażona w mechanizmy przechwytywania i obsługi typowych błędów mogących wystąpić podczas jej działania:

* **Błędy związane z plikami:**
    * Nieznalezienie pliku `users.txt` lub `appsettings.json` przy starcie (uniemożliwia działanie).
    * Problemy z uprawnieniami do odczytu `users.txt` lub zapisu do `logs.txt` (`IOException`, `UnauthorizedAccessException`).
    * Nieprawidłowy format danych w pliku `users.txt` (np. zła liczba pól w linii).
* **Błędy Konfiguracji:** Brak kluczowych wpisów w `appsettings.json` (np. `ConnectionStrings:DefaultConnection`, `UserSettings:UsersFilePath`, `UserSettings:LogsFilePath`).
* **Błędy Bazy Danych (`MySqlException`, `DbException`):**
    * Nieudane połączenie z serwerem MariaDB (np. zły adres, port, dane logowania, serwer nie działa).
    * Błędy składniowe w zapytaniach SQL.
    * Naruszenie ograniczeń bazy danych (np. próba dodania rekordu z wartością `UNIQUE`, która już istnieje – obsłużone specyficznie dla nazw typów rachunków).
    * Problemy z kluczami obcymi.
* **Błędy Danych/Mapowania:**
    * Próba odczytania nieistniejącej kolumny z wyniku zapytania (`IndexOutOfRangeException`).
    * Próba konwersji typu danych z bazy na niezgodny typ C# (`InvalidCastException`).
    * Błędy przy parsowaniu statusu zamówienia lub roli użytkownika (`ArgumentException`).
* **Błędy Logowania:** Nieprawidłowy login lub hasło, nieistniejący użytkownik, nieprawidłowy format hasha BCrypt.
* **Błędy Wprowadzania Danych przez Użytkownika:** Podanie tekstu zamiast oczekiwanej liczby, podanie liczby spoza dozwolonego zakresu, pozostawienie pustego pola tam, gdzie jest ono wymagane. Są one obsługiwane przez pętle w `ConsoleHelper`, prosząc o ponowne wprowadzenie danych.
* **Błędy Logiki Aplikacji:** Np. próba edycji lub usunięcia rekordu o ID, które nie istnieje w bazie (aplikacja zazwyczaj sprawdza istnienie rekordu przed operacją i wyświetla ostrzeżenie).

**Komunikacja i Rejestrowanie Błędów:** Błędy krytyczne są sygnalizowane komunikatem `[BŁĄD]` w konsoli, często kończąc działanie aplikacji lub danej funkcji. Mniej krytyczne problemy (np. nieprawidłowe dane wejściowe) są sygnalizowane jako `[UWAGA]`. Większość wyjątków (szczególnie te związane z plikami, bazą danych lub nieoczekiwane) jest przechwytywana, a ich szczegóły (komunikat, ślad stosu) są zapisywane do pliku `logs.txt` przez `FileLoggerService`, co ułatwia diagnozę problemów.

## 10. Testowanie

* **Metodologia:** Aplikacja została przetestowana **wyłącznie manualnie**.
* **Zakres Testów:**
    * Sprawdzono proces logowania dla każdej z trzech ról (`Client`, `Herbalist`, `Admin`) z poprawnymi i niepoprawnymi danymi.
    * Przetestowano działanie wszystkich opcji menu dostępnych dla każdej roli.
    * Zweryfikowano poprawność operacji CRUD (Create, Read, Update, Delete) na wszystkich zarządzanych encjach (Produkty, Kategorie, Klienci, Adresy, Dostawcy, Powiązania Dostaw, Typy Rachunków, Rachunki) poprzez interakcję z aplikacją i sprawdzanie wyników bezpośrednio w bazie danych MariaDB.
    * Przetestowano działanie mechanizmu składania zamówień i zmiany ich statusu.
    * Sprawdzono obsługę błędów poprzez wprowadzanie nieprawidłowych danych (np. tekst zamiast liczby, ID nieistniejących rekordów, próba dodania rekordu naruszającego ograniczenia `UNIQUE`).
    * Zweryfikowano poprawność zapisywanych logów w pliku `logs.txt` dla różnych akcji i błędów.
    * Sprawdzono działanie obejścia problemu czyszczenia konsoli.
* **Testy Jednostkowe/Integracyjne:** W projekcie **nie zaimplementowano** żadnych zautomatyzowanych testów jednostkowych (np. xUnit, NUnit) ani testów integracyjnych. Weryfikacja poprawności opierała się na testach manualnych.

## 11. Problemy i ograniczenia

Podczas implementacji i testowania zidentyfikowano pewne obszary, które stanowią ograniczenia obecnej wersji lub mogłyby zostać ulepszone:

* **Interfejs Użytkownika:** Jako aplikacja konsolowa, interfejs jest ograniczony do tekstu i prostych menu. Brakuje wizualnych elementów i bardziej zaawansowanych mechanizmów interakcji, co może utrudniać obsługę przy większej ilości danych. Problem czyszczenia bufora konsoli jest rozwiązany tylko częściowo przez obejście.
* **Zarządzanie Użytkownikami:** Przechowywanie użytkowników i hashy w pliku tekstowym `users.txt` jest rozwiązaniem prostym, ale niebezpiecznym i niewygodnym w zarządzaniu w porównaniu do przechowywania ich w bazie danych. Aplikacja nie oferuje funkcji zarządzania użytkownikami (dodawania, usuwania, zmiany hasła) z poziomu interfejsu.
* **Walidacja Danych:** Walidacja danych wprowadzanych przez użytkownika jest podstawowa (sprawdza typ, czy nie jest puste). Brakuje bardziej szczegółowej walidacji (np. formatu adresu email, NIP-u dostawcy, zakresu cen, maksymalnej długości nazw) oraz sprawdzania unikalności danych (np. email klienta) przed próbą zapisu do bazy.
* **Funkcjonalność Dostaw:** Obecna implementacja (`Dostawy`) pozwala jedynie na zarządzanie powiązaniami między dostawcami a produktami (który dostawca może dostarczyć dany produkt). Nie obsługuje rejestrowania konkretnych dostaw (daty, ilości, ceny zakupu), co wymagałoby rozbudowy schematu bazy.
* **Zarządzanie Adresami:** Adresy są powiązane z klientami i dostawcami, ale usunięcie klienta/dostawcy nie usuwa adresu, co może prowadzić do powstawania nieużywanych ("osieroconych") rekordów adresów w bazie.
* **Wydajność:** Przy bardzo dużej liczbie rekordów w tabelach (np. produkty, zamówienia, rachunki), metody pobierające wszystkie dane naraz (`GetAll...Async`) mogą działać wolno. Brakuje mechanizmów stronicowania (paginacji) wyników. Istnieje też potencjalne ryzyko problemu N+1 w miejscach pobierania danych powiązanych (choć starano się je minimalizować np. przez pobieranie słowników nazw typów).
* **Brak Testów Automatycznych:** Brak testów jednostkowych i integracyjnych utrudnia weryfikację poprawności po wprowadzeniu zmian i potencjalnie zwiększa ryzyko regresji.

## 13. Autorzy

* Dawid Hadas
* Igor Zieliński