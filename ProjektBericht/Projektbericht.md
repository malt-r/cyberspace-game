---
title: "Projektbericht Cyberspace"
author: [Malte Reinsch (1111598), Tim Lücking (1109744), Dennis Eller (1109421)]
date: "02.02.2022"
lang: "de"
titlepage: true
toc-own-page: true
secPrefix: "Kapitel"
figPrefix: "Abbildung"
lofTitle: "Abbildungsverzeichnis"
figureTitle: "Abbildung"

---

\pagenumbering{gobble}
\pagebreak
\listoffigures
\pagenumbering{arabic}
\pagebreak

# Einleitung #

Im Rahmen des Moduls "Computer Games (Visualisierung)" wurde das Videospiel "Escape
the Cyberspace" entwickelt. Dieser Projektbericht dokumentiert das Konzept, die Umsetzung
und die daran anschließende Evaluierung des Spiels.
Hierzu wird zunächst das Konzept und die Projektplanung beschrieben. Hierauf folgt
eine detaillierte Beschreibung der Umsetzung des Konzepts und eine Spielanleitung.
Abschließend werden das Vorgehen und die Ergebnisse der Evaluierung dargestellt.

# Konzept #

## Spielidee ##

Die Idee des Spiels ist, dass der Spielcharakter von einem Computer in einem 3D-Level
gefangen gehalten wird (dem "Cyberspace"). Anfangs startet der Spielcharakter ohne die
oftmals in Videospielen vorausgesetzten Fähigkeiten (wie z.B. Springen und Sprinten) und
muss den Cyberspace erkunden, um diese Fähigkeiten zu erlernen. Das Spielziel dabei ist,
den Computer zu besiegen und anschließend aus dem Cyberspace zu entkommen.

Das Spiel ist in drei Level aufgeteilt. Im ersten Level erlernt der Spielcharakter
die Fähigkeiten Springen und Sprinten und muss dazu Rätsel lösen, welche im Level
versteckt sind. Dabei werden die Aktionen des Spielcharakters sarkastisch vom Computer
kommentiert, um die Motivation zu erhöhen, den Computer zu besiegen.

Im zweiten Level erlangt der Spielcharakter einen Scanner, mit dem bestimmte Items aufgesaugt werden
können. Um weiter im Level voranzukommen, muss der Spielcharakter den Lasermodus
für den Scanner finden, mit dem Gegner bekämpft werden können. Thematisch passend
werden diese Gegner "Computerviren" genannt. Das zweite Level ist deutlich mehr von
der Kampfmechanik bestimmt. Dieser Laser verbraucht Energie und kann überhitzen, sobald
die Energie aufgebraucht ist, so wird sichergestellt, dass der Laser nicht dauerhaft
genutzt werden kann. Am Ende vom zweiten Level muss der Spielcharakter einen Computervirus
besiegen, der Bomben wirft. Anschließend kann der Spieler den Bombenmodus für den Scanner
aufheben.

Während der Erkundung der Level können sogenannte "Collectibles" gefunden werden,
welche keine besondere Funktion haben, außer den Sammeltrieb anzusprechen und so
die Erkundung der Level zu motivieren.

Um den Wiederspielwert der Level zu erhöhen werden die beiden Level prozedural generiert.
Dabei werden die Räume fest vordefiniert und in randomisierter Anordnung platziert und
verbunden.

Das dritte Level stellt den Abschluss der Geschichte des Spiels dar, daher muss
der Spielcharakter in diesem Level gegen den Computer selbst antreten (dargestellt
durch einen Röhrenmonitor). Der Bosskampf ist dabei in zwei Phasen aufgeteilt, indem
erst Schildgeneratoren durch gezielte Bombenwürfe durch den Spielcharakter zerstört werden müssen.
Abschließend muss der Computer noch mit einer Bombe getroffen werden.

Das Spiel soll mithilfe der Unity Gameengine umgesetzt werden. Diese Engine ist
sehr weit verbreitet, kostenlos nutzbar und insbesondere für kleinere Projekte sehr beliebt. Im Vergleich
zu der ebenfalls kostenlos nutzbaren Unreal Engine ist die Lernkurve deutlich flacher.

## Meilensteine ##

| **KW** | **Meilenstein** |
| - | --------|
| 42 | Konzept verfeinert, Projektplanung und Zuständigkeiten geklärt|
| 43 | Konzept für Generatoralgorithmus steht, Spielerbewegung funktioniert, Sandboxraum für Grafik- / Spielmechaniktests existiert|
| 44 | Design für Collectibles finalisiert, Monsterdesign finalisiert, erste Räume vordefiniert, Levelgenerator prototypisch implementiert, Basis für Kampfmechanik steht​|
| 45 | Levelgenerator finalisiert, Texturen für Dungeon finalisiert, Kampfmechanik prototypisch implementiert|
| 46 | Kampfmechanik finalisiert, Collectible placement implementiert, Levelgenerator ist game-ready, Kampfmechanik implementiert, ​|
| 47 | Fähigkeiten prototypisch implementiert, Rätselmechanik und erste Rätsel implementiert, Sprachausgabe (Computer) prototypisch implementiert, Minimap prototypisch implementiert|
| 48 | Fähigkeiten implementiert, Hilfetaste prototypisch implementiert, Spielflussmanager|
| 49 | Prototyp implementiert, Forschungsfragen sammeln |
| 50 | Letzte Fehler behoben; fertige Versuchsdurchlaufsbeschreibung |
| 51 | |
| 52 | Spiel finalisiert |
| 01 | Erste Evaluierungsrunde durchgeführt |
| 02 | Evaluierung abgeschlossen |
| 03 | Projektbericht abgeschlossen |
| 04 | Abschlusspräsentation erstellt |
| 05 | Abschlusspräsentation gehalten |

## Arbeitspakete und Zuständigkeiten ##

Tim Lücking:

- Level-Design
- Minispiel
- Türmechanik

Dennis Eller:

- Spielerbewegung
- Aufsammelbares
- Kampfmechanik
- Gegner
- Sound
- Menüs

Malte Reinsch:

- Level-Generator
- Minimap
- Story & Erzähler
- Tutorial
- Bosskampf Soundtrack
- GameManager

# Umsetzung #

## Framework ##

Das Framework sammelt einige Komponenten, die in jeder Szene für den Spielablauf benötigt werden.
Im Folgenden werden die wichtigsten Komponenten kurz vorgestellt.

### GameManager ###

Der GameManager ist für übergeordnete Aufgaben zuständig. Hierzu zählen:
- Laden der Spielszenen und Zwischenmenüs
- Anstoßen der Level-Generierung
- Instanziierung des Spielcharakters am Spawnpunkt
- Erstellung der Minimap
- Logging von Statistiken über den aktuellen Spieldurchlauf
- Exportieren der Statistiken als `.json`-Datei

## Generator ##

Der Level-Generator wird vom GameManager nach dem Laden der Spielszene aufgerufen.
Die Aufgabe des Level-Generators ist die Konstruktion eines Levels aus bereits
vordefinierten Räumen in einer teilweise randomisierten Anordnung. Zwischen den
Räume bestehen teilweise Abhängigkeiten, z.B. muss vor einem Raum mit einem Abgrund
der Raum erreichbar sein, in dem der Spieler die Fähigkeit Springen erlernt.

