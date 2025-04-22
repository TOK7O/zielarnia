-- Tworzenie bazy danych
DROP DATABASE IF EXISTS zielarnia;
CREATE DATABASE zielarnia COLLATE 'utf8mb4_polish_ci';
USE zielarnia;

-- Tworzenie tabeli adresy
CREATE TABLE IF NOT EXISTS adresy (
   id INTEGER AUTO_INCREMENT,
   kraj VARCHAR(100) NOT NULL,
   miasto VARCHAR(50) NOT NULL,
   ulica VARCHAR(50) NOT NULL,
   numer_budynku VARCHAR(5) NOT NULL,
   numer_mieszkania VARCHAR(4),
   PRIMARY KEY(id)
);

-- Tworzenie tabeli Klienci
CREATE TABLE IF NOT EXISTS Klienci (
   id INTEGER AUTO_INCREMENT,
   imie VARCHAR(50) NOT NULL,
   nazwisko VARCHAR(50) NOT NULL,
   email VARCHAR(100) NOT NULL UNIQUE,
   telefon VARCHAR(20) NOT NULL UNIQUE,
   adres_id INTEGER NOT NULL,
   id_paczkomatu VARCHAR(5),
   PRIMARY KEY(id),
   FOREIGN KEY(adres_id) REFERENCES adresy(id)
);

-- Tworzenie tabeli Kategorie
CREATE TABLE IF NOT EXISTS Kategorie (
   id INTEGER AUTO_INCREMENT,
   nazwa VARCHAR(50) NOT NULL UNIQUE,
   PRIMARY KEY(id)
);

-- Tworzenie tabeli Produkty
CREATE TABLE IF NOT EXISTS Produkty (
   id INTEGER AUTO_INCREMENT,
   nazwa VARCHAR(100) NOT NULL,
   opis TEXT,
   cena DECIMAL(10,2) NOT NULL,
   kategoria_id INTEGER,
   PRIMARY KEY(id),
   FOREIGN KEY(kategoria_id) REFERENCES Kategorie(id) ON DELETE RESTRICT
);

-- Tworzenie tabeli Zamowienia
CREATE TABLE IF NOT EXISTS Zamowienia (
   id INTEGER AUTO_INCREMENT,
   klient_id INTEGER,
   data_zamowienia DATETIME DEFAULT CURRENT_TIMESTAMP,
   status ENUM('Nowe', 'W trakcie', 'Zrealizowane', 'Anulowane') NOT NULL,
   PRIMARY KEY(id),
   FOREIGN KEY(klient_id) REFERENCES Klienci(id) ON DELETE CASCADE
);

-- Tworzenie tabeli SzczegolyZamowienia
CREATE TABLE IF NOT EXISTS SzczegolyZamowienia (
   zamowienie_id INTEGER,
   produkt_id INTEGER,
   ilosc INTEGER NOT NULL CHECK(ilosc > 0),
   PRIMARY KEY(zamowienie_id, produkt_id),
   FOREIGN KEY(zamowienie_id) REFERENCES Zamowienia(id) ON DELETE CASCADE,
   FOREIGN KEY(produkt_id) REFERENCES Produkty(id) ON DELETE CASCADE
);

-- Tworzenie tabeli Dostawcy
CREATE TABLE IF NOT EXISTS Dostawcy (
   id INTEGER AUTO_INCREMENT,
   nazwa VARCHAR(100) NOT NULL UNIQUE,
   kontakt VARCHAR(100),
   adres_id INTEGER,
   PRIMARY KEY(id),
   FOREIGN KEY(adres_id) REFERENCES adresy(id) ON DELETE RESTRICT
);

-- Tworzenie tabeli Dostawy
CREATE TABLE IF NOT EXISTS Dostawy (
   dostawca_id INTEGER,
   produkt_id INTEGER,
   PRIMARY KEY(dostawca_id, produkt_id),
   FOREIGN KEY(dostawca_id) REFERENCES Dostawcy(id) ON DELETE CASCADE,
   FOREIGN KEY(produkt_id) REFERENCES Produkty(id) ON DELETE CASCADE
);

-- Tworzenie tabeli typy_rachunkow
CREATE TABLE IF NOT EXISTS typy_rachunkow (
   id INTEGER AUTO_INCREMENT,
   typ VARCHAR(30) NOT NULL,
   opis TEXT,
   PRIMARY KEY(id)
);

-- Tworzenie tabeli rachunki
CREATE TABLE IF NOT EXISTS rachunki (
   id INTEGER AUTO_INCREMENT,
   data DATE NOT NULL,
   typ INTEGER,
   kwota DECIMAL(10,2) NOT NULL,
   PRIMARY KEY(id), -- Dodano PRIMARY KEY
   FOREIGN KEY(typ) REFERENCES typy_rachunkow(id) ON DELETE SET NULL
);

-- Indeksy B-tree
CREATE INDEX idx_produkty_nazwa ON Produkty(nazwa);
CREATE INDEX idx_klienci_email ON Klienci(email);

-- Wyłączanie sprawdzania kluczy obcych
SET FOREIGN_KEY_CHECKS = 0;

INSERT INTO Kategorie (nazwa) VALUES
('Zioła lecznicze'),
('Herbaty ziołowe i mieszanki'),
('Suplementy diety'),
('Kosmetyki naturalne'),
('Oleje i maceraty'),
('Przyprawy i suszone rośliny'),
('Miodowe i pszczele produkty'),
('Eteryczne olejki zapachowe'),
('Ekologiczne produkty spożywcze'),
('Akcesoria zielarskie');

