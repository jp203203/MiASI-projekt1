# Gra programistyczna z użyciem ANTLR w silniku Godot

Projekt gry logicznej stworzonej w silniku Godot z wykorzystaniem języka C# oraz parsera ANTLR.

## Opis projektu

Gracz programuje ruch postaci przy pomocy prostego języka skryptowego przypominającego pseudokod. Po uruchomieniu programu postać wykonuje kolejkę poleceń w świecie gry.

Projekt zawiera:

- system wykonywania komend,
- interpreter poleceń oparty o ANTLR,
- sterowanie postacią po siatce,
- warunki i pętle,
- procedury,
- interaktywne elementy świata.

---

# Wymagania

## Oprogramowanie

Do uruchomienia projektu wymagane są:

- Godot Engine 4.6 z obsługą `.NET / C#`
- .NET SDK (zalecana wersja zgodna z Godot 4)

## Pobranie Godot

Oficjalna strona:

https://godotengine.org/download

Należy pobrać wersję:

- **Godot Engine .NET**

---

# Jak uruchomić projekt

## 1. Sklonowanie repozytorium

```bash
git clone https://github.com/jp203203/MiASI-projekt1.git
```

lub pobranie archiwum ZIP i rozpakowanie projektu.

---

## 2. Otworzenie projektu w Godot

1. Uruchom Godot Engine
2. Kliknij `Import`
3. Wskaż plik:

```text
project.godot
```

4. Zaimportuj projekt
5. Otwórz projekt

---

## 3. Uruchomienie gry

Po otwarciu projektu:

- naciśnij przycisk `Play`
- lub użyj skrótu:

```text
F5
```

Godot może chwilę kompilować skrypty C# przy pierwszym uruchomieniu.

---

# Struktura projektu

## Najważniejsze katalogi

```text
assets/                 -> grafiki i tilesety
scenes/                 -> sceny Godota
scripts/                -> logika gry
scripts/grammar/        -> interpreter i parser poleceń
grammar/                -> pliki ANTLR
data/                   -> dane poziomów
```

---

# Jak grać

## Cel gry

Celem gry jest doprowadzenie postaci do pola końcowego przy pomocy własnoręcznie napisanego kodu.

Gracz wpisuje instrukcje w edytorze kodu znajdującym się po prawej stronie ekranu.

Po kliknięciu przycisku `RUN` postać wykonuje przygotowany program.

---

# Dostępne komendy

## Ruch

### MOVE

Porusza postacią o określoną liczbę pól w kierunku w którym zwrócona jest postać.

Przykład:

```text
MOVE 3
```

Porusza postacią o 3 pola.

---

## Obrót

### ROTATE

Zmienia kierunek postaci, obracając ją o 90° względem aktualnego kierunku.

Dostępne kierunki:

- LEFT
- RIGHT

Przykład:

```text
ROTATE RIGHT
MOVE 2
```

Obraca postacią w prawo (o 90° zgodnie z ruchem wskazówek zegara) oraz porusza o 2 pola.

---

## Interakcje

### TAKE

Podnosi przedmiot jeżeli postać znajduje się przed nim. Postać może w danej chwili trzymać jeden przedmiot. Jako przedmiot rozpoznawane są sterty kamieni umieszczone na mapie.

```text
TAKE
```

### DROP

Upuszcza przedmiot na pole przed postacią.

```text
DROP
```

---

# Instrukcje warunkowe

## IF / ELSE

Przykład:

```text
IF ITEM_TO RIGHT:
    ROTATE RIGHT
    MOVE 1
ELSE:
    MOVE 2
ENDIF
```

---

# Pętle

## WHILE

```text
WHILE OBSTACLE_TO UP:
    ROTATE RIGHT
ENDWHILE
```

## REPEAT

```text
REPEAT 3:
    MOVE 1
ENDREPEAT
```

---

# Procedury

Możliwe jest definiowanie własnych procedur.

Przykład:

```text
PROCEDURE STEP_FORWARD(x):
    MOVE x
ENDPROC

STEP_FORWARD(3)
```

---

# Predykaty i warunki

Dostępne warunki:

## ITEM_TO

Sprawdza czy w danym kierunku znajduje się przedmiot (kamień, który można podnieść).

```text
ITEM_TO RIGHT
```

## OBSTACLE_TO

Sprawdza czy w danym kierunku znajduje się przeszkoda (ściana, przez którą nie da się przejść).

```text
OBSTACLE_TO LEFT
```

### Dostępne kierunki dla ITEM_TO i OBSTACLE_TO:

- LEFT
- RIGHT
- UP
- DOWN

---

# Przykładowy program

```text
ROTATE RIGHT
MOVE 2
TAKE
ROTATE DOWN
MOVE 1
SHIELD
MOVE 3
```

---

# Sterowanie kamerą

W projekcie dostępne jest przybliżanie i oddalanie kamery.