Um diese Abhängigkeiten besser abbilden zu können, werden die Räume in Partitionen aufgeteilt,
alle Räume in einer Partition können beliebig miteinander durch Korridore verbunden werden.
Diese Partitionierung geschieht auf Basis von "StoryMarker"-Komponenten, die den Raum-Objekten
untergeordnet sind und eine "StoryIndex"-Property haben. Der StoryIndex gibt die
grobe Reihenfolge vor, in der die Räume von Spieler durchlaufen werden. Ein StoryMarker
kann als "Barriere" markiert werden, wodurch angegeben wird, dass durch diesen Raum
eine Partition beendet und eine neue begonnen werden muss.

Zur Vereinfachung der Arbeit des Generators operiert dieser auf einem Gitter. Die
zu platierenden Räume und Korridore sind auf die Dimensionen der Gitterzellen abgestimmt.

Basierend auf den Dimensionen der Räume in einer Partition berechnet der Generator einen
Bereich auf dem Gitter, in dem die Räume mit zufälliger Position und Rotation platziert werden können.
Stellt der Generator eine Kollision der Räume fest, wird dieser Prozess erneut gestartet,
bis eine gültige Raumanordnung gefunden wird.


![Platzierte Räume](./pics/gitter.png)

Anschließend müssen die Räume durch Korridore verbunden werden. Die Türen der Räume
sind durch "DoorMarker"-Komponenten markiert, aus deren Positionen die passenden Gitterzellen-Indizes
berechnet werden. Die so markierten Zellen fasst der Generator anschließend zu
Türpartitionen zusammen. Enthält eine Raum eine Barriere, so wird die Tür vor der Barriere
zu der Türpartition hinzugefügt, in der auch die Türen der Räume mit kleineren StoryIndizes
enthalten sind. So wird sichergestellt, dass alle Türen vor der Barriere miteinander verbunden werden.
Die Tür hinter der Barriere wird dementsprechend zu der nachfolgenden Türpartition hinzugefügt.

Um Pfade zwischen den Türen einer Türpartition zu definieren wird im ersten Schritt eine
Delauney-Triangulation zwischen allen betroffenene Türen berechnet (bzw. der ersten Zellen,
die im Korridor an die Tür andocken).

![Delauney-Triangulation der Türen](./pics/delauney.png)

Um die Verbindungen zwischen den Türen zu reduzieren wird ein minimaler Spannbaum auf
der Delauney-Triangulation konstruiert. Das Ergebnis ist im untenstehenden Bild zu erkennen.

![Minimaler Spannbaum](./pics/mst.png)

Mit dem minimalen Spannbaum sind die finalen Verbindungen zwischen den Türen bekannt.
Für jede dieser Verbindungen muss anschließend ein Pfad im Gitter gefunden werden, sodass
die Korridore zwischen den Räumen platziert werden können. Hierfür wird der A\*-Algorithmus
verwendet. Die Kostenfunktion wird dabei so gewählt, dass der Generator keine Pfade
durch andere Räume oder Pfade einer anderen Partition wählt. Das Ergebnis dieser
Phase ist im untenstehenden Bild zu sehen.

![Ermittelte Pfade](./pics/path.png)

Entlang der so definierten Pfade werden pro Zelle Korridormodule eingesetzt, um einen
zusammenhängenden Korridor zu erzeugen. Hierfür muss der Korridortyp für jede Zelle
ermittelt werden (gerades Stück, Kurve, T-Kreuzung) und die Rotation, mit der das
entsprechende Korridormodul eingesetzt werden muss. Die fertig konstruierten Korridore
sind in der untenstehenden Abbildung zu erkennen.

![Eingesetzte Korridormodule](./pics/corridor.png)

## GUI und Menüs ##

### Zwischenmenü ###

Das Zwischenmenü wird nach Vollendung des ersten und zweiten Levels angezeigt und
fasst den Durchlauf des Levels zusammen. Es zeigt, wie in der untenstehenden
Abbildung zu erkennen, die Anzahl der gefundenen Collectibles, die benötigte
Zeit und die Anzahl der Tode an. Außerdem werden einige Informationen für
die Evaluierung dargestellt.

![Zwischenmenü](./pics/zwischenmenu.png)

### Minimap ###

Die Minimap zeigt in zwei unterschiedlichen Formen einen kleinen Ausschnitt des Levels
an, um die Orientierung im Level zu erleichtern. Hierfür verwendet die Minimap die
Gitterdaten, welche bei der Erstellung des Levels durch den Level-Generator erzeugt werden,
um eine Miniatur des Levels aufzubauen. Diese Miniatur besteht aus Kacheln, welche
die Wände des Levels als grüne Linien darstellen. Über der Miniatur wird ein Pfeil
passend zur Spielerbewegung bewegt und mit einer orthografischen Kamera auf eine
Rendertextur in der GUI projeziert.

In der Basisminimap ist anfangs nur ein kleiner Teil der Minimap sichtbar. Beim Betreten
einer neuen Zelle oder eines neuen Raums werden diese auf der Minimap aufgedeckt.
Um besser erkennen zu können, an welchen Stellen der Spieler das Level noch weiter erkunden kann,
werden die angrenzenden (aber noch nicht betretenen) Zellen abgedunkelt dargestellt.
Hierzu wird eine dunkle, transparente Kachel über die entsprechenden Minimap-Kacheln gelegt.
Ein Beispiel ist in den untenstehenden Abbildungen zu erkennen.

![Aufbau der Basis-Minimap in der Szene](./pics/basic_minimap_scene.png){width=60%}

![Basisminimap](./pics/basic_minimap.png){width=40%}

Die erweiterte Minimap zeigt den Weg zur nächsten Story-Aufgabe als rote Linie durch das Level
an. Hierfür wird auf Basis der Gitterdaten der A\*-Algorithmus angewandt, um einen Pfad
von der aktuellen Spielerposition zu der Position des aktuell aktiven Storymarkers
zu finden. Die Punkte dieses Pfads werden anschließend in einem Linerenderer über den Minimap-Kacheln
dargestellt. Ein Beispiel ist den untenstehenden Abbildungen zu entnehmen.

![Aufbau der erweiterten Minimap in der Szene](./pics/extended_minimap_scene.png){width=60%}

![Erweiterte Minimap](./pics/extended_minimap.png){width=40%}

## Story ##

Die Story wird insbesondere durch die angezeigten Aufgaben an den Spieler kommuniziert.
Hierfür ließt der StoryManager aller StoryMarker des Levels ein und sortiert diese
nach StoryIndex.

Jeder StoryMarker kann eine Beschreibung enthalten, welche bei dessen Aktivierung über
die GUI angezeigt wird. Diese Beschreibung gibt an, welche Spieleraktion erforderlich ist,
um die Story-Aufgabe abzuschließen. Da dies viele unterschiedliche Spieleraktivitäten beinhaltet
(über einen Abgrund springen, ein Rätsel lösen, einen Gegner besiegen) ist die StoryTrigger-Komponente
in der Lage, ein Event mit einem aktivierten StoryMarker an den StoryManager zu senden.
Der StoryTrigger wird durch diverse spezifische Trigger überladen, um die verschiedenen
zu absolvierenden Spieleraktionen zu erfassen.

## Design ##