INSERT INTO Produkty (nazwa, opis, cena, kategoria_id) VALUES
('Rumianek lekarski', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus lacinia odio vitae vestibulum. Fusce nec ligula ut felis feugiat malesuada.', 12.99, 1),
('Pokrzywa suszona', 'Suspendisse potenti. Proin scelerisque, sapien sed scelerisque auctor, felis elit pharetra justo, non cursus lorem metus nec arcu.', 9.50, 1),
('Dziurawiec zwyczajny', 'Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Nullam quis risus eget urna mollis ornare vel eu leo.', 14.00, 1),
('Skrzyp polny', 'Integer posuere erat a ante venenatis dapibus posuere velit aliquet. Aenean eu leo quam. Pellentesque ornare sem lacinia quam venenatis vestibulum.', 11.30, 1),
('Mięta pieprzowa', 'Vestibulum id ligula porta felis euismod semper. Curabitur blandit tempus porttitor. Duis mollis, est non commodo luctus, nisi erat porttitor ligula.', 8.99, 1),
('Melisa lekarska', 'Fusce dapibus, tellus ac cursus commodo, tortor mauris condimentum nibh, ut fermentum massa justo sit amet risus.', 10.50, 1),
('Szałwia lekarska', 'Donec ullamcorper nulla non metus auctor fringilla. Maecenas sed diam eget risus varius blandit sit amet non magna.', 13.40, 1),
('Kozłek lekarski (waleriana)', 'Etiam porta sem malesuada magna mollis euismod. Aenean lacinia bibendum nulla sed consectetur.', 15.80, 1),
('Tymianek pospolity', 'Duis mollis, est non commodo luctus, nisi erat porttitor ligula, eget lacinia odio sem nec elit.', 9.20, 1),
('Kwiat lipy', 'Nullam id dolor id nibh ultricies vehicula ut id elit. Integer posuere erat a ante venenatis dapibus.', 11.90, 1),
('Herbata relaksacyjna „Wieczorny Spokój”', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 19.99, 2),
('Mieszanka na odporność „Zimowy Tarcza”', 'Suspendisse potenti. Proin scelerisque, sapien sed scelerisque auctor, felis elit pharetra justo.', 21.50, 2),
('Herbatka oczyszczająca „Zielone Ukojenie”', 'Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.', 18.00, 2),
('Napar trawienny „Zdrowy Brzuch”', 'Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 20.30, 2),
('Ziołowa kompozycja „Leśna Mgła”', 'Vestibulum id ligula porta felis euismod semper. Duis mollis, est non commodo luctus, nisi erat porttitor ligula.', 22.99, 2),
('Herbatka energetyzująca „Poranna Moc”', 'Fusce dapibus, tellus ac cursus commodo, tortor mauris condimentum nibh, ut fermentum massa justo sit amet risus.', 23.50, 2),
('Mieszanka na sen „Senna Melodia”', 'Donec ullamcorper nulla non metus auctor fringilla. Maecenas sed diam eget risus varius blandit.', 17.40, 2),
('Napar na serce „Zielone Serce”', 'Etiam porta sem malesuada magna mollis euismod.', 19.80, 2),
('Herbatka na stres „Spokojna Głowa”', 'Duis mollis, est non commodo luctus, nisi erat porttitor ligula.', 20.20, 2),
('Napar na kaszel „Ziołowa Ulga”', 'Nullam id dolor id nibh ultricies vehicula ut id elit.', 21.90, 2),
('Kapsułki z ashwagandhą', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 35.99, 3),
('Ekstrakt z żeń-szenia', 'Suspendisse potenti. Proin scelerisque, sapien sed scelerisque auctor, felis elit pharetra justo.', 40.50, 3),
('Witamina C z dzikiej róży', 'Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.', 29.00, 3),
('Suplement z wąkrotką azjatycką', 'Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 37.30, 3),
('Olej z czarnuszki w kapsułkach', 'Vestibulum id ligula porta felis euismod semper.', 41.99, 3),
('Proszek z młodego jęczmienia', 'Fusce dapibus, tellus ac cursus commodo.', 28.50, 3),
('Spirulina organiczna', 'Donec ullamcorper nulla non metus auctor fringilla.', 39.40, 3),
('Kurkuma z piperyną w kapsułkach', 'Etiam porta sem malesuada magna mollis euismod.', 34.80, 3),
('Krople z propolisem', 'Duis mollis, est non commodo luctus.', 27.20, 3),
('Chlorella w tabletkach', 'Nullam id dolor id nibh ultricies vehicula ut id elit.', 33.90, 3),
('Mydło lawendowe z masłem shea', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 14.99, 4),
('Balsam do ust z propolisem', 'Suspendisse potenti. Proin scelerisque, sapien sed scelerisque auctor, felis elit pharetra justo.', 7.50, 4),
('Krem do twarzy z nagietkiem', 'Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.', 19.00, 4),
('Olejek do włosów z pokrzywą', 'Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 22.50, 4),
('Dezodorant w kremie „Zielona Świeżość”', 'Vestibulum id ligula porta felis euismod semper.', 16.00, 4),
('Maseczka algowa do twarzy', 'Fusce dapibus, tellus ac cursus commodo.', 21.00, 4),
('Serum odmładzające z dziką różą', 'Donec ullamcorper nulla non metus auctor fringilla.', 28.90, 4),
('Szampon w kostce „Ziołowa Regeneracja”', 'Etiam porta sem malesuada magna mollis euismod.', 18.00, 4),
('Hydrolat różany', 'Duis mollis, est non commodo luctus.', 25.50, 4),
('Peeling cukrowy z kawą', 'Nullam id dolor id nibh ultricies vehicula ut id elit.', 19.00, 4),
('Olej z pestek malin', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 45.00, 5),
('Olej konopny tłoczony na zimno', 'Suspendisse potenti. Proin scelerisque, sapien sed scelerisque auctor, felis elit pharetra justo.', 38.20, 5),
('Macerat nagietkowy', 'Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.', 25.50, 5),
('Olej z wiesiołka', 'Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 30.00, 5),
('Olej lniany zimnotłoczony', 'Vestibulum id ligula porta felis euismod semper.', 23.80, 5),
('Macerat z żywokostu', 'Fusce dapibus, tellus ac cursus commodo.', 29.00, 5),
('Olej z pestek dyni', 'Donec ullamcorper nulla non metus auctor fringilla.', 35.00, 5),
('Olej z rokitnika', 'Etiam porta sem malesuada magna mollis euismod.', 42.50, 5),
('Olej z marchwi do opalania', 'Duis mollis, est non commodo luctus.', 26.40, 5),
('Olej arganowy', 'Nullam id dolor id nibh ultricies vehicula ut id elit.', 49.90, 5),
('Mielony imbir ekologiczny', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 7.99, 6),
('Liście laurowe suszone', 'Suspendisse potenti. Proin scelerisque, sapien sed scelerisque auctor, felis elit pharetra justo.', 3.99, 6),
('Cynamon cejloński', 'Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.', 9.50, 6),
('Kurkuma w proszku', 'Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 12.30, 6),
('Kardamon mielony', 'Vestibulum id ligula porta felis euismod semper.', 15.20, 6),
('Ziele angielskie całe', 'Fusce dapibus, tellus ac cursus commodo.', 5.80, 6),
('Oregano suszone', 'Donec ullamcorper nulla non metus auctor fringilla.', 8.00, 6),
('Kwiat muszkatołowy', 'Etiam porta sem malesuada magna mollis euismod.', 11.10, 6),
('Pieprz czarny ziarnisty', 'Duis mollis, est non commodo luctus.', 6.40, 6),
('Suszona bazylia', 'Nullam id dolor id nibh ultricies vehicula ut id elit.', 7.60, 6),
('Miód lipowy naturalny', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 28.99, 7),
('Miód spadziowy leśny', 'Suspendisse potenti. Proin scelerisque, sapien sed scelerisque auctor, felis elit pharetra justo.', 32.50, 7),
('Miód rzepakowy kremowany', 'Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.', 27.00, 7),
('Miód wrzosowy ekologiczny', 'Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 36.80, 7),
('Pyłek pszczeli', 'Vestibulum id ligula porta felis euismod semper.', 23.90, 7),
('Propolis w nalewce', 'Fusce dapibus, tellus ac cursus commodo.', 18.20, 7),
('Mleczko pszczele w kapsułkach', 'Donec ullamcorper nulla non metus auctor fringilla.', 34.60, 7),
('Miód z cynamonem', 'Etiam porta sem malesuada magna mollis euismod.', 29.80, 7),
('Miód z maliną', 'Duis mollis, est non commodo luctus.', 31.50, 7),
('Miód z propolisem', 'Nullam id dolor id nibh ultricies vehicula ut id elit.', 33.00, 7),
('Olejek lawendowy', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 24.50, 8),
('Olejek eukaliptusowy', 'Suspendisse potenti. Proin scelerisque, sapien sed scelerisque auctor, felis elit pharetra justo.', 21.80, 8),
('Olejek rozmarynowy', 'Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.', 26.00, 8),
('Olejek miętowy', 'Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 23.00, 8),
('Olejek cytrynowy', 'Vestibulum id ligula porta felis euismod semper.', 19.90, 8),
('Olejek sosnowy', 'Fusce dapibus, tellus ac cursus commodo.', 22.60, 8),
('Olejek bergamotowy', 'Donec ullamcorper nulla non metus auctor fringilla.', 27.40, 8),
('Olejek paczulowy', 'Etiam porta sem malesuada magna mollis euismod.', 29.50, 8),
('Olejek drzewa herbacianego', 'Duis mollis, est non commodo luctus.', 25.30, 8),
('Olejek kadzidłowy', 'Nullam id dolor id nibh ultricies vehicula ut id elit.', 35.00, 8),
('Kasza jaglana ekologiczna', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 15.00, 9),
('Orzechy nerkowca BIO', 'Suspendisse potenti. Proin scelerisque, sapien sed scelerisque auctor, felis elit pharetra justo.', 24.50, 9),
('Mąka kokosowa ekologiczna', 'Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.', 18.70, 9),
('Syrop klonowy BIO', 'Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 29.00, 9),
('Czekolada gorzka 85% BIO', 'Vestibulum id ligula porta felis euismod semper.', 9.90, 9),
('Makaron orkiszowy pełnoziarnisty', 'Fusce dapibus, tellus ac cursus commodo.', 11.50, 9),
('Ryż jaśminowy organiczny', 'Donec ullamcorper nulla non metus auctor fringilla.', 15.40, 9),
('Chleb na zakwasie z mąki żytniej', 'Etiam porta sem malesuada magna mollis euismod.', 13.00, 9),
('Suszone morele niesiarkowane', 'Duis mollis, est non commodo luctus.', 22.00, 9),
('Sok z granatu 100%', 'Nullam id dolor id nibh ultricies vehicula ut id elit.', 19.90, 9),
('Młynek do ziół ręczny', 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 18.99, 10),
('Zestaw do parzenia herbaty z sitkiem', 'Suspendisse potenti. Proin scelerisque, sapien sed scelerisque auctor, felis elit pharetra justo.', 29.50, 10),
('Drewniana łyżeczka do miodu', 'Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.', 8.40, 10),
('Moździerz kamienny do przypraw', 'Integer posuere erat a ante venenatis dapibus posuere velit aliquet.', 26.20, 10),
('Bawełniane torebki na zioła', 'Vestibulum id ligula porta felis euismod semper.', 9.80, 10),
('Butelka na maceraty szklana', 'Fusce dapibus, tellus ac cursus commodo.', 14.50, 10),
('Słoik z ciemnego szkła na oleje', 'Donec ullamcorper nulla non metus auctor fringilla.', 12.90, 10),
('Filtr do herbaty wielorazowy', 'Etiam porta sem malesuada magna mollis euismod.', 6.99, 10),
('Termometr do macerowania olejów', 'Duis mollis, est non commodo luctus.', 23.50, 10),
('Szczoteczka do masażu skóry', 'Nullam id dolor id nibh ultricies vehicula ut id elit.', 15.30, 10);

INSERT INTO typy_rachunkow(typ, opis) VALUES
('Sprzedaz', 'Sprzedaż produktów'),
('Zwrot', 'Zwrot produktów'),
('Koszty stałe', 'Koszty stałe firmy'),
('Inny', 'Inne operacje');

INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Jarocin', 'Słonecznikowa', '37');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Maciej', 'Borczyk', 'ymuzyk@example.com', '796 241 907', 1, 'P1000');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Kędzierzyn-Koźle', 'Złota', '72', '51');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Blanka', 'Tecław', 'kajetan52@example.net', '797 354 620', 2);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Przemyśl', 'Powstańców Śląskich', '54/91', '15');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Marianna', 'Fiedorczuk', 'piotrbabel@example.org', '661 401 994', 3, 'P7257');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Piaseczno', 'Traugutta', '56');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Justyna', 'Utrata', 'nataniel56@example.com', '736 047 461', 4);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Łódź', 'Spółdzielcza', '12/79');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Błażej', 'Orzeszek', 'grodekantoni@example.com', '22 206 16 69', 5, 'P0304');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Skawina', 'Cedrowa', '823', '56');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Jacek', 'Kunka', 'agorgol@example.com', '+48 726 156 879', 6);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Pszczyna', 'Chopina', '84/65');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Mieszko', 'Pierz', 'kjagielo@example.org', '+48 508 268 094', 7, 'P6229');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Tarnobrzeg', 'Krasickiego', '770');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Roksana', 'Wielgo', 'iwolatoszek@example.com', '+48 602 251 073', 8);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Ostrów Mazowiecka', 'Nowa', '14', '3');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Konrad', 'Łuszczak', 'daniellemanowicz@example.net', '536 913 100', 9, 'P6440');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Piła', 'Ceglana', '374');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Natan', 'Kurzac', 'sonia96@example.net', '+48 738 494 248', 10);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Bartoszyce', 'Turystyczna', '63');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Błażej', 'Kiełek', 'marcelinakolesnik@example.com', '+48 32 512 32 68', 11, 'P3100');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Bydgoszcz', 'Chopina', '13/45', '87');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Daniel', 'Wawrzonek', 'dgromala@example.net', '730 242 391', 12);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Mysłowice', 'Kaszubska', '25');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Janina', 'Swatek', 'adalemiesz@example.org', '+48 785 190 829', 13, 'P1538');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Sochaczew', 'Kwiatowa', '495');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Mateusz', 'Fierek', 'fjany@example.com', '666 280 783', 14);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Wałcz', 'Perłowa', '13');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Rozalia', 'Pasiut', 'trudzik@example.com', '+48 22 097 27 23', 15, 'P8793');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Oława', 'Klonowa', '46');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Krzysztof', 'Krüger', 'norbert94@example.net', '+48 888 816 048', 16);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Zielona Góra', 'Wieniawskiego', '36/90');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Adam', 'Józwik', 'zbojdo@example.com', '+48 739 996 032', 17, 'P8751');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Cieszyn', 'Jagodowa', '96/23', '36');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Radosław', 'Ostapczuk', 'ewa30@example.net', '725 949 003', 18);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Gorlice', 'Świerczewskiego', '235', '59');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Grzegorz', 'Tołoczko', 'ewa20@example.org', '+48 516 319 140', 19, 'P9378');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Łuków', 'Słowicza', '69', '91');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Stanisław', 'Narożny', 'anna-mariapikor@example.net', '504 113 717', 20);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Cieszyn', 'Boczna', '846');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Juliusz', 'Pal', 'dlugajczykolga@example.com', '+48 570 086 950', 21, 'P5348');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Chorzów', 'Długosza', '21');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Cezary', 'Kierepka', 'gwizdekantoni@example.com', '698 178 723', 22);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Biała Podlaska', 'Makowa', '54/52');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Mikołaj', 'Patyna', 'klujnikodem@example.com', '+48 600 459 818', 23, 'P7458');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Ostrów Wielkopolski', 'Kopernika', '35', '41');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Przemysław', 'Para', 'pawliczekaleksander@example.com', '795 975 395', 24);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Rumia', 'Rzemieślnicza', '56/12');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Sonia', 'Gorgol', 'apoloniaanton@example.com', '22 178 82 73', 25, 'P8739');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Giżycko', 'Rybacka', '967');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Alan', 'Jeske', 'michallula@example.com', '+48 538 111 517', 26);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Zielona Góra', 'Kościelna', '10/97');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Julita', 'Szymula', 'btulacz@example.com', '508 421 015', 27, 'P7241');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Starogard Gdański', 'Ludowa', '97', '21');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Adrian', 'Klucznik', 'jakubprejs@example.org', '798 936 396', 28);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Kielce', 'Szeroka', '68/68', '56');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Grzegorz', 'Nehring', 'roza63@example.org', '+48 570 342 746', 29, 'P6544');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Wejherowo', 'Daszyńskiego', '376');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Mateusz', 'Karpiel', 'jeremi09@example.com', '+48 32 549 25 74', 30);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Zgierz', 'Piwna', '583');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Ewelina', 'Stosik', 'anita28@example.net', '518 872 006', 31, 'P8375');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Szczytno', 'Osiedlowa', '58/61', '60');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Sara', 'Długozima', 'krystianwysota@example.com', '692 904 596', 32);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Kluczbork', 'Wczasowa', '499', '9');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Nikodem', 'Sromek', 'yzapior@example.com', '+48 32 684 05 85', 33, 'P6647');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Ostrów Wielkopolski', 'Floriana', '81/68');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Tymoteusz', 'Myk', 'elizapal@example.org', '887 776 040', 34);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Kłodzko', 'Borowa', '540');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Dariusz', 'Turlej', 'tutkajuliusz@example.org', '32 887 02 24', 35, 'P9204');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Tarnowskie Góry', 'Lipca', '95', '99');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Sylwia', 'Smektała', 'jozef64@example.org', '32 934 02 43', 36);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Wołomin', 'Sosnowa', '43/93');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Tomasz', 'Petrus', 'patryk33@example.net', '574 831 380', 37, 'P0737');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Piła', 'Kusocińskiego', '588');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Julita', 'Wita', 'xolchawa@example.com', '+48 797 145 392', 38);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Środa Wielkopolska', 'Pszenna', '56/34');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Eryk', 'Padło', 'angelikarygula@example.net', '+48 22 758 24 30', 39, 'P7909');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Jarosław', 'Chmielna', '510');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Albert', 'Cofała', 'xmaleszka@example.com', '+48 602 206 086', 40);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Żagań', 'Nowa', '49/48', '68');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Nela', 'Truchan', 'kowalukewelina@example.com', '+48 728 882 732', 41, 'P9044');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Nowa Sól', 'Okrężna', '75');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Lidia', 'Majos', 'oliwier34@example.net', '+48 579 055 162', 42);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Świętochłowice', 'Stroma', '52/39');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Andrzej', 'Pyś', 'ewa64@example.org', '+48 518 859 335', 43, 'P7622');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Grodzisk Mazowiecki', 'Okrężna', '34');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Franciszek', 'Hendzel', 'kuziakrystyna@example.org', '886 947 944', 44);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Czerwionka-Leszczyny', 'Agrestowa', '23');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Dorota', 'Skolik', 'dziwakkalina@example.org', '+48 739 257 696', 45, 'P7596');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Dębica', 'Rubinowa', '431');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Cezary', 'Sugier', 'monika66@example.net', '+48 570 688 821', 46);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Żory', 'Moniuszki', '882', '64');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Marek', 'Grabias', 'apolonia60@example.com', '531 359 410', 47, 'P0635');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Bielawa', 'Kaliska', '01/78');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Franciszek', 'Pyrkosz', 'emilmisiurek@example.org', '797 608 358', 48);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Augustów', 'Jana Sobieskiego', '18');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Maks', 'Leszcz', 'kamil45@example.com', '532 710 970', 49, 'P9033');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Zgierz', 'Chopina', '13');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Jakub', 'Wojtczuk', 'konrad79@example.com', '508 926 534', 50);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Kraśnik', 'Topolowa', '90', '10');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Aurelia', 'Karpiak', 'apoloniadyszkiewicz@example.org', '724 223 789', 51, 'P7689');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Puławy', 'Malinowa', '564', '14');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Rafał', 'Dub', 'prokopiakmaksymilian@example.com', '32 905 59 32', 52);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Opole', 'Makuszyńskiego', '69');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Patryk', 'Ziegler', 'gradzielstefan@example.com', '+48 32 440 59 83', 53, 'P5464');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Police', 'Lawendowa', '99/11', '88');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Kaja', 'Gągała', 'aleksander92@example.com', '786 935 113', 54);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Grudziądz', 'Moniuszki', '340');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Sylwia', 'Sarota', 'aureliapalus@example.com', '+48 508 810 730', 55, 'P3873');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Brzeg', 'Krakowska', '385');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Szymon', 'Szela', 'jeremikraszkiewicz@example.net', '22 439 95 79', 56);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Police', 'Kolejowa', '49');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Roksana', 'Mik', 'zaczykmateusz@example.com', '510 764 513', 57, 'P1414');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Bolesławiec', 'Fiołkowa', '758', '30');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Kazimierz', 'Łągiewka', 'marekgranat@example.org', '887 590 717', 58);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Bochnia', 'Perłowa', '514');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Radosław', 'Welc', 'ignacydurlik@example.net', '+48 889 584 931', 59, 'P1599');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Ciechanów', 'Kolejowa', '144');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Olgierd', 'Wojtyna', 'dariusz89@example.net', '+48 796 759 495', 60);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Kielce', 'Żeglarska', '064', '93');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Paweł', 'Łuksza', 'jwidlak@example.com', '+48 609 692 169', 61, 'P8738');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Cieszyn', 'Widokowa', '45/96');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Damian', 'Władyka', 'wiekieraantoni@example.com', '788 871 725', 62);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Gliwice', 'Pomorska', '19');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Elżbieta', 'Jarema', 'jan97@example.com', '+48 32 268 33 40', 63, 'P0313');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Gniezno', 'Skrajna', '131');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Lidia', 'Zwara', 'maksprazmo@example.net', '+48 505 826 275', 64);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Pabianice', 'Chabrowa', '366', '19');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Maks', 'Heller', 'malwinaborzych@example.org', '537 322 702', 65, 'P2077');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Gdańsk', 'Staszica', '31', '48');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Dariusz', 'Gierach', 'cyprian53@example.org', '+48 786 618 543', 66);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Kościerzyna', 'Rzeczna', '69', '45');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Aleks', 'Pyś', 'wiktor93@example.com', '+48 720 377 992', 67, 'P8320');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Racibórz', 'Jana Pawła II', '99/93');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Igor', 'Pitura', 'andrejczukeliza@example.org', '+48 22 054 73 26', 68);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Białogard', 'Azaliowa', '989');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Justyna', 'Wałęsa', 'monika29@example.net', '725 281 474', 69, 'P2196');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Ełk', 'Strzelecka', '171');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Adrian', 'Kowala', 'sylwialewicz@example.org', '+48 608 409 143', 70);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Czerwionka-Leszczyny', 'Skrajna', '30/24');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Bartek', 'Gosik', 'fryderyk39@example.com', '514 972 898', 71, 'P0727');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Mikołów', 'Wierzbowa', '745');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Urszula', 'Budzich', 'miszkurkaangelika@example.com', '792 971 044', 72);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Nowy Dwór Mazowiecki', 'Młynarska', '921');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Patryk', 'Kandziora', 'egolofit@example.org', '723 372 307', 73, 'P6299');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Siedlce', 'Pałacowa', '26');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Krystyna', 'Czak', 'antoni96@example.org', '+48 797 973 675', 74);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Płońsk', 'Korczaka', '890');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Norbert', 'Spadło', 'julitafrukacz@example.net', '+48 22 722 24 82', 75, 'P6007');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Chrzanów', 'Moniuszki', '99/48');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Igor', 'Brząkała', 'qsarniak@example.org', '+48 887 454 359', 76);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Włocławek', 'Zielona', '34/90', '83');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Jeremi', 'Armata', 'marceltoborek@example.net', '+48 579 806 448', 77, 'P8315');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Oleśnica', 'Zielona', '23', '36');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Antoni', 'Laszuk', 'jacek19@example.org', '534 003 095', 78);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Sanok', 'Kolejowa', '79');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Natasza', 'Śnieg', 'ryszard47@example.com', '881 868 150', 79, 'P5273');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Nysa', 'Księżycowa', '37');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Sonia', 'Wijata', 'konrad77@example.org', '573 649 270', 80);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Głogów', 'Borowa', '58', '6');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Blanka', 'Franik', 'jan64@example.net', '797 390 398', 81, 'P4328');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Katowice', 'Mała', '430');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Rozalia', 'Cierpka', 'olaf68@example.com', '+48 609 599 292', 82);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Legnica', 'Zamkowa', '76/03');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Stefan', 'Herbut', 'sakwamarcelina@example.com', '+48 22 108 00 33', 83, 'P7483');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Olkusz', 'Rajska', '25');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Julianna', 'Orawiec', 'tymoteuszgortat@example.net', '22 138 99 19', 84);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Gdańsk', 'Kusocińskiego', '816');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Szymon', 'Płocharczyk', 'bjezior@example.net', '883 362 026', 85, 'P4245');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Wałbrzych', 'Szeroka', '971');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Błażej', 'Nojman', 'liwia23@example.net', '+48 506 405 134', 86);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Sochaczew', 'Wyszyńskiego', '01/30');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Ewa', 'Kościelak', 'hpawelkiewicz@example.org', '+48 664 104 374', 87, 'P5977');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Biłgoraj', 'Makuszyńskiego', '412');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Marika', 'Stroka', 'krystianwnuczek@example.org', '+48 22 552 61 05', 88);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Jarocin', 'Kilińskiego', '16');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Olaf', 'Więckiewicz', 'vzagozdzon@example.com', '32 389 78 23', 89, 'P9453');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Gdańsk', 'Miarki', '79');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Mariusz', 'Kopała', 'milosz06@example.org', '+48 799 955 953', 90);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Police', 'Dojazdowa', '94');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Leon', 'Gęsiarz', 'alanpitura@example.org', '+48 504 019 032', 91, 'P3744');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Nowy Dwór Mazowiecki', 'Miarki', '67', '1');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Jakub', 'Pałgan', 'hosipowicz@example.net', '+48 662 814 349', 92);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Chojnice', 'Konwaliowa', '12');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Adam', 'Fraszczyk', 'miaskomalwina@example.net', '+48 32 888 02 74', 93, 'P8867');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Czerwionka-Leszczyny', 'Kaszubska', '91', '14');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Karol', 'Gapys', 'adrianna52@example.com', '+48 538 985 310', 94);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Radom', 'Kowalska', '165');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Mateusz', 'Świerszcz', 'agrabczak@example.com', '601 879 289', 95, 'P5458');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku, numer_mieszkania) VALUES ('Poland', 'Grudziądz', 'Grottgera', '40/01', '59');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Julita', 'Kaluga', 'gustaw80@example.net', '794 515 261', 96);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Turek', 'Pszenna', '89/73');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Julianna', 'Prochownik', 'adamowmarcin@example.com', '+48 691 276 902', 97, 'P3736');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Września', 'Krucza', '40');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Leonard', 'Mandrysz', 'olgierdbalas@example.org', '797 349 031', 98);
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Piastów', 'Parkowa', '812');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id, id_paczkomatu) VALUES ('Wiktor', 'Parada', 'saraambrozewicz@example.net', '+48 790 466 942', 99, 'P1225');
INSERT INTO adresy (kraj, miasto, ulica, numer_budynku) VALUES ('Poland', 'Goleniów', 'Morska', '83/95');
INSERT INTO Klienci (imie, nazwisko, email, telefon, adres_id) VALUES ('Karol', 'Twardosz', 'anna-marialewicz@example.net', '725 732 730', 100);

