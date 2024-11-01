# Funkční specifikace pro CLI hru Wordle

## Verze dokumentu
- **Autor:** Ondřej Hummel
- **Verze:** 1.0
- **Datum:** 14.10.2024

### Historie verzí

| Verze | Autor | Datum | Popis změn |
| --- | --- | --- | --- |
| 1.0 | Ondřej Hummel | 14.10.2024 | První verze funkční specifikace |

## Obsah
1. Přehled
2. Uživatelské rozhraní
3. Hlavní funkce
4. Zpracování chybových stavů
5. Nefunkční požadavky

---

## 1. Přehled
Tento dokument definuje funkční specifikace pro CLI verzi hry Wordle. Hra je určena k tomu, aby byla hratelná jak v online, tak v offline módu. Pokud je uživatel offline, hra používá lokální slovník a ukládá výsledky lokálně, dokud nedojde k synchronizaci se serverem. V online módu je slovo získáno ze serveru a výsledky jsou synchronizovány okamžitě.

## 2. Uživatelské rozhraní

### 2.1 Terminálové rozhraní
- **Přihlašovací/Registrační obrazovka**: Uživatel bude vyzván k přihlášení, pokud je online. Příkazová řádka vyzve k zadání uživatelského jména a hesla. Pokud není připojen k internetu nebo jsou jiné problémy s připojením, zobrazí se zpráva a hra se spustí v offline módu.
  
- **Herní obrazovka**: Po úspěšném přihlášení se hráči zobrazí instrukce a výzva k zadání pětipísmenného slova. Po každém zadání bude obrazovka aktualizována barevnou zpětnou vazbou:
  - **Zelená**: Písmeno je správné a na správné pozici.
  - **Žlutá**: Písmeno je ve slově, ale na nesprávné pozici.
  - **Šedá/Nebarevné**: Písmeno není ve slově vůbec.
- V případě barvoslepeckého módu může být zpětná vazba doplněna jinými vizuálními reprezentacemi/barvami.
  
- **Konec hry**: Po uhodnutí slova nebo vyčerpání pokusů je hráči nabídnuta možnost hru ukončit. Pokud je offline, výsledky se uloží lokálně.


## 3. Hlavní funkce

### 3.1 Online/Offline režim
- **Online mód**: Hráč se přihlásí nebo registruje. Slovo je získáno ze serveru a výsledky jsou synchronizovány s databází.
- **Offline mód**: Hráč může hrát i bez připojení k internetu. Slovo je vybráno z lokálního slovníku a výsledky jsou ukládány lokálně. Jakmile se hráč znovu připojí, výsledky se synchronizují.

### 3.2 Herní logika
- **Game loop**: Hráč zadá slovo, hra ho porovná se slovem dne a poskytne zpětnou vazbu. Hra pokračuje, dokud hráč buď neuhodne slovo, nebo nevyčerpá všechny pokusy.
- **Počet pokusů**: Hráč má šest pokusů na uhodnutí správného slova.

### 3.3 Výběr slova
- **Online**: Slovo dne je vybráno serverem.
- **Offline**: Lokální slovník poskytuje slovo pro hru.

### 3.4 Ukládání výsledků
- **Online**: Výsledky se okamžitě synchronizují se serverem.
- **Offline**: Výsledky se ukládají lokálně a synchronizují se, jakmile bude připojení k internetu.

## 4. Zpracování chybových stavů

### 4.1 Chyby přihlášení
- Pokud se uživatel pokusí přihlásit bez připojení k internetu, hra se spustí v offline módu. V případě neúspěšného přihlášení z jiného důvodu se uživateli zobrazí chybová zpráva s pokynem k dalším krokům (např. kontrola přihlašovacích údajů).

### 4.2 Chybně zadané slovo
- Pokud hráč zadá slovo kratší nebo delší než pět písmen, zobrazí se chybová zpráva a hráč bude vyzván k opětovnému zadání slova.

### 4.3 Problémy se synchronizací
- Pokud se výsledky nepodaří synchronizovat se serverem, hra uloží výsledky lokálně a pokusí se je odeslat při další dostupné příležitosti.

## 5. Nefunkční požadavky

### 5.1 Bezpečnost
- **Přihlašovací údaje**: Uživatelská jména a hesla jsou bezpečně hashována a ukládána na serveru.
- **Tokenizace**: Po úspěšném přihlášení je uživateli přidělen bezpečnostní token pro ověřování.

### 5.2 Výkon
- Hra musí fungovat na minimální hardwarové konfiguraci nejlépe s nízkými nároky na výkon.