# Konzept #

Arbeitstitel: Cyberspace

## Für welche Zielgruppe / Personas? ##

- Jugendliche & Junge Erwachsene (sollten Englisch sprechen können)
TODO: Personas

## Story ##

Der Spieler ist in einem Computer (Cyberspace) gefangen und muss entkommen, indem er die Simulation
durch das Lösen einiger Rätsel (evtl. Logikrätsel?) "manipuliert", das Level erkundet
"Monster" bekämpft und so neue Fähigkeiten freischaltet.

Erst ist der Spieler relativ machtlos, nach und nach stärker werden. Erst Monster große Bedrohung,
nach und nach immer mehr Möglichkeiten, sich zu wehren.

Gegner sind Computerviren. "Waffe" ist Virenscanner.
<!-- Fähigkeiten sind Firmwaremodule. -->


## Spielidee ##

- Spielziel: Fähigkeiten erlangen, um aus Cyberspace zu entkommen
- Level Teilweise prozedural generiert
    - Räume sind vorher modelliert, werden aber
        prozedural angeordnet und mit Korridoren verbunden
    - Inhalt der Räume auch prozedural generiert (wobei das 'Thema' des Raums vorher
        feststeht)
    - Pro Level (1,2,3...) immer gleiche Aufgaben/Ziele, nur Anordnung unterschiedlich
- Währrenddessen:
    - Fähigkeiten freischalten, um zu entkommen
        - Bei Beginn fehlen die Basisfähigkeiten (z.B. sich umsehen)
    - Rätsel lösen (z.B. Logikgatter)
        - TODO: Anteil am gesamten Spielablauf festlegen!
    - Collectibles sammeln (Erkundungsmotivation), Freischaltung von Coolem Kram bei Vervollständigung
    - Monster bekämpfen/ davor flüchten
    - Monster sind Computerviren (bekämpfen mit 'Virenscanner')
- Zeit: ca. 15-20 Minuten zum Durchspielen
    - evtl. auch so umsetzen, dass man mehr Zeit verbringen kann
- Mehrere Levels
- Low Poly Stil

## Welche Aufgaben werden gelöst ##

- Rätsel lösen (Logikgatter)
- Rätsel mit neu erlangten Fähigkeiten
- Rätsel im "Among Us"-Stil, nur komplexer..
- Collectibles / Secrets sammeln
- Monster besiegen
    - Bsp: letztes Monster lässt den Schlüssel fürs nächste Level fallen
- Truhen / Türen öffnen

## Ziel des Spiels ##

Den "Computer" schlagen und aus dem Cyberspace entkommen.

- Erkundungskomponente
    (Hauptaufgabe vs. kleine Zusatzinformationen, die man finden kann ('ROM-Module',
    die Hintergrundstory enthalten -> Zähler wird beim Finden inkrementiert, schafft
    Anreiz für Erkundung))
- Einige Monster benötigen die richtige 'Waffe', um weiterzukommen -> muss andere Räume
    erkunden, um zu finden
    - Bei falscher Waffe vermehren sich evtl. manche Gegner

## Eingabegeräte ##

- Maus/Tastatur
- nur Maus mit zusätzlicher Belegung von Maustasten und Mausrad
- Gamepad
- (Pulssensor &/ VR, falls möglich)
- (druckempfindliche Matte)

## Barrierefrei? ##

- Rot/Grün-Schwäche sollte bei Rätseln und Leveldesign bedacht
werden (Navigation)
- Blinkeffekte entschärfen (für Epilepsie), per Option
- Bewegung auch nur mit der Maus möglich machen (Laufen mit Mausklick, Interaktion,
Springen mit rechtem Mausklick)
- Festhalten mit Toggle statt dauerhalten

## Spielende beim finden von Zielen unterstützen ##

- Farbliche Hervorhebung von relevanten Spielelementen
    - Umrandung
    - langsames Blinken
- Ansagen vom "Computer"
- Räume, in denen man schon war, werden auf karte angezeigt
- magischer Pfeil, der Richtung anzeigt.
- Hilfetaste (mit Cooldown)
- "Magischer Sand"
- Fähigkeiten & Collectibles

## Navigations-Methoden/Techniken ##

- siehe finden von Zielen
- Evtl. unterschiedliche Arten implementieren und mit evaluieren

## Verfahren einfach in Anwendung und motivierend? ##

- gängige Methoden der Hilfestellung
- Motivation wird dadurch erhalten, dass die Hilfe nur dann aktiviert wird, falls
    man nicht weiterkommt

## Aufgabe/Story motivierend? ##