### Räume ###

Räume werden in einem Level platziert und mit Korridoren verbunden. Dabei müssen die Räume von den Korridoren
unterscheidbar sein. Grundsätzlich besitzen die Räume eine höhere Decke und sind breiter als die Korridore.

Die Räume müssen so gebaut sein, dass eine Geschichte erzählt werden kann und das Level spielbar ist. Barrieren in
Räumen müssen so entworfen werden, dass sie nicht passierbar sind, sofern nicht eine bestimmte Fähigkeit erlernt wurde.

Es sind Räume notwendig, welche als Barriere der Geschichte wirken. Nachdem die Testperson *Springen* erlernt, kann
der Raum mit einem Abgrund als Barriere passiert werden. Blaue Lava am Boden des Abgrunds soll der Testperson zeigen,
dass beim Runterfallen die Testperson im Spiel stirbt und respawned wird. Ein Passieren ohne die Fähigkeit *Springen* ist
nicht möglich. Die Testperson kann ebenfalls das erste Level nicht abschließen ohne die Fähigkeit *Sprinten* erlernt zu haben.
Im Raum des Ausgangs befindet sich ebenfalls ein Abgrund, welcher breiter als im bereits vorgestellten Raum ist. So ist
ein Passieren nur mit einem Sprung während man sprintet möglich. Im zweiten Level muss vor dem Besiegen des ersten Gegners
der Lasermodus aufgehoben werden. Der erste Kampfraum kann nicht ohne diese Fähigkeit passiert werden. Im Folgenden Abbildungen
wird ein Raum mit Barriere und eine verschlossene Tür gezeigt.

<div id="fig:barriere">
![Raum mit Barriere](./pics/Design/Barriere.png){width=70%}
![Tür als Barriere](./pics/Design/tuer.png){width=30%}

Räume mit Barriere
</div>

Das Ende eines Levels wird dabei mit Hilfe eines Portals und einem Ausgangs-Schild dargestellt. Das folgende Bild stellt
das Ende eines Levels dar.

![Ende eines Levels](./pics/Design/Exit.png){width=40%}

Um die Fähigkeiten erlernen zu können werden ebenfalls Räume benötigt, welche Schlüsselinhalte beinhalten. In diesen Räumen
können Fähigkeiten nach dem Abschließen eines Minispiels erlernt werden. Diese Räume besitzen ein Minispiel oder einen Gegner,
welcher besiegt werden muss. Auf das Minispiel wird in [@sec:Minispiel] eingegangen. Im Folgenden wird ein solcher Schlüsselraum
dargestellt.

![Raum mit Schlüssel](./pics/Design/Schluessel.png){width=70%}

Zum Schluss werden Räume benötigt, welche nicht von der Geschichte abhängig sind. Diese Räume können beispielsweise
Collectibles (siehe [@sec:Collectibles]), Gegner und Jump'n'Run-Einlagen beinhalten. In folgender Abbildung wird so ein geschichtsunabhängiger
Raum dargestellt.

![Geschichtsunabhängiger Raum](./pics/Design/nichtwichtig.png){width=80%}

Die Textur der Räume ist simpel gehalten und ist eine Anlehnung an die alte Darstellung des *Cyberspace*. Der Film
*Tron* dient beispielsweise als Inspiration für das simple Design. Gleichzeitig erschwert die immer gleichbleibende
Textur das Orientiern im Level. Dies wurde gewollt umgesetzt, damit Testpersonen auf die Minimap angewiesen sind und
öfters auf diese schauen. Die Farbe grün wurde wegen ihrer entspannenden Wirkung gewählt. Der Bloom-Effekt ist ebenfalls
an alte *Cyberspace*-Vorstellungen angelehnt und soll zeigen, dass die Textur der Räume leuchtet. Die folgende Abbildung zeigt die
Textur im Spiel.

![Raumtextur](./pics/Design/Textur.png){width=50%}


### Collectibles {#sec:Collectibles}

Die Collectibles besitzen ein abstraktes Modell. Sie leuchten und drehen sich, um die Aufmerksamkeit auf sich zu richten.
Zusätzlich emittieren sie ein Geräusch, welches nur in einem bestimmten Umkreis zu hören ist. Im ersten Raum des Spiels
wird der Testperson ein Collectible gezeigt, welches aufgehoben werden kann. So werden Collectibles den Testpersonen vorgestellt.
Im Folgenden wird ein Collectible dargestellt.

![Collectible](./pics/Design/collectible.png){width=50%}

Die weiteren Collectibles sind in den Leveln in verschiedenen Räumen verteilt. Diese sind teils versteckt, werden direkt
gezeigt oder benötigen Anstrengung der Testperson. Für ein paar Collectibles muss ein Jump'n'Run abgeschlossen werden. Weitere sind
in Räumen versteckt, welche sich abseits des Hauptpfades befinden. Andere Collectibles werden den Testpersonen direkt beim
Betreten eines Raumes gezeigt, befinden sich aber beispielsweise an einer erhöhten Person, die es zu erklimmen gilt. Zusätzlich
werden Collectibles ebenfalls von Gegnern bewacht oder befinden sich in Sackgassen innerhalb der Korridore.

Ein Collectible ist besonders versteckt. Zum Erlangen des Collectibles wird die Fähigkeit *Sprung* benötigt. Der Raum, in dem
sich das Collectible befindet wird dabei von der Testperson vor dem Erhalten der *Sprung*-Fähigkeit passiert. Um dieses einsammeln
zu können, müssen Testpersonen die Fähigkeit erlangen und zurück gehen, um das Collectible einsammeln zu können. So ist es
möglich zu schauen, ob eine Testperson dazu bereit ist, für Collectibles zurück zu gehen.

### Minispiel {#sec:Minispiel}

Das Minispiel dient dem Erhalten bzw. Erlernen einer Fähigkeit. Es ist an ein Minispiel vom Hit-Game *Among Us* angelehnt.
Aufgabe ist es, die zufällig platzierten Farben der linken Seite mit dazugehöriger Farbe auf der rechten Seite zu verbinden.
Für Farbenblinde werden ebenfalls Buchstaben zur Unterscheidung an die Farben geschrieben. Nach erfolgreichem Abschließen
eines Minispiels wird eine Fähigkeit erlernt oder eine Tür geöffnet. Im folgenden wird das Design und die Funktionsweise
des Minispiels dargestellt.

<div id="fig:minigame">
![Minispiel](./pics/Design/Minispiel1.png){width=66%}
![Minispiel mit verbundenen Farben](./pics/Design/Minispiel2.png){width=33%}

Minispiel
</div>

### Gegnerdesign ###

Die Gegner im Spiel sollen Computerviren darstellen. Aus diesem Grund wurden virusähnliche Assets ausgewählt.
Zur spielerischen Abwechslungen soll die Testperson verschiedene
Monster besiegen, die unterschiedliche Angriffsarten besitzen.
Der Nahkampfgegner (siehe [@fig:gegnera]) stellt eine so genannte *Bakteriophage* dar. Das Asset besitzt Animationen für einen Nahkrampangriff
und für die Bewegung mit Hilfe der "Beine". Der Bombenvirus (siehe [@fig:gegnerb])
stellt einen gepanzerten Virus dar und kann Bomben werfen. Der Fernkampfvirus wurde bewusst abstrakt gewählt (siehe [@fig:gegnerc]).

