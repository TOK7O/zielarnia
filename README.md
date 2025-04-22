# Zielarnia - Aplikacja Konsolowa

Prosta aplikacja konsolowa w C# (.NET 8) do zarządzania sklepem zielarskim "Zielarnia". Projekt demonstruje podstawy OOP, obsługę bazy danych MariaDB oraz system logowania oparty na rolach.

**W repozytorium znajduje się również obszerna dokumentacja projektu.**

## Kluczowe Funkcjonalności

* Logowanie użytkowników w oparciu o plik `users.txt` (hasła hashowane BCrypt).
* System ról (Client, Herbalist, Admin) z różnymi uprawnieniami.
* Zarządzanie Produktami i Kategoriami.
* Zarządzanie Klientami i ich Adresami.
* Składanie Zamówień i zarządzanie ich Statusem.
* Zarządzanie Dostawcami i Powiązaniami Dostawca-Produkt.
* Zarządzanie Typami Rachunków i Rachunkami (dla Admina).
* Logowanie zdarzeń i błędów do pliku `logs.txt`.

## Technologie

* C# / .NET 8.0
* MariaDB (lub MySQL)
* MySqlConnector (NuGet)
* BCrypt.Net-Next (NuGet)
* Microsoft.Extensions.Configuration (NuGet)

## Szybki Start (Instalacja i Uruchomienie)

1.  **Wymagania:** .NET 8.0 SDK, serwer MariaDB/MySQL.
2.  **Baza Danych:** Utwórz bazę danych `zielarnia` (COLLATE `utf8mb4_polish_ci`) i wykonaj na niej skrypt `baza.sql`.
3.  **Konfiguracja:** Edytuj plik `appsettings.json`, dostosowując `ConnectionStrings.DefaultConnection` do swojej bazy danych. Upewnij się, że istnieje sekcja `UserSettings` ze ścieżkami do `users.txt` i `logs.txt`.
4.  **Użytkownicy:** Utwórz plik `users.txt` w głównym folderze projektu. Dodaj użytkowników w formacie `login,hash_bcrypt,rola`. **Ważne:** Hasła muszą być zahashowane! Użyj dołączonego kodu pomocniczego (w pełnej dokumentacji lub wygeneruj sam) dla poniższych danych:
    * `admin` (rola: `Admin`), hasło: `admin123`
    * `zielarz` (rola: `Herbalist`), hasło: `zielarz123`
    * `kajetan52@example.net` (rola: `Client`), hasło: `kajetan123` (Login musi zgadzać się z emailem w bazie!)
5.  **Pakiety NuGet:** Otwórz terminal w folderze projektu i wykonaj `dotnet restore`.
6.  **Uruchomienie:** W terminalu w folderze projektu wykonaj `dotnet run`.

## Domyślni Użytkownicy (Przykładowe Dane Logowania)

* Login: `admin`, Hasło: `admin123`, Rola: `Admin`
* Login: `zielarz`, Hasło: `zielarz123`, Rola: `Herbalist`
* Login: `kajetan52@example.net`, Hasło: `kajetan123`, Rola: `Client`

*(Pamiętaj o ustawieniu poprawnych **hashy** haseł w pliku `users.txt`!)*