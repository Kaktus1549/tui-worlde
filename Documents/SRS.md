# Videohra v CLI: Wordle – Technické požadavky
* **SSPŠ**
* **Ondřej Hummel**

## Verze dokumentu
- **Autor:** Ondřej Hummel
- **Verze:** 1.1
- **Datum:** 6.10.2024

### Historie verzí

| Verze | Autor | Datum | Popis změn |
| --- | --- | --- | --- |
| 1.0 | Ondřej Hummel | 30.9.2024 | První verze dokumentu. |
| 1.1 | Ondřej Hummel | 6.10.2024 | Oprava chyb a doplnění informací. |
| 1.2 | Ondřej Hummel | 13.10.2024 | Přemístění Verze a Historie verzí |

## Obsah
1. Úvod
2. Hrubý náčrt hry
3. Hlavní funkce aplikace
    3.1 Client side <br>
    3.2 Server side <br>
    3.3 Offline mode <br>
4. Cíl programu

---

## 1. Úvod
- **Účel dokumentu** - Cílem tohoto dokumentu je popsat všechny funkce hry Wordle a uvést nefunkční požadavky.
- **Kontaktní údaje:** hummel.on.2022@skola.ssps.cz

---

## 2. Hrubý náčrt hry
- **Jazykové verze:** Aplikace bude dostupná v angličtině a češtině, s možností přidat další jazyky pomocí "modů".
- **Spuštění:** Při spuštění aplikace je hráč vyzván k přihlášení nebo registraci.
- **Po přihlášení:** Po úspěšném přihlášení je hráč přesměrován přímo do hry.

---

## 3. Hlavní funkce aplikace

### 3.1. Client side (Priorita: vysoká)

#### 3.1.1 Game loop
1. **Přihlášení/Registrace:** Hráč se přihlásí nebo zaregistruje na základě toho, zda již má účet. (Priorita: vysoká)
2. **Zadání slova:** Hráč zadá slovo o pěti písmenech, které je porovnáno se slovem dne. (Priorita: vysoká)
3. **Vyhodnocení slova:** Hra porovná zadané slovo se skrytým slovem a poskytne zpětnou vazbu prostřednictvím barev. (Priorita: vysoká)
4. **Konec hry:** Hra končí buď po správném uhodnutí slova, nebo po vyčerpání všech pokusů, kdy se zobrazí výsledek a hráči je nabídnuta možnost hru ukončit. (Priorita: vysoká)

##### 3.1.2 Barevná zpětná vazba
- **Zelená:** Písmeno se nachází na správné pozici. (Priorita: vysoká)
- **Žlutá:** Písmeno je obsaženo ve slově, ale na nesprávném místě. (Priorita: vysoká)
- **Šedá/Nebarevné:** Písmeno není součástí slova. (Priorita: vysoká)
- **Zpětná vazba pro osoby s poruchami vnímání barev** - Plánuje se implementace varianty pro barevné poruchy. (Priorita: nízká)

##### 3.1.3 Výběr slova
- **Serverem poskytované slovo** - Slovo je vybíráno z oficiálního slovníku Wordle na serveru, který jej poskytne klientovi. (Priorita: vysoká)

##### 3.1.4 Ukončení hry
- Hra končí buď po správném uhodnutí slova, nebo po vyčerpání všech pokusů. (Priorita: vysoká)

---

### 3.2. Server side (Priorita: střední)

#### 3.2.1 Přihlášení
- **Přihlášení:** Hráč se přihlásí pomocí uživatelského jména a hesla, která jsou ověřena proti databázi (hashovaná hesla se solí). (Priorita: vysoká)
- **Token:** Po úspěšném přihlášení je hráči přidělen autentizační token (JWT) platný jeden den. (Priorita: střední)

#### 3.2.2 Registrace
- **Registrace:** Hráč se registruje pomocí uživatelského jména a hesla, která jsou bezpečně uložena v databázi (hashovaná se solí). (Priorita: střední)

#### 3.2.3 Ukládání výsledků
- **Ukládání výsledků:** Výsledky každé hry jsou ukládány do databáze, kde jsou propojeny s hráčským účtem. (Priorita: nízká)

---

### 3.3. Offline mode (Priorita: vysoká)

#### 3.3.1 Ukládání výsledků
- **Lokální ukládání výsledků:** Výsledky jsou ukládány lokálně a synchronizovány, jakmile se hráč připojí k internetu. (Priorita: vysoká)

#### 3.3.2 Výběr slova
- **Neoficiální slovník:** Slovo je vybíráno z lokálního neoficiálního slovníku. (Priorita: vysoká) 

---

## 4. Cíl programu

### 4.1 Trénink logiky a slovní zásoby
Hra je navržena jako zábavný nástroj pro zlepšování logického myšlení a rozšíření slovní zásoby hráčů.

### 4.2 Zlepšení strategie
Díky barevné zpětné vazbě hráči postupně zlepšují své strategie a logicky odvozují správné slovo.