<div id="fig:gegner">
![Nahkampfgegner](./pics/Design/Nahkampf.png){#fig:gegnera width=30%}
![Bombengegner](./pics/Design/Bombenvirus.png){#fig:gegnerb width=36%}
![Fernkampfgegner](./pics/Design/Fernkampfvirus.png){#fig:gegnerc width=33%}

Gegnertypen
</div>

### Items ###

Als Items werden farbige Würfel verwendet. Dabei gibt es die Farben grün, rot und schwarz. Das grüne Item lädt beim Aufheben
Lebensenergie der Testperson auf. Der rote Würfel dient zum Erlernen des Lasermodus, der schwarze Würfel zum Erlenen des Bombenmodus. In folgender Abbildung sind die drei Würfel dargestellt.

![Items](./pics/Design/items.png){width=50%}

### Waffe ###

Als Waffe wurde ein Asset aus dem Internet gewählt. Es ist ein Model mit wenigen Polygonen und passt deswegen gut in die
erstellte Spielwelt. Ein Farbiger Zylinder auf der Waffe zeigt den aktuell ausgewählten Modus der Waffe an. Im folgenden wird
die Waffe dargestellt.

![Waffe](./pics/Design/Waffe.png){width=50%}

## Spielmechanik ##

### Spielbewegung ###

Für die Bewegung des Spielercharakters wird der First Person Controller (FPS-Controller)
von Unity verwendet. Dabei handelt es sich um einen Playercontroller auf Basis des
Charactercontrollers, der nicht rigbidbodybasiert ist. Zum Steuern der
Bewegungsfähigkeiten wird der FPS-Controller um die Funktion ergänzt, einzelne Funktionen umzuschalten.
Die Instanz des Spielercharakters in Unity ist in folgender Abbildung dargestellt.

![Spielcharakter in Unity](./pics/unity_player.png){#fig:unity_character}

Zum Steuern des Charakters wird das neue Input System von Unity verwendet.
In dem neuen System kann ein Input Action Asseet erstellt werden, welche verschiedene
Actionmaps beinhaltet. Das Input Action Asset ist in folgender Abbildung dargestellt.

![Input Action Asset](./pics/unity_input.png){#fig:unity_inputactionasset}

Im Input Action Asset ist immer eine Actionmap aktiv. Durch Actionmaps können
Gruppen von Aktionen (Actions) definiert werden, die unabhängig voneinander sind.
Dadurch können die Spielinteraktionen von den Interaktionen im Menü und Minispiel
getrennt werden. Dadurch wird verhindert, dass sich der Charakter bewegt, während
sich der Teilnehmende im Menü oder Minispiel befindet. Durch die Verwendung von
Actionmaps wird komplexer Code vermieden, der Eingaben der Teilnehmenden verarbeiten muss.
Des Weiteren können an dieser Stelle ohne Änderungen am Code weitere Eingabegeräte
hinzugefügt werden, wie z.B. Gamepad oder VR-Controller.

### Aufsammelbares ###

Im Spiel kann der Charakter verschiedene Gegenstände einsammeln. Bis auf die
Collectibles und den Scanner können die Gegenstände nur durch den Absorbermodus
des Scanners aufgenommen werden. Alle Gegenstände haben eine gemeinsame Basisklasse,
wodurch sie entweder mit dem Spielcharakter oder dem Absorberstrahl interagieren.
Wird ein Gegenstand von dem Absorberstrahl getroffen, wird dieser in Richtung
des Spielchatakters gezogen. Hat sich der Gegenstand dem Charakter ausreichend genähert,
wird dieser aufgenommen. Wird ein Lebenswürfel aufgesammelt, erhöhen sich die aktuellen
Lebenspunkte des Charakters.

Mit dem Absorber können auch weitere Scannermodi aufgesammelt werden. Dazu zählt
einmal der Lasermodus, mit dem Gegner im Level besiegt werden können. Des Weiteren
kann auch ein Bombenmodus aufgesammelt werden, mit dem der Teilnehmende Bomben
werfen kann, die mehrere Gegner und sich selbst Schaden zufügt.

### Gegner ###

Im Spiel muss der Teilnehmende verschiedene Gegner besiegen. Damit die Gegner
für den Teilnehmenden eine Herausforderung darstellen, können die Gegner den
Spielcharakter besiegen. Die Gegner verwenden den Navmesh Agent von Unity, um
die Bewegung durch Wände oder Hindernisse zu unterbinden. Der Einsatz des Navmesh
und des Navmesh Agent ist in foglender Abbildung dargestellt.

![Navmesh Agent und Navmesh](./pics/unity_navmesh.png){#fig:unity_navmesh}

Des Weiteren besitzen die Gegner einen PlayerDetector um den Spielcharakter zu sehen.
Der PlayerDetector ist in folgender Abbildung dargestellt.

![Raycast Check des PlayerDector](./pics/unity_playerdetector.png){#fig:unity_playerdetector}

Die gründe Sphäre zeigt die Sichtweite des Gegners an, während die pinke Sphäre
die Reichweite der aktuellen Waffe darstellt. Um zu verhindern, dass die Gegner
den Spielcharakter durch Wände sehen können, wird zwischen dem Charakter und Gegner
ein Raycast gezogen. Trifft der Raycast auf dem Weg zum Spielcharakter auf eine Wand
oder Hindernis, wird vom Playerdetector kein Charakter an den Gegner weitergegeben.

![Raycast Check des PlayerDector](./pics/unity_enemyraycast.png){#fig:unity_raycastcheck}

Die Gegner besitzen wie der Spielcharakter einen WeaponHolder der die Waffe
abstrahiert. Dadurch kann ohne Codeänderung im Gegner die aktive Waffe gewechselt
werden, was vielseitige Angriffsstrategien ermöglicht. Im Model befindet sich das 3D Modell des Gegners.
Das Model beinhaltet damit auch die Teile des Gegners, die animiert werden.
Das hat den Vorteil, dass Animationen nicht mit der Spiellogik interferieren.


<!-- Virenarten aufzählen?-->

### Kampfmechanik ###

Im Konzept wird ein Scanner als Waffe vorgeschlagen der über Energie statt
Munition verfügt. Damit wird verhindert, dass dem Spielcharakter die Munition
ausgeht sowie, dass der Teilnehmende nicht taktisch vorgehen muss, weil unendlich
Munition zur Verfügung steht.
Die Energie regeneriert sich von selbst, wenn der Scanner nicht verwendet wird
und hat einen Wert von maximal 100 Punkten. Fallen die Energiepunkte auf 0, überhitzt
der Scanner und kann für einen Zeitraum von 3 Sekunden nicht verwendet werden. Währenddessen
steigt Qualm auf, der die Überhitzung andeutet. Nachdem der Scanner abgekühlt ist,
regenreriert sich der Scanner wieder. Während der Absorber keine Energie verbraucht,
benötigt der Lasermodus pro Sekunde 10 Energiepunkte. Das Verwenden des Bombenmodus sorgt
für ein sofortiges Überhitzen des Scanners, wodurch ein taktisch kluges Verwenden der
Bombe nötig ist. Das Überhitzen des Scanners ist in folgender Abbildung dargestellt.

![Überhitzung des Scanners](./pics/unity_overheat.png){#fig:unity_overheat}

Mit dem Laser- und Bombenmodus kann der Spielcharakter die Gegner im Level besiegen.
Der Kampf gegen einen Nahkampfvirus mit dem Lasermodus ist in folgender Abbildung dargestellt.

![Kampf gegen einen Nahkampfvirus](./pics/unity_meleefight.png){#fig:unity_meleefight}

Der Nahkampfvirus lässt einen Gesundheitswürfel fallen, nachdem er besiegt wurde.
Die anderen Monsterarten lassen ebenfalls Gesundheitswürfel fallen. Der
Bombenvirus stellt eine Ausnahme dar, da er neben einem Gesundheitswürfel auch den
Bombenmodus für den Scanner fallen lässt. Das Fallenlassen von Gesundheitswürfeln
ist in folgender Abbildung dargestellt.

![Fallen gelassene Gesundheitswürfel eines Gegners](./pics/unity_health.png){#fig:unity_health}

Um die Begegnungen mit den Monstern herausfordernd zu gestalten, können sie dem
Spielcharakter Schaden zufügen. Der zuvor gezeigte Nahkampfvirus kann nur Nahkampfschaden
austeilen, während die anderen beiden Virenarten dem Spielcharakter mit Projektilen oder Bomben Schaden zufügen können.

### Bosskampf ###

Im letzten Level findet ein Bosskampf statt. Hier trifft der Teilnehmde auf den
Computer, der den Teilnehmenden im Cyberspace eingesperrt hat. Beim Betreten des
Raumes wird dieser verschlossen und der Computer erscheint. In diesem Level
kommen alle Monsterarten zusammen. Der Teilnehmende benötigt alle Lasermodi um
den Computer zu besiegen. Der Computer lässt in einem festen Intervall Lava vom
Boden aufsteigen, die dem Spielcharakter als auch den Gegnern Schaden zufügt.
Die Kampfszene mit der Lava ist in folgender Abbildung dargestellt.

![Bosskampf](./pics/unity_bossfight.png){#fig:unity_bosslava}

Um den Computer zu besiegen müssen die Schildgeneratoren zerstört werden. Nachdem
alle vier Schildgeneratoren zerstört wurden, kann der Computer mit einer Bombe
zerstört werden. Das Zerstören eines Schildgenerators ist in foglender Abbildung dargestellt.

![Zerstörter Schildgenerator](./pics/unity_shielddestroyed.png){#fig:unity_bossshield}

Nachdem der Computer besiegt wurde, verschwindet die Lava sowie alle Monster aus
dem Raum. Die Türen öffnen sich und der Spieler kann den Cyberspace verlassen.

# Spielanleitung #

TODO.

# Evaluierung #

Nach der Umsetzung wird das Spiel mithilfe von Probanden evaluiert. Dazu wird zuerst der Aufbau der Evaluierung vorgestellt.
Im Anschluss folgen die Ergebnisse.

## Aufbau ##

Zunächst werden die aufgestellten Forschungsfragen und Hypothesen vorgestellt.
Im Anschluss folgen die Fragebögen, mit denen die Forschungsfragen evaluiert werden sollen.
Daten, die über Fragebögen nicht zuverlässig erhoben werden können, werden von dem GameManager aufgenommen. Zum Schluss werden Versuchsdurchführung und -ablauf dargestellt.

### Forschungsfragen und Hypothesen ###

Für die Evaluierung wird die Art der Minimap als Variable ausgewählt. In der Evaluierung sollen zwei Arten von Minimaps evaluiert werden.
Beide Minimap-Arten zeigen das Level in Vogelperspektive. Beide Minimap-Arten sind in folgender Abbildung dargestellt.

<div id="fig:figureRef">
![Basisminimap](./pics/basic_minimap.png){width=50%}
![Erweiterte Minimap](./pics/extended_minimap.png){width=50%}

Zu untersuchende Minimaparten
</div>

Die Basisminimap wird für einen Raum aufgedeckt, sobald der Proband einen Raum betritt.
Angrenzende Räume werden durch eine transparente Schattierung angezeigt.
Die erweiterte Minimap ist von Beginn an komplett aufgedeckt zeigt den Weg zur nächsten zu lösenden Storyaufgabe.
Anhand der zu untersuchenden Variable wurden folgende Forschungsfragen mit den zugehörigen Hypothesen formuliert:

- F1: Welchen Einfluss haben Navigationshilfen auf die Erkundung in einem 3D Level?
    - H1: Mit der Basisminimap werden mehr Collectibles gefunden
    - H2: Mit der erweiterten Minimap wird das Level schneller abgeschlossen
- F2: Welchen Einfluss haben Navigationshilfen auf die Orientierung in einem 3D Level?
    - H3: Mit der erweiterten Minimap wird die Orientierung im Spiel als einfacher wahrgenommen
- F5: Welche Navigationshilfe wird von den Nutzern bevorzugt?
    - H4: Probanden mit stärkerer “Achiever”-Ausprägung bevorzugen die ausgebaute Minimap
    - H5: Probanden mit stärkerer “Explorer”-Ausprägung bevorzugen die Basisminimap

### Fragebögen ###

Zur Evaluierung der Hypothesen und zum Erfassen von generellem Feedback werden folgende Fragebögen verwendet:

- Bartletest
- Minimapfragebogen
- Fragebogen über Demographie und allgemeiner Fragebogen

Es werden sowohl Likert-Skala-Fragen, als auch Freitextfragen gestellt.
Bei den Likert-Skala-Fragen wird eine 5er Skala von -2 bis 2 ("Stimme garnicht zu" bis "Stimme voll zu") verwendet.

**Bartletest**

Der Bartletest klassifiziert die Probanden in vier Spielertypen nach Bartle (von nun an: "Bartletyp").
Die Fragen dienen der Bestimmung der Spielinteressen.
Der Test basiert auf einem Paper von Richard Bartle und wurde ursprünglich für
Multiplayer-Spiele entworfen, wird jedoch mittlerweile auch für Singleplayer-Spiele verwendet.

**Minimap**

Zur Evaluierung der Forschungsfragen bezüglich der Minimap-Arten wird ein Fragebogen
entworfen, der sich am IEQ (Immerssive Experience Questionaire) orientiert.
Dieser besteht aus einer Reihen von Fragen, welche einer 5-Likert-Skala von "Stimme garnicht zu" bis "Stimme voll zu" folgen.
Dieser Fragebogen wird nach jedem Level von den Probanden ausgefüllt.

**Demographie und Allgemein**

Im demografischen Fragebogen wird das Alter, Geschlecht, der aktuelle Berufsstatus
und Spielerfahrung erfragt, um Informationen über die Probanden zu erhalten.
Des Weiteren wird allgemeines Feedback über das Spiel im allgemeinen Fragebogen
gesammelt. Dieser besteht auch aus einer Reihen von Fragen, die einer 5-Likert-Skala
von "Stimme garnicht zu" bis "Stimme voll zu" folgen. Neben den Likert-Skala-Fragen
werden über Freitextfragen Anregungen zu den Minimap-Arten gesammelt.

### Messdaten ###

Neben der Evaluierung über Fragebögen werden folgende Daten während des Spielens aufgenommen:

- Logging der Probandenbewegung im Level
- Zeit zum Abschließen der Level
- Zeit zum Abschließen der Rätsel
- Anzahl der gefundenen Collectibles mit Position
- Anzahl der Tode pro Level mit Position

### Versuchsdurchführung und -ablauf ###

Beide Navigationsmethoden werden mit dem within Subject-Design evaluiert.
Die Probanden absolvieren jeweils ein Level mit der Basisminimap oder der erweiterten Minimap.
Die Zuordnung, welcher Proband mit welcher Minimap beginnt, erfolgt dabei randomisiert.
Pro Versuchsablauf werden 45 Minuten angesetzt, wobei 25 Minuten für den Fragebogen eingeplant sind.
Durch das Andauern der Corona-Pandemie müssen die Versuche im häuslichen Umfeld
sowie online im Bekanntenkreis der Entwickler durchgeführt werden.
Der Ablauf eines Versuchs erfolgt immer in folgender Reihenfolge:

1. Einführung geben
    - Spiel bereitstellen
    - Link zur Umfrage schicken
2. Gesundheitszustand abfragen
    - Ausschluss von Schwangeren oder Epilepsieerkrankten
    - Abbruch des Versuchs bei Schwindel, Kopfschmerzen etc.
3. Einverständniserklärung einholen
4. Fragebogen zum Bartletyp ausfüllen
5. Spielen des ersten Levels
    - Hier wird die erste Variation der Minimap evaluiert
6. Ausfüllen des Minimap-Fragebogens
7. Spielen des zweiten Levels
    - Hier wird die zweite Variation der Minimap evaluiert
8. Ausfüllen des Minimap-Fragebogens
9. Spielen des Boss-Levels
10. Ausfüllen des allgemeinen Fragebogens


## Ergebnisse ##

Nach der Vorstellung des Aufbaus folgen die Ergebnisse der Evaluierung.
Zunächst werden die demografischen Daten der Probanden dargestellt.
Im Anschluss werden die vorher aufgestellten Forschungsfragen bzw. Hypothesen evaluiert.
Abschließend folgen die allgemeinen Ergebnisse über das Spiel.

### Probanden ###

Insgesamt haben 41 Personen teilgenommen. Eine Person musste die Evaluierung wegen Schwindel und Übelkeit abbrechen.
In folgender Abbildung ist die Altersverteilung der Probanden dargestellt.

![Alter der Probanden](./pics/evaluation/age_subjects.png){#fig:subjects_age}

| Mittelwert | Median|
|--------------|--------|
|  24,25   | 24 |

Das Alter der Probanden liegt zwischen 18 und 37 Jahren.
Das Durchschnittsalter liegt bei 24,25 und der Median bei 24 Jahren.
Die Verteilung der Geschlechter der Probanden ist in folgender Abbildung dargestellt.

![Geschlecht der Probanden](./pics/evaluation/subjects_sex.png){#fig:subjects_sex}

28 (70%) Probanden sind männlich und 12 (30%) weiblich.
Niemand hat divers angegeben oder keine Angabe gemacht.
Die aktuelle berufliche Situation der Probanden ist in folgender Abbildung dargestellt.

![Berufliche Situation der Probanden](./pics/evaluation/subjects_job.png){#fig:subjects_job}

25 (62,5%) der Probanden geben als aktuelle berufliche Situation Student und 15 (37,5%) berufstätig an.
Wie oft die Probanden Videospiele spielen ist in folgender Abbildung dargestellt.

![Spielerfahrung der Probanden](./pics/evaluation/subjects_playtime.png)

16 (40%) Probanden gaben an täglich und sechs (15%) mehrmals pro Woche Videospiele zu spielen.
Jeweils neun (22,5%) gaben an mehrmals im Monat, sowie weniger als einmal pro Monat Videospiele zu spielen.
Mit dem ersten Fragebogen wird der Bartletyp der Probanden festgestellt.
Die Verteilung der Bartletypen der Probanden ist in folgender Abbildung dargestellt.

![Ausprägungen der Bartletypen der Probanden](./pics/evaluation/subjects_preplayertype.png)

Nach dem Bartletest ergibt sich eine Aufteilung in sieben (17,5%) Socializer,
neun (22,5%) Achiever, 22 (55%) Explorer und zwei Killer (5%).
Da im Rahmen der Evaluierung die Ausprägungen Achiever und Explorer untersucht werden sollen,
werden die Socializer und Killer entsprechend ihrer stärksten Ausprägung zu den
Achievern oder Explorern gezählt. Die daraus resultierende Verteilung ist in folgender Abbildung dargestellt.

![Finale Bartletypen der Probanden](./pics/evaluation/subjects_finalplayertype.png){#fig:age_subjects}

Damit ergibt sich eine Aufteilung in 13 (32,5%) Achiever und 27 (67,5%) Explorer.
Mit dieser Aufteilung werden die folgenden Forschungsfragen bzw. Hypothesen evaluiert.

### Forschungsfrage 1 Hypothese 1 ###

Die erste Forschungsfrage (F1) untersucht welchen Einfluss Navigationshilfen
auf die Erkundung in einem 3D Level haben. Die erste Hypothese (H1) nimmt an,
dass Probanden mit der Basisminimap mehr Collectibles finden, als mit der erweiterten Minimap.
Die gefundenen Collectibles in Abhängigkeit der Minimap sind in folgender Abbildung dargestellt.

![Gesammelte Collectibles nach Minimapart](./pics/evaluation/all_collectibles.png){#fig:all_collectibles}

In jedem Level können 6 Collectibles gefunden werden. Die Mittelwerte bzw. Mediane der
gesammelten Collectibles in Abhängigkeit der verwendete Minimap sind in folgender Tabelle dargestellt.

| Basisminimap |        | Erweiterte Minimap |        |
|--------------|--------|--------------------|--------|
| Mittelwert   | Median | Mittelwert         | Median |
| 4,55         | 5      | 3,12               | 3,5    |

Bei den Durchläufen mit der Basisminimap beträgt der Mittelwert der gesammelten
Collectibles 4,55 und der Median 5. Bei der erweiterten Minimap beträgt der Mittelwert
der gesammelten Collectibles 3,12 und der Median 3,5. Es zeigt sich eine Tendenz,
dass mit der Basisminimap mehr Collectibles gefunden werden, als mit der erweiterten
Minimap. Des Weiteren lassen die Messwerte die Vermutung zu, dass die Probanden
insbesondere bei der Basisminimap Collectibles gesucht haben. Durch die Ermittlung
des Bartletyps lassen sich die gesammelten Collectibles in Abhängigkeit des Bartletyps untersuchen.

![](./pics/evaluation/collectibles_b_achiever.png){width=50%}
![](./pics/evaluation/collectibles_b_explorer.png){width=50%}

\begin{figure}[!h]
\caption{Gesammelte Collectibles in Abhängigkeit der Minimap und Bartletyps}
\end{figure}


|          | Basisminimap |         | Erweiterte Minimap |        |
|----------|--------------|---------|--------------------|--------|
|          | Mittelwert   | Median  | Mittelwert         | Median |
| Achiever | 5,23         | 6       | 3,85               | 5      |
|          | Mittelwert   | Median  | Mittelwert         | Median |
| Explorer | 4,22         | 5       | 2,78               | 2      |

Der Mittelwert und Median bei der Basisminimap liegt bei den Achievern bei 5,23
bzw. 6 Collectibles. Bei der erweiterten Minimap liegen diese Werte bei 3,85 bzw.
5 Collectibles. Bei den Explorern beträgt die Anzahl der Collectibles 4,22 bzw. 5 bei
der Basisminimap und 2,78 bzw. 2 bei der erweiterten Minimap. Auch bei der Unterteilung
nach Achiever und Explorer zeigt sich die Tendenz, dass mit der Basisminimap mehr
Collectibles gefunden werden, als mit der erweiterten Minimap. Des Weiteren zeigen die
Messwerte Tendenzen, dass die Collectibles insbesondere von den Achievern gesucht wurden.

### Forschungsfrage 1 Hypothese 2 ###

Die zweite Hypothese (H2) nimmt an, dass Probanden mit der erweiterten Minimap
das Level schneller abschließen, als mit der Basisminimap. Die Dauer, die Probanden für das Absolvieren eines Levels benötigen,
wird vom Spiel gemessen. Die Spieldauer in Abhängigkeit der Minimap ist in folgender
Abbildung dargestellt.

![Spielzeit nach Minimapart](./pics/evaluation/playtime_b.png){#fig:all_playtime}

Der Mittelwert sowie Median für die gemessenen Spielzeiten sind in folgender Tabelle dargestellt.

| Basisminimap |        | Erweiterte Minimap |        |
|--------------|--------|--------------------|--------|
| Mittelwert   | Median | Mittelwert         | Median |
| 387,06       | 463,10 | 303,67             | 358,97 |

Der Mittelwert und Median beträgt für die Basisminimap 387,06 bzw 463,10 Sekunden.
Bei der erweiterten Minimap betragen diese Werte 303,67 bzw 358,97 Sekunden.
Es zeigt sich eine leichte Tedenz, dass Probanden mit der erweiterten Minimap das
Level schneller abschließen.

Wie bei der Betrachtung der Collectibles lässt sich die gemessene Spielzeit auch
im Hinblick auf den Bartletypen evaluieren.

![](./pics/evaluation/playtime_b_achiever.png){width=50%}
![](./pics/evaluation/playtime_b_explorer.png){width=50%}

\begin{figure}[!h]
\caption{Spielzeit in Abhängigkeit der Minimap und Bartletyps}
\end{figure}

|          | Basisminimap |         | Erweiterte Minimap |        |
|----------|--------------|---------|--------------------|--------|
|          | Mittelwert   | Median  | Mittelwert         | Median |
| Achiever | 432,24       | 368,00  | 376,15             | 325,22 |
|          | Mittelwert   | Median  | Mittelwert         | Median |
| Explorer | 477,95       | 390,30  | 350,70             | 292,57 |


Der Mittelwert und Median betragen für die Basisminimap bei den Achievern
432,24 bzw. 368,00 Sekunden. Bei der erweiterten Minimap betragen diese Werte
376,15 bzw. 325,22 Sekunden. Bei den Explorern betragen diese Werte 477,95 bzw
390,30 Sekunden bei der Basisminimap und 350,70 bzw. 292,57 Sekunden bei der erweiterten Minimap.
Auch bei der Unterteilung nach Achiever und Explorer zeigt sich die Tendenz,
dass mit der erweiterten Minimap das Level schneller abgeschlossen wird, als mit der Basisminimap.

### Forschungsfrage 2 Hypothese 3 ###

Die zweite Forschungsfrage (F2) untersucht welchen Einfluss Navigationshilfen auf
die Orientierung in einem 3D Level haben. Die dritte Hypothese (H3) nimmt an,
dass mit der erweiterten Minimap die Orientierung von den Probandenn als einfacher
wahrgenommen wird, als mit der Basisminimap. Das Empfinden der Probanden bezüglich
der Orientierung in Abhängigkeit der Minimap ist in folgender Abbildung dargestellt.

![Die Minimap hat mir bei der Ortientierung geholfen](./pics/evaluation/orientation_b_ae.png){#fig:ortientation_b_ae}

| Basisminimap |        | Erweiterte Minimap |        |
|--------------|--------|--------------------|--------|
| Mittelwert   | Median | Mittelwert         | Median |
| 1,38         | 1,50   | 1,52               | 2,00   |

Der Mittelwert beträgt bei der Basisminimap 1,38 und 1,52 bei der erweiterten Minimap.
Der Median beträgt bei der Basisminimap 1,5 und bei der erweiterten MInimap 2,0.
Es liegt keine klare Tendenz vor, mit welcher Minimap die Orientierung als einfacher
empfunden wird. Damit findet sich keine Tendenz zur Bestätigung der Hypothese.
Es zeigt aber die Tendenz auf, dass beide Minimaparten für die Orientierung im Spiel geeignet sind.
Die empfundene Hilfe bei der Orientierung ist in folgender Abbildung nach Bartletyp
dargestellt. Wie bei der Betrachtung der Collectibles lässt sich die gemessene
Spielzeit auch in Hinblick auf den Bartletypen evaluieren.

![](./pics/evaluation/orientation_b_a.png){width=50%}
![](./pics/evaluation/orientation_b_e.png){width=50%}

\begin{figure}[!h]
\caption{Empfundene Hilfe bei der Orientierung Abhängigkeit der Minimap und Bartletyps}
\end{figure}

|          | Basisminimap |         | Erweiterte Minimap |        |
|----------|--------------|---------|--------------------|--------|
|          | Mittelwert   | Median  | Mittelwert         | Median |
| Achiever | 1,54         | 2       | 1,31               | 2      |
|          | Mittelwert   | Median  | Mittelwert         | Median |
| Explorer | 1,30         | 1       | 1,63               | 2      |

Bei der Aufteilung nach den Bartletypen lässt sich die Tendenz festellen,
dass beide Probandengruppen sich mit beiden Minimaparten orientieren können.
Nur bei den Explorern mit Basisminimap fällt auf, dass die Tendenz beim Median
um einen Punkt geringer ausfällt als bei den Achievern.

### Forschungsfrage 3 Hypothese 4 und 5 ###

Die dritte Forschungsfrage (F3) untersucht, welche Navigationshilfe von den Nutzern
bevorzugt wird. Die vierte Hypothese nimmt an, dass Probanden mit Achiever-Ausprägung
die ausgebaute Minimap bevorzugen. Die fünfte Hypothese (H5) nimmt an, dass Probanden
mit Explorer-Ausprägung die Basisminimap bevorzugen. Die Präferenzen der Probanden
sind in folgender Abbildung dargestellt.

![](./pics/evaluation/prefer_b.png){width=50%}
![](./pics/evaluation/fun_b.png){width=50%}
\begin{figure}[!h]
\caption{Minimap-Präferenzen der Probanden}
\end{figure}

26 Probanden (65%) gaben an, dass sie die Basisminimap bevorzugen, während 14 Probanden
(35%) die erweiterte Minimap bevorzugen. 27 Probanden (67,5%) gaben an, dass sie
mit der Basisminimap und 13 Probanden  mit der erweiterten Minimap mehr Spaß hatten.
Die Ergebnisse zeigen Tendenzen, dass die Mehrheit der Probanden die Basisminimap
bevorzugt. Eine Aufteilung nach Spielertyp ist in folgender Abbildung dargestellt.

![](./pics/evaluation/prefer_b_ae.png){width=50%}
![](./pics/evaluation/fun_b_ae.png){width=50%}
\begin{figure}[!h]
\caption{Minimap-Präferenzen der Probanden nach Spielertyp}
\end{figure}

Bei der Aufteilung nach Spielertyp zeigt sich eine Tendenz, dass beide Spielertypen
die Basisminimap bevorzugen. Es gibt eine leichte Tendenz, dass die Achiever die
Basisminimap stärker bevorzugen als die Explorer. Damit liegt keine Tendenz zur
Bestätigung der Hypothese H4 und Hypothese H5 vor.

### Allgemeines zum Spiel ###

Neben den Fragen zur Evaluierung der Hypothesen werden die Probanden zu allgemeinen
Aspekten des Spiels befragt. Zunächst sollten die Probanden angeben, wie anspruchsvoll sie das Spiel empfanden.
Die Ergebnisse sind in folgender Abbildung dargestellt.

![Das Spiel war anspruchsvoll](./pics/evaluation/gamehard_b.png){#fig:gamehard}

|          |  |         |
|----------|-----------------------------|---------|
|          | Mittelwert                  | Median  |
| Achiever | -0,31                       | 0       |
|          | Mittelwert                  | Median  |
| Explorer | 0,41                        | 1       |

Es lässt sich eine leichte Tendenz erkennen, dass Explorer das Spiel als eher
anspruchsvoll bewerten. Bei den Achievern hingegen gibt es eine leichte Tendenz,
dass das Spiel als eher nicht anspruchsvoll bewertet wird. Als nächstes wurden
die Probanden gefragt, ob es Momente gab, in denen sie aufgeben wollten.
Die Ergebnisse sind in folgender Abbildung dargestellt.

![Es gab Momente in denen aufgeben wollte](./pics/evaluation/all_giveup_ae.png){#fig:wingame1}

|          ||         |
|----------|-----------------------------|---------|
|          | Mittelwert                  | Median  |
| Achiever | -1                          | -2      |
|          | Mittelwert                  | Median  |
| Explorer | -1,15                       | -2      |

Bei beiden Spielergruppen gibt es eine Tendenz dazu, dass es keine Momente gab,
in denen sie aufgeben wollten. Dieses Ergebnis deckt sich mit den Antworten zu
der Frage "Ich wollte das Spiel unbedingt absolviert".

![Ich wollte das Spiel unbedingt erfolgreich absolvieren](./pics/evaluation/all_wingame_ae.png){#fig:wingame2}

|          | |         |
|----------|-----------------------------|---------|
|          | Mittelwert                  | Median  |
| Achiever | 1,46                        | 2       |
|          | Mittelwert                  | Median  |
| Explorer | 1,37                        | 2       |

Diese Ergebnisse zeigen Tendenzen, dass die Achiever und Explorer gleichermaßen
motiviert waren, das Spiel abzuschließen und nicht aufzugeben. Diese Tendenzen
spiegeln sich auch im den Anspruch wieder, das Spiel mit möglichst wenig Toden zu beenden.

![Ich wollte das Spiel mit möglichst wenig Toden beenden](./pics/evaluation/all_lowdeaths_ae.png){#fig:playdeaths}

|          ||         |
|----------|-----------------------------|---------|
|          | Mittelwert                  | Median  |
| Achiever | 0,59                        | 1       |
|          | Mittelwert                  | Median  |
| Explorer | 0,62                        | 1       |

Dieselben Tendenzen zeigen sich auch beim Spielspaß, der in folgender Abbildung dargestellt ist.

![Mir hat das Spiel Spaß gemacht](./pics/evaluation/all_fun_ae.png){#fig:playfun}

|          ||         |
|----------|-----------------------------|---------|
|          | Mittelwert                  | Median  |
| Achiever | 1,46                        | 2       |
|          | Mittelwert                  | Median  |
| Explorer | 1,41                        | 1       |

Die Ergebnisse zeigen Tendenzen, dass das Spiel den Probanten, unabhängig vom Spielertyp,
Spaß gemacht hat. Dabei gibt es eine leichte Tendenz, dass das Spiel den Achievern
mehr Spaß gemacht hat, da der Median um einen Punkt höher ist. Als nächstes wurden
die Probanden befragt, ob sie das Spiel erneut spielen wollen würden. Die Ergebnisse
sind in folgender Abbildung dargestellt.

![Ich würde das Spiel erneut spielen wollen](./pics/evaluation/all_playagain.png){#fig:playagain}

|          ||         |
|----------|-----------------------------|---------|
|          | Mittelwert                  | Median  |
| Achiever | 0,08                        | 0       |
|          | Mittelwert                  | Median  |
| Explorer | 0,56                        | 1       |

Hier lässt sich eine leichte Tendenz erkennen, dass die Explorer eher gewillt wären,
das Spiel noch mal zu spielen. Neben den Likert-Skala-Fragen werden auch die Freitextfragen
evaluiert. In den Freitextfragen zu Verbesserungsvorschlägen zur Basisminimap wurden folgende Punkte genannt:

- Zoomstufe der Minimap einstellbar machen
- Minimap dreht sich mit dem Spieler
- Ansicht mit dem kompletten Level

In den Freitextfragen zu Verbesserungsvorschlägen zur erweiterten Minimap wurden foglende Punkte genannt:

- Zoomstufe der Minimap einstellbar machen
- Minimap dreht sich mit dem Spieler
- Nur den Zielort und nicht den Pfad anzeigen
- Den Pfad weniger aufdringlich darstellen
- Nicht die komplette Map zu Beginn aufdecken

In den Freitextfragen zu Verbesserungsvorschlägen zum Spiel wurden folgende Punkte genannt:

- Weniger monotone Texturen zur besseren räumlichen Ortientierung
- Tipps in der Snackbar auffälliger gestalten und mit Ton versehen
- Field of View erhöhen oder eine Einstellung dafür bereitstellen
- Icons für Aufsammelbares
- Trefferfeedback deutlicher darstellen
- Texturen die weniger Cybersickness oder Desorientierung auslösen
- Optimierung der Sprungmechanik

# Zusammenfassung und Ausblick #

TBD
Die Messwerte könnten durch die hohe Probandenzahl statistisch analysiert werden. Des weiteren wurden nicht alle Antworten und Messwerte evaluiert. Die Antworten bezüglich der Glücksgefühle beim sammeln von Collectibles oder die geloggten Spielerbewegungen könnten die Basis weiterer Untersuchungen sein. In weiteren Studien könnten die Ausprägung Socializer und Killer untersucht werden. Des Weiteren sollte das Spiel dahingehend angepasst werden, dass Schwindel und Übelkeit während des Spielens verringert wird. Außerdem könnte die Aufteilung der Probanden nach dem Test von Jon Radoff erfolgen, der statt einem Kategorie-Framework ein Komponenten-Framework vorschlägt. Ein weiteres Feld was im Rahmen des Moduls betrachtet werden könnte, wäre das Cheaten im Singeplayer-Kontext.

# Lessons Learned #

TBD

Licht in Räumen
Virendesign
Allgemein
Siehe PPP

# Literaturverzeichnis und Quellen #

TBD