- `+` / przycisk zoom in — przybliżenie
- `-` / przycisk zoom out — oddalenie
- przeciąganie myszką trzymając prawy przycisk — przesuwanie widoku

---

# Mechaniki gry

Projekt zawiera kilka mechanik świata:

- poruszanie po siatce,
- przeszkody terenowe,
- schody i klify,
- przedmioty do podnoszenia.

---

# Przykładowe rozwiązania

## Pokaz funkcjonalności

Poniższe rozwiązanie przedstawia opisane wyżej funkcjonalności i elementy gramatyki, jednocześnie prowadząc na pole końcowe:

```
MOVE 7

ROTATE LEFT

x = 2*3
y = 2 + 3

IF x > y
	REPEAT 4
		ROTATE LEFT
	ENDREPEAT
ELSE
	MOVE 7
ENDIF

IF x < y
	REPEAT 4
		ROTATE LEFT
	ENDREPEAT
ELSE
	MOVE 7
ENDIF

ROTATE RIGHT
MOVE 1

PROCEDURE move_items(n)
	REPEAT n
		TAKE
		ROTATE LEFT
		DROP
		ROTATE RIGHT
	ENDREPEAT
ENDPROC

move_items(2)

MOVE 1

WHILE OBSTACLE_TO RIGHT
	MOVE 1
ENDWHILE

MOVE 1

PROCEDURE if_item_move_else_walk()
	IF ITEM_TO RIGHT:
		ROTATE RIGHT
		MOVE 2
		ROTATE LEFT
		MOVE 2
	ELSE:
		MOVE 5
	ENDIF
ENDPROC

REPEAT 12 / 4
	if_item_move_else_walk()
ENDREPEAT

move_items(3)

MOVE 4

ROTATE LEFT

PROCEDURE move_avoid_trees()
	IF OBSTACLE_TO RIGHT
		ROTATE RIGHT
		MOVE 1
		ROTATE LEFT
		MOVE 3
		ROTATE LEFT
		MOVE 1
		ROTATE RIGHT
	ELSE
		MOVE 1
	ENDIF
ENDPROC

REPEAT 20
	move_avoid_trees()
ENDREPEAT

ROTATE RIGHT

MOVE x
```

## Proste rozwiązanie

Poniżej przedstawione jest mało zaawansowane rozwiązanie pozwalające dotrzeć na pole końcowe, wykorzystujące jedynie podstawowe komendy MOVE, ROTATE, TAKE i DROP:

```
MOVE 8
ROTATE LEFT
MOVE 7
ROTATE RIGHT

TAKE
ROTATE LEFT
DROP
ROTATE RIGHT
TAKE
ROTATE LEFT
DROP
ROTATE RIGHT

MOVE 15
ROTATE RIGHT
MOVE 4
ROTATE LEFT
MOVE 4

TAKE
ROTATE LEFT
DROP
ROTATE RIGHT
TAKE
ROTATE LEFT
DROP
ROTATE RIGHT
TAKE
ROTATE LEFT
DROP
ROTATE RIGHT

MOVE 4
ROTATE LEFT
MOVE 2
ROTATE RIGHT
MOVE 1
ROTATE LEFT
MOVE 26
ROTATE RIGHT
MOVE 5
```

## Proste rozwiązanie z procedurą

Poniżej przedstawione jest to samo rozwiązanie co poprzednio, po dodaniu procedury move_items() służącej do przekładania przedmiotów blokujących drogę, którą można wykorzystać wielokrotnie by zmniejszyć ilość powtarzanego kodu:

```
PROCEDURE move_items(n)
	REPEAT n
		TAKE
		ROTATE LEFT
		DROP
		ROTATE RIGHT
	ENDREPEAT
ENDPROC

MOVE 8
ROTATE LEFT
MOVE 7
ROTATE RIGHT

move_items(2)

MOVE 15
ROTATE RIGHT
MOVE 4
ROTATE LEFT
MOVE 4

move_items(3)

MOVE 4
ROTATE LEFT
MOVE 2
ROTATE RIGHT
MOVE 1
ROTATE LEFT
MOVE 26
ROTATE RIGHT
MOVE 5
```

---

# Technologie użyte w projekcie

- Godot Engine 4.6
- C#
- .NET
- ANTLR4

---

# Autorzy

Projekt wykonany w ramach projektu MiASI.
Jakub Piasecki, Mateusz Adamczak, Michał Herbut, Oskar Krawczyk

---

# Możliwe problemy

## Brak kompilacji C#

Upewnij się, że:

- używasz wersji Godot `.NET`,
- zainstalowany jest .NET SDK,
- Godot poprawnie wykrywa środowisko .NET.

---

## Projekt nie uruchamia się

Spróbuj:

1. usunąć folder `.godot`
2. ponownie otworzyć projekt
3. przebudować projekt C#