- Freiheitsgedanke ist attraktiv
- gradueller Fähigkeitenzuwachs hat sich vielfach in der Spieleindustrie
    bewährt
- durch konkrete Ebene/Level immer klare Signale, dass ein Spielabschnitt abgeschlossen
    wurde

## Evaluierungsform? ##

- Fragen direkt im Spiel zu bestimmten Zeitpunkten -> Spielfluss nicht zu sehr unterbrechen
- Fragebogen im Anschluss
    - Schieberegler
    - Checkboxen
- Logging von Spielerbewegung
- Pulsmessung
- Zählen der Betätigungen der Hilfetaste
- Distribution im Browser, macht die Evaluierung einfacher

## Erreichen der Ziele messbar? ##

- Zeit pro Level
- Zeit zwischen Kern-Events (neue Fähigkeit freigeschaltet, Rätsel gelöst, etc.)
    - mit Entfernung zwischen Event-Triggern ins Verhältnis gesetzt
- Sammelaufgabe (und damit Erkundungsaspekt) messbar, indem Anteil von allen
    Sammelobjekten gezählt wird

## Wann ist User-Aufgabe messbar erfüllt ##

- Rätsel geschafft oder nicht
- Level geschafft oder nicht
- Spiel geschafft oder nicht.

## Wie wird gute Nutzbarkeit und Wohlbefinden des Users sichergestellt? ##

- Anzahl der Aktivierungen der Hilfetaste
- Zeit zwischen Kern-Events
- Epilepsie-Disclaimer
- Bei hohem Puls (falls Pulsmesser) Spielgeschwindigkeit verringern / beruhigende
    Musik abspielen
    - Optional umgekehrter Zusammenhang

## Technologie ##

- Engine: Unity
- Platform: Browser (Webassembly)

## Projektplan ##

TODO.

## TODO ##

- Spielidee präzisieren, was soll in den einzelnen Levels passieren?
- Projektplan (Milestones)
- Zuständigkeiten festlegen

## Spielidee präzisieren ##

### Levels ###
- 1
    - Tutorial (nicht generiert, immer die gleichen Voraussetzungen)
        - Story-Exposition
            - Computer sagt hallo
            - Computer erklärt, dass man gefangen ist und keine Macht in dem
                Cyberspace hat
            - Computer gibt Tipps auf sarkastische Art ("Bewege auf gar keinen Fall
                die Maus" -> Mausbewegung führt zum Lernen von Fähigkeit "Sich umsehen", oder
                so)
        - Fähigkeiten:
            - Gucken
            - Umsehen
            - Laufen
        - Hilfetaste erwähnen
        - Monster hinter Gitter darstellen, um Angst hervorzurufen (das erste Mal charakteristischen
            Soundeffekt abspielen)
        - Monster nach Laufen lernen freilassen, um Spieler aus Tutorial Level rauszujagen
        - Danach Tür zum zweiten Teil von Level 1
    - zweiter Teil (generiert)
        - Fähigkeiten:
            - Sprinten (um schnell genug über Abgrund springen zu können)
            - Springen (um über Abgrund springen zu können)
            - Iteragieren (um Rätsel zu starten, Items/Collectibles aufzuheben)
        - Collectibles einführen (2 Stück)
        - Frei laufende Monster, die noch nicht besiegt werden können?
            (evtl. schwierig im Balancing)
    - nach Level-Abschluss ein Zusammenfassungs-Bildschirm, der Statistik anzeigt
        - Zeit
        - besiegte Monster
        - gefundene Collectibles
        - evtl. Hochrechnung (Level __gut__ abgeschlossen oder schlecht?)
- 2
    - Kampfmechnik einführen
    - Rätselmechanik einführen
    - neue Fähigkeiten
        - Dishonored-style 'Blink'? -> eher schwierig, sinnvoll einzusetzen
        - Zeit verlangsamen? -> technisch einfach möglich? Evtl. sowieso durch
            Pulsmessung triggern
        -
    - mehr Collectibles
- 3
    - Kampfmechanik und Rätselmechanik weiter herausfordern
        - mehr Gegner
        - schwerere Rätsel
        - neue 'Waffe', welche für anderen Gegnertyp gebraucht wird
    - mehr Collectibles
    - Bosskampf

### Fähigkeiten ###
- Tutorial Level (erster Teil Level 1)
    - Gucken
    - Umsehen
    - Laufen
- Sprinten (Falle,)
- Springen


### Aufgaben ###


<!-- Kerstin zu gitlab hinzufügen -->

<!-- Andre Gruppe zu Assets fragen -->

