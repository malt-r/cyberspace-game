# Konzept #

## Für welche Zielgruppe / Personas? ##

- Jugendliche & Junge Erwachsene (sollten Englisch sprechen können)

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
    - Rätsel lösen (Logikgatter) -> Lernaspekt, langsam steigender Schwierigkeitsgrad?
        - TODO: Anteil am gesamten Spielablauf festlegen!
    - Collectibles sammeln (Erkundungsmotivation), Freischaltung von Coolem Kram bei Vervollständigung
    - Monster bekämpfen/ davor flüchten (Anteil von Blut und Gedärmen sollte gering sein)
    - Monster sind Computerviren (bekämpfen mit 'Virenscanner')
- Zeit: ca. 15 Minute zum Durchspielen
    - evtl. auch so umsetzen, dass man mehr Zeit drin verbringen kann
- Mehrere Levels

## Story ##

Der Spieler ist in einem Computer gefangen und muss entkommen, indem er die Simulation
durch das Lösen einiger Rätsel (evtl. Logikrätsel?) "manipuliert" und so neue
Fähigkeiten freischaltet.

Erst relativ machtlos, nach und nach stärker werden. Erst Monster große Bedrohung,
nach und nach immer mehr Möglichkeiten, sich zu wehren.

Fähigkeiten sind Firmwaremodule.

Gegner sind Computerviren. "Waffe" ist Virenscanner.

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

- Irgendwie noch Erkundungskomponente hinzufügen, um prozeduralen Aspekt zu berücksichtigen
    (Hauptaufgabe vs. kleine Zusatzinformationen, die man finden kann ('ROM-Module',
    die Hintergrundstory enthalten -> Zähler wird beim Finden inkrementiert, schafft
    Anreiz für Erkundung))
- Einige Monste benötigen die richtige Waffe, um weiterzukommen -> muss andere Räume
    erkunden, um zu finden
    - Bei falscher Waffe vermehren sich evtl. manche Gegner

## Eingabegeräte ##

- Maus/Tastatur
- nur Maus mit zusätzlicher Belegung von Maustasten und Mausrad
- Gamepad
- (Pulssensor & VR, falls möglich)
- druckempfindliche Matte

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
- ja, da gradueller Fähigkeitenzuwachs sich vielfach in der Spieleindustrie
    bewährt hat
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
    - Option, welche das umkehrt (Hardcore-Modus)

# Kernkonzept Besprechung 08_10_21 #

- Proband in Spiele-Evaluation, bekommt VR-Brille auf, ist dann gefangen genommen.
- Spielziel: Fähigkeiten erlangen, um aus Cyberspace zu entkommen
- Währrenddessen:
    - Rätsel lösen (Logikgatter) -> Lernaspekt, langsam steigender Schwierigkeitsgrad?
        - TODO: Anteil am gesamten Spielablauf festlegen!
    - Fähigkeiten freischalten
    - Collectibles sammeln (Erkundungsmotivation), Freischaltung von Coolem Kram bei Vervollständigung
    - Monster bekämpfen/ davor flüchten (Anteil am Spiel sollte gering sein)
    - Monster sind Computerviren (bekämpfen mit 'Virenscanner')
- Musik? Adaptiv? (Falls Zeit) Einfach irgendwas? Nur Soundeffekte?
- Mehrere Level?
- prozedurale Generierung von Levels (was genau generieren? Was vorher definieren?)
    - Räume fertig modellieren, prozedural die Korridore dazwischen erzeugen
- Boss? Irgendwas, was man besiegen / kaputtmachen muss -> Motivation.