-- INSERT-y do tabeli Zamowienia (100 rekordów)
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (23, '2025-01-03 23:24:54', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (88, '2025-03-01 08:15:57', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (4, '2025-03-09 14:20:08', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (51, '2025-03-04 12:21:29', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (12, '2025-01-09 08:31:24', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (78, '2025-03-10 18:17:59', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (39, '2025-03-06 21:16:11', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (65, '2025-02-15 16:55:53', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (1, '2025-03-20 09:08:18', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (80, '2025-02-08 08:20:30', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (45, '2025-02-15 08:26:07', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (33, '2025-01-10 06:47:58', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (7, '2025-01-27 03:05:46', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (58, '2025-01-04 01:29:59', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (29, '2025-02-20 00:17:03', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (72, '2025-03-03 06:29:38', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (19, '2025-03-17 00:23:24', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (86, '2025-02-27 12:35:34', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (5, '2025-02-04 07:47:48', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (61, '2025-03-04 09:07:18', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (31, '2025-01-08 19:50:40', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (90, '2025-01-14 09:08:25', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (25, '2025-02-22 00:37:22', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (55, '2025-01-15 12:46:29', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (14, '2025-02-11 16:19:08', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (74, '2025-02-23 17:47:05', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (36, '2025-01-10 23:07:45', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (68, '2025-02-18 00:25:02', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (8, '2025-01-14 09:19:13', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (48, '2025-01-22 06:58:54', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (82, '2025-01-24 02:34:19', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (21, '2025-02-19 18:33:48', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (53, '2025-01-21 22:49:29', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (16, '2025-02-28 11:41:41', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (76, '2025-02-07 00:16:37', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (3, '2025-02-03 20:57:43', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (41, '2025-03-05 12:56:13', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (63, '2025-01-27 17:11:19', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (27, '2025-02-08 16:39:26', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (85, '2025-02-03 14:39:16', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (10, '2025-03-07 01:20:56', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (70, '2025-02-20 09:56:32', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (37, '2025-03-01 06:05:01', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (59, '2025-02-05 04:04:19', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (22, '2025-03-18 22:27:35', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (83, '2025-02-26 16:53:44', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (47, '2025-03-20 07:38:20', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (67, '2025-01-11 08:37:13', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (11, '2025-03-19 04:42:09', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (73, '2025-03-04 18:47:43', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (34, '2025-01-13 15:00:46', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (60, '2025-02-15 02:43:14', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (26, '2025-02-12 01:38:05', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (56, '2025-02-08 03:28:42', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (17, '2025-01-14 14:34:51', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (79, '2025-01-31 21:25:24', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (42, '2025-02-15 10:46:32', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (64, '2025-01-29 08:13:05', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (28, '2025-01-30 23:02:52', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (87, '2025-03-16 15:50:56', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (6, '2025-01-11 17:58:43', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (50, '2025-03-18 14:53:54', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (30, '2025-01-10 21:26:33', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (71, '2025-02-14 11:17:04', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (38, '2025-02-08 01:59:45', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (62, '2025-02-04 22:26:45', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (20, '2025-03-01 09:23:30', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (84, '2025-01-27 01:31:46', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (46, '2025-03-16 07:09:54', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (69, '2025-02-03 13:37:35', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (13, '2025-03-17 23:18:21', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (75, '2025-03-02 21:21:36', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (35, '2025-03-09 14:47:07', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (57, '2025-02-12 21:35:52', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (18, '2025-03-12 10:36:36', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (81, '2025-02-06 05:55:13', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (43, '2025-01-29 23:24:59', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (66, '2025-03-02 04:01:46', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (24, '2025-01-16 18:25:26', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (49, '2025-02-06 12:33:50', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (89, '2025-02-01 18:13:57', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (9, '2025-03-02 07:18:22', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (52, '2025-03-20 19:58:46', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (15, '2025-03-05 14:58:56', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (77, '2025-01-21 02:12:53', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (40, '2025-01-12 05:20:49', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (54, '2025-02-25 09:54:33', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (2, '2025-03-12 00:38:10', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (44, '2025-02-15 19:56:18', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (32, '2025-01-04 20:38:16', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (93, '2025-02-26 12:10:31', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (91, '2025-03-01 18:59:48', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (92, '2025-02-12 04:46:39', 'W trakcie');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (94, '2025-01-02 16:33:06', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (95, '2025-03-17 18:36:14', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (96, '2025-01-30 23:34:28', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (97, '2025-02-07 01:38:48', 'Nowe');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (98, '2025-02-13 11:19:02', 'Zrealizowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (99, '2025-01-02 07:58:41', 'Anulowane');
INSERT INTO Zamowienia (klient_id, data_zamowienia, status) VALUES (100, '2025-01-07 21:08:23', 'Zrealizowane');

-- INSERT-y do tabeli SzczegolyZamowienia (rekordy dla zamówień)
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (1, 26, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (1, 87, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (1, 31, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (1, 52, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (2, 63, 3);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (2, 19, 5);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (2, 28, 1);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (2, 43, 6);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (2, 13, 6);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (3, 44, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (3, 88, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (3, 70, 7);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (3, 55, 5);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (4, 97, 10);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (4, 51, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (4, 3, 1);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (4, 86, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (4, 78, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (5, 24, 6);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (5, 13, 10);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (5, 79, 7);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (5, 38, 3);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (5, 100, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (6, 78, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (6, 80, 5);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (6, 27, 1);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (6, 13, 3);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (6, 64, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (7, 51, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (7, 53, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (7, 46, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (7, 33, 7);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (7, 94, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (8, 5, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (8, 89, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (8, 56, 5);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (9, 5, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (9, 11, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (9, 98, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (10, 7, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (10, 31, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (10, 43, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (10, 38, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (10, 29, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (11, 35, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (11, 66, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (11, 52, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (11, 59, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (11, 10, 3);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (12, 36, 10);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (13, 87, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (13, 91, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (13, 12, 10);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (13, 88, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (14, 33, 1);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (15, 39, 1);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (15, 45, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (15, 15, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (15, 77, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (16, 43, 5);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (17, 32, 6);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (17, 42, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (17, 99, 1);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (17, 17, 3);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (17, 75, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (18, 49, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (19, 67, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (19, 35, 6);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (19, 10, 3);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (19, 2, 10);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (20, 21, 10);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (20, 97, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (21, 12, 7);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (21, 2, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (22, 76, 3);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (22, 40, 6);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (22, 37, 6);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (22, 88, 6);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (23, 30, 1);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (24, 24, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (24, 2, 7);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (24, 68, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (24, 44, 6);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (25, 51, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (25, 91, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (25, 64, 3);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (25, 17, 4);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (25, 13, 7);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (26, 43, 2);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (26, 1, 10);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (26, 30, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (26, 5, 7);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (27, 1, 3);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (27, 44, 10);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (28, 15, 10);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (28, 79, 5);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (28, 66, 9);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (28, 55, 7);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (28, 20, 8);
INSERT INTO SzczegolyZamowienia (zamowienie_id, produkt_id, ilosc) VALUES (29, 40, 3);

INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Wyroba-Zagozda s.c.', '+48 660 725 014', 1);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('PPUH Frącz-Kus Sp.k.', '+48 720 672 798', 2);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Waleczek-Berek Sp.k.', '+48 786 312 800', 3);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Rabiej i syn s.c.', '505 618 781', 4);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Stowarzyszenie Chlebek-Godzik Sp. z o.o.', '502 064 887', 5);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Domino i syn s.c.', '+48 693 293 470', 6);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Policht', '+48 727 763 174', 7);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Nasiadka i syn s.c.', '697 820 231', 8);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Górkiewicz', '695 095 062', 9);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Dycha', '666 048 302', 10);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Bąbel Sp.j.', '570 078 745', 11);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Fundacja Ćwierz Sp.k.', '+48 607 421 574', 12);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Gąszczak Sp.j.', '504 195 375', 13);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Fedorczyk i syn s.c.', '721 110 331', 14);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Pych', '+48 726 411 789', 15);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Fundacja Jany Sp. z o.o. Sp.k.', '605 311 952', 16);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Stowarzyszenie Zubel-Nikel i syn s.c.', '+48 32 279 11 43', 17);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Tabak', '796 845 955', 18);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gasik-Merda Sp. z o.o.', '+48 795 419 100', 19);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Stowarzyszenie Owsianka-Sulich s.c.', '883 424 381', 20);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gołota Sp.j.', '+48 720 300 652', 21);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Hojda-Rutowicz s.c.', '+48 22 027 64 69', 22);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Nasiadka-Klinger Sp.j.', '+48 790 601 473', 23);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Gosz-Petrus s.c.', '+48 608 636 481', 24);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Różak', '+48 502 945 567', 25);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('FPUH Grochowina-Tylus Sp.k.', '575 554 432', 26);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('PPUH Kut-Michnowicz Sp. z o.o. Sp.k.', '22 968 36 73', 27);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Wojcik Sp. z o.o.', '+48 32 532 10 48', 28);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Kumorek Sp.j.', '+48 693 040 186', 29);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Mazan Sp.k.', '720 536 739', 30);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Franiak-Komander Sp.k.', '+48 880 815 206', 31);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Stachelek-Słonka S.A.', '+48 880 156 354', 32);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Krakowczyk-Borejko Sp.j.', '+48 799 608 724', 33);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Świgoń Sp.k.', '579 885 924', 34);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Glenc S.A.', '514 764 479', 35);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('FPUH Jamroz-Gosik Sp. z o.o.', '797 797 925', 36);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Wincenciak-Machniak s.c.', '+48 739 422 033', 37);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Fundacja Srokosz', '797 342 823', 38);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('FPUH Mizgała Sp.k.', '+48 519 992 449', 39);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Makosz Sp.j.', '+48 570 118 959', 40);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('PPUH Drygas i syn s.c.', '+48 608 326 886', 41);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Stowarzyszenie Narożny', '+48 577 096 098', 42);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Starzak Sp.j.', '+48 578 074 351', 43);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grund-Szaj Sp.j.', '+48 729 242 545', 44);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Stawowczyk-Łuczkiewicz Sp. z o.o.', '880 820 661', 45);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Burchardt-Budniak i syn s.c.', '660 420 978', 46);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gral-Tekieli Sp. z o.o.', '574 307 559', 47);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Głowiak S.A.', '503 185 624', 48);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Glazik Sp. z o.o. Sp.k.', '+48 577 191 462', 49);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Wasil-Waloszek Sp.j.', '+48 887 423 047', 50);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Kuświk Sp. z o.o.', '738 685 104', 51);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Kuk i syn s.c.', '665 409 734', 52);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Maćków i syn s.c.', '603 733 879', 53);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Tołoczko', '+48 515 216 975', 54);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Fundacja Siedlik', '691 025 422', 55);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Żelazo s.c.', '532 883 288', 56);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Buszta', '+48 32 670 99 91', 57);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Fundacja Ważny S.A.', '788 609 947', 58);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Januchta', '726 070 253', 59);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Kuza-Ośka Sp. z o.o.', '600 765 039', 60);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gajownik s.c.', '+48 694 251 370', 61);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('FPUH Politowicz-Bazydło s.c.', '+48 22 106 17 41', 62);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Szczerek-Dziarmaga Sp.k.', '+48 22 459 64 45', 63);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Ubysz-Rembacz s.c.', '+48 606 601 887', 64);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Front', '+48 720 800 096', 65);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Leonowicz-Pastuszko s.c.', '+48 791 915 584', 66);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Stowarzyszenie Kurz-Jakimiak Sp.k.', '+48 32 588 36 61', 67);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('FPUH Kłosiewicz-Kapinos Sp.k.', '+48 575 018 590', 68);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Kurzac', '+48 533 230 073', 69);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Fundacja Bejm-Piszcz Sp.j.', '+48 502 446 919', 70);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('PPUH Peszko Sp. z o.o. Sp.k.', '22 289 83 89', 71);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('FPUH Martynowicz-Pawela S.A.', '668 733 264', 72);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Suchta Sp. z o.o. Sp.k.', '+48 723 579 736', 73);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('PPUH Łosiewicz', '732 387 690', 74);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Wośko Sp.k.', '666 358 366', 75);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Prokopczyk-Idec s.c.', '32 208 17 23', 76);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Ograbek Sp. z o.o. Sp.k.', '+48 515 631 169', 77);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Czajor Sp.k.', '+48 516 293 224', 78);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Gabryszak s.c.', '+48 32 855 48 35', 79);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('PPUH Żabierek', '+48 722 043 707', 80);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Frankowicz i syn s.c.', '514 190 362', 81);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Stowarzyszenie Organiściak Sp.j.', '+48 22 994 83 77', 82);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Fundacja Dziuda', '794 529 034', 83);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Łyś-Ciastek Sp. z o.o.', '+48 728 533 800', 84);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Zieja', '+48 22 303 13 05', 85);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Kamyk S.A.', '+48 505 118 633', 86);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Roszkiewicz-Molga Sp.j.', '579 629 474', 87);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Gabinety Ledzion', '+48 32 405 30 41', 88);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('FPUH Turczyk', '512 126 878', 89);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('FPUH Oleksa-Durma Sp. z o.o. Sp.k.', '728 701 166', 90);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grzeszkowiak-Pachowicz s.c.', '660 414 820', 91);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Swaczyna i syn s.c.', '+48 22 796 74 31', 92);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Pitura-Litwińczuk Sp.j.', '880 681 551', 93);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Spółdzielnia Pinda-Wysota Sp.k.', '+48 22 879 36 90', 94);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Forma Sp. z o.o.', '507 938 529', 95);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Salomon Sp. z o.o.', '799 501 978', 96);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('FPUH Walusiak-Siembab s.c.', '733 961 478', 97);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Klyta-Potok S.A.', '+48 22 752 84 19', 98);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Morka-Danel s.c.', '+48 519 830 587', 99);
INSERT INTO Dostawcy (nazwa, kontakt, adres_id) VALUES ('Grupa Posłuszna i syn s.c.', '+48 507 403 782', 100);


-- INSERT-y do tabeli Dostawy
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (55, 1);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (3, 1);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (40, 1);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (43, 2);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (53, 3);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (54, 3);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (58, 3);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (65, 4);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (81, 4);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (85, 5);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (60, 5);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (6, 5);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (71, 6);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (88, 6);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (24, 7);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (19, 8);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (40, 8);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (31, 8);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (89, 9);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (47, 9);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (31, 10);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (46, 10);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (92, 11);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (67, 11);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (9, 11);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (11, 12);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (38, 12);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (35, 12);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (68, 13);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (18, 13);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (2, 13);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (5, 14);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (19, 14);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (96, 15);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (92, 15);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (19, 16);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (53, 16);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (65, 17);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (93, 18);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (75, 19);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (38, 19);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (96, 19);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (91, 20);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (52, 20);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (47, 20);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (100, 21);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (70, 21);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (13, 21);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (47, 22);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (81, 22);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (73, 22);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (6, 23);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (66, 24);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (30, 24);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (52, 24);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (89, 25);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (46, 26);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (52, 26);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (13, 26);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (74, 27);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (28, 28);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (12, 28);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (43, 29);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (50, 29);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (32, 29);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (67, 30);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (55, 31);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (52, 31);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (51, 31);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (90, 32);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (49, 33);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (60, 33);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (62, 33);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (91, 34);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (33, 34);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (19, 34);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (9, 35);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (77, 36);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (2, 36);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (59, 37);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (85, 38);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (10, 38);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (1, 39);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (29, 40);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (47, 41);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (49, 41);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (99, 42);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (66, 42);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (24, 42);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (22, 43);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (82, 44);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (31, 44);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (30, 45);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (50, 45);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (51, 46);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (71, 47);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (18, 48);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (84, 49);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (70, 49);
INSERT INTO Dostawy (dostawca_id, produkt_id) VALUES (21, 49);

-- INSERT-y do tabeli rachunki
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-20', 3, 225.71);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-18', 2, 257.58);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-02', 1, 260.72);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-17', 2, 116.61);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-03', 4, 67.66);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-19', 4, 475.57);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-03', 4, 15.17);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-23', 3, 217.87);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-05', 2, 153.55);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-01', 3, 354.44);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-14', 1, 89.72);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-05', 1, 360.42);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-22', 3, 466.75);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-26', 2, 460.38);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-01', 3, 75.07);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-05', 3, 259.26);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-09', 1, 215.25);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-29', 2, 258.14);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-10', 2, 35.72);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-18', 4, 275.03);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-11', 2, 206.11);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-25', 2, 192.6);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-15', 1, 132.07);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-23', 2, 158.57);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-02', 3, 188.14);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-12', 2, 360.09);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-21', 1, 59.54);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-05', 1, 13.38);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-15', 4, 361.2);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-01', 3, 224.12);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-03', 4, 51.79);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-13', 4, 474.11);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-09', 4, 95.39);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-12', 2, 350.96);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-21', 2, 296.7);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-08', 3, 491.64);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-02', 2, 36.92);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-11', 2, 468.85);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-15', 4, 417.69);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-11', 4, 342.43);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-17', 2, 251.77);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-07', 2, 265.34);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-28', 1, 86.13);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-21', 2, 348.31);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-18', 1, 309.24);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-20', 2, 169.44);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-10', 2, 239.58);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-10', 1, 205.14);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-10', 3, 318.74);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-25', 2, 204.2);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-10', 1, 217.38);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-13', 3, 470.65);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-19', 1, 79.57);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-06', 1, 312.98);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-26', 4, 483.62);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-10', 4, 434.53);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-07', 4, 420.84);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-25', 4, 103.7);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-12', 1, 462.17);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-01', 1, 131.12);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-31', 4, 496.25);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-08', 4, 100.44);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-02', 4, 475.6);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-07', 4, 368.57);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-20', 3, 225.85);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-05', 2, 230.38);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-17', 3, 252.29);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-23', 1, 229.14);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-20', 3, 249.21);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-13', 4, 371.85);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-19', 3, 428.51);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-10', 4, 298.15);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-09', 1, 222.88);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-14', 1, 352.61);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-03', 2, 470.9);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-11', 2, 418.65);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-16', 1, 167.9);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-25', 4, 325.95);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-04', 2, 356.11);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-07', 2, 420.71);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-09', 1, 431.17);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-21', 3, 210.68);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-01', 4, 377.76);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-14', 4, 487.36);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-02', 4, 197.16);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-02', 3, 273.83);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-13', 1, 111.64);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-13', 1, 393.35);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-20', 2, 46.71);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-31', 4, 334.73);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-12', 4, 486.21);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-19', 4, 44.06);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-11', 2, 321.03);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-16', 1, 313.49);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-02-27', 1, 277.32);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-18', 2, 431.76);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-06', 4, 439.47);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-14', 4, 97.76);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-01-10', 2, 223.19);
INSERT INTO rachunki (data, typ, kwota) VALUES ('2025-03-11', 3, 451.44);


-- Włączanie sprawdzania kluczy obcych
SET FOREIGN_KEY_CHECKS = 1;