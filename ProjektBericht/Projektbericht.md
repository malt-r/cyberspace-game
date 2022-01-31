---
title: "Projektbericht Cyberspace"
author: [Malte Reinsch (1111598), Tim Lücking (1109744), Dennis Eller (1109421)]
date: "02.02.2022"
lang: "de"
titlepage: true
toc-own-page: true

---

\pagenumbering{gobble}
\pagebreak
\listoffigures
\pagenumbering{arabic}

# Einleitung #

# Konzept und Umsetzung #

## Spielidee ##

## Meilenstiene ##

## Arbeitspakete und Zuständigkeiten ##

## Umsetzung ##

### Framework ###

### GUI unnd Menüs ###

### Generator ###

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

TODO: Gitterbild

Basierend auf den Dimensionen der Räume in einer Partition berechnet der Generator einen
Bereich auf dem Gitter, in dem die Räume mit zufälliger Position und Rotation platziert werden können.
Stellt der Generator eine Kollision der Räume fest, wird dieser Prozess erneut gestartet,
bis eine gültige Raumanordnung gefunden wird.

TODO: Platierungsbild

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

TODO: Delauneybild

Um die Verbindungen zwischen den Türen zu reduzieren wird ein minimaler Spannbaum auf
der Delauney-Triangulation konstruiert. Das Ergebnis ist im untenstehenden Bild zu erkennen.

TODO: MSTBild

Mit dem minimalen Spannbaum sind die finalen Verbindungen zwischen den Türen bekannt.
Für jede dieser Verbindungen muss anschließend ein Pfad im Gitter gefunden werden, sodass
die Korridore zwischen den Räumen platziert werden können. Hierfür wird der A\*-Algorithmus
verwendet. Die Kostenfunktion wird dabei so gewählt, dass der Generator keine Pfade
durch andere Räume oder Pfade einer anderen Partition wählt. Das Ergebnis dieser
Phase ist im untenstehenden Bild zu sehen.

TODO: PFadbild

Entlang der so definierten Pfade werden pro Zelle Korridormodule eingesetzt, um einen
zusammenhängenden Korridor zu erzeugen. Hierfür muss der Korridortyp für jede Zelle
ermittelt werden (gerades Stück, Kurve, T-Kreuzung) und die Rotation, mit der das
entsprechende Korridormodul eingesetzt werden muss. Die fertig konstruierten Korridore
sind in der untenstehenden Abbildung zu erkennen.

TODO: Korridorbild

### Story ###

### Räume ###

### Probanden ###

### Gegner ###

### Kampfsystem ###

### Aufsammelbares ###

### Bosskampf ###

## Spielanleitung ##

# Evaluierung #

Nach der Umsetzung wird das Spiel mithilfe von Probanden evaluiert. Dazu wird als Erstes der Aufbau der Evaluierung vorgestellt. Im Anschluss folgen die Ergebnisse.

## Aufbau ##

Zunächst werden die aufgestellten Forschungsfragen und Hypothesen vorgestellt. Im Anschluss folgen die Fragebögen, mit denen die Forschungsfragen evaluiert werden sollen. Daten die über Fragebögen nicht zuverlässig erhoben werden können, werden von dem Spiel selbständig aufgenommen. Zum Schluss wird die Versuchsdurchführung- und ablauf dargestellt.

### Forschungsfragen und Hypothesen ###

Für die Evaluierung wird die Art der Minimap als Variable ausgewählt. In der Evaluierung sollen zwei Arten von Minimaps evaluiert werden. Beide Minimaps zeigen das Level in Vogelperspektive. Beide Minimaps sind in folgender Abbildung dargestellt.

![](./pics/basic_minimap.png){width=50%}
![](./pics/extended_minimap.png){width=50%}
\begin{figure}[!h]
\caption{Zu untersuchende Minimaparten}
\end{figure}
Die Basisminimap wird für einen Raum aufgedeckt, sobald der Proband einen Raum betritt. Angrenzende Räume werden durch eine transparente Schattierung angezeigt. Bei der erweiterten Minimap wird der Weg zur nächsten zu lösenden Storyaufgabe eingezeichnet. Des Weiteren ist sie ab Beginn komplett aufgedeckt. Anhand der zu untersuchenden Variable wurden folgende Forschungsfragen mit den dazugehörigen Hypothesen formuliert:

- F1: Welchen Einfluss haben Navigationshilfen auf die Erkundung in einem 3D Level?
    - H1: Mit der Basisminimap werden mehr Collectibles gefunden
    - H2: Mit der erweiterten Minimap wird das Level schneller abgeschlossen
- F2: Welchen Einfluss haben Navigationshilfen auf die Orientierung in einem 3D Level?
    - H3: Mit der erweiterten Minimap wird die Orientierung im Spiel als einfacher wahrgenommen
- F3: Welche Navigationshilfe wird von den Nutzern bevorzugt?
    - H4: Probanden mit stärkerer “Achiever”-Ausprägung bevorzugen die ausgebaute Minimap
    - H5: Probanden mit stärkerer “Explorer”-Ausprägung bevorzugen die Basisminimap

### Fragebögen ###

Zur Evaluieren der Hypothesen und Einholen von generellem Feedback werden folgende Fragebögen verwendet:

- Bartletest
- Minimapfragebogen
- Fragebogen über Demographie und allgemeiner Fragebogen

Es werden sowohl Likert-Skala-Fragen, als auch Freitextfragen gestellt. Bei den Likert-Skala-Fragen wird eine 5er Skala von -2 bis 2 (Stimme garnicht zu bis Stimme voll zu) verwendet.

**Bartletest**

Der Bartletest klassifiziert die Probanden auf Grundlage von einer Reihe von Fragen in vier Typen ein. Die Fragen dienen der Bestimmung der Spielinteressen. Der Test basiert auf einem Paper von Richard Bartle und wurde ursprünglich für Multiplayer-Spiele entworfen, wird jedoch mittlerweile auch für Singleplayer-Spiele verwendet.

**Minimap**

Zur Evaluierung der Forschungsfragen bezüglich der Minimaps wird ein Fragebogen entworfen, der sich am IEQ (Immerssive Experience Questionaire) orientiert. Dieser besteht aus einer Reihen von Fragen, die einer 5-Likert-Skala von "Stimme garnicht zu" bis "Stimme voll zu" folgen. Dieser Fragebogen wird jeweils nach jedem Level von den Probanden ausgefüllt.

**Demographie und Allgemein**

Im demografischen Fragebogen wird das Alter, Geschlecht, der aktuelle Berufsstatus und Spielerfahrung erfragt, um Informationen über die Probanden zu erhalten.
Des Weiteren wird allgemeines Feedback über das Spiel im allgemeinen Fragebogen gesammelt. Dieser besteht auch aus einer Reihen von Fragen, die einer 5-Likert-Skala von "Stimme garnicht zu" bis "Stimme voll zu" folgen. Neben den Likert-Skala-Fragen werden über Freitextfragen Anregungen zu den Minimaps gesammelt.

### Messdaten ###

Neben der Evaluierung über Fragebögen werden folgende Daten während des Spielens gemessen:

- Logging der Probandenbewegung
- Zeit zum Abschließen der Level
- Zeit zum Abschließen der Rätsel
- Anzahl der gefundenen Collectibles mit Position
- Anzahl der Tode pro Level mit Position

### Versuchsdurchführung und -ablauf ###

Beide Navigationsmethoden werden mit dem within Subject-Design evaluiert. Die Probanden absolvieren jeweils ein Level mit der Basis- oder der ausgebauten -Minimap. Die Zuordnung, welcher Proband mit welcher Minimap beginnt, wird dabei randomisiert zugewiesen. Pro Versuchsablauf werden 45 Minuten angesetzt, wobei 25 Minuten für den Fragebogen eingeplant ist. Durch das Andauern der Corona-Pandemie müssen die Versuche im häuslichen Umfeld sowie online im Bekanntenkreis der Entwickler durchgeführt werden. Der Ablauf eines Versuchs erfolgt immer in folgender Reihenfolge:

1. Einführung geben
    - Spiel bereitstellen
    - Link zur Umfrage schicken
1. Gesundheitszustand abfragen
    - Ausschluss von Schwangeren oder Epilepsieerkrankten
    - Abbruch des Versuchs bei Schwindel, Kopfschmerzen etc.
1. Einverständniserklärung einholen
2. Fragebogen zum Bartletyp ausfüllen
3. Spielen des ersten Levels
    - Hier wird die erste Variation der Minimap evaluiert
4. Ausfüllen des Minimap-Fragebogens
5. Spielen des zweiten Levels
    - Hier wird die zweite Variation der Minimap evaluiert
6. Ausfüllen des Minimap-Fragebogens
7. Spielen des Boss-Levels
8. Ausfüllen des Allgemeinen Fragebogens


## Ergebnisse ##

Nach der Vorstellung des Aufbaus folgen die Ergebnisse der Evaluierung. Zunächst werden die Ergebnisse über die Probanden dargestellt. Im Anschluss werden die vorher aufgestellten Forschungsfragen bzw. Hypothesen evalauiert. Zum Schluss folgen die allgemeinen Ergebnisse über das Spiel.

### Probanden ###

Insgesamt haben 41 Personen teilgenommen. Eine Person musste die Evaluierung wegen Schwindel und Übelkeit abbrechen. In folgender Abbildung ist die Altersverteilung der Probanden dargestellt.

![Alter der Probanden](./pics/evaluation/age_subjects.png){#fig:subjects_age}

| Mittelwert | Median|
|--------------|--------|
|  24,25   | 24 |

Das Alter der Probanden liegt zwischen 18 und 37 Jahren. Das Durchschnittsalter liegt bei 24,25 und der Median bei 24 Jahren. Die Verteilung der Geschlechter der Probanden ist in folgender Abbildung dargestellt.

![Geschlecht der Probanden](./pics/evaluation/subjects_sex.png){#fig:subjects_sex}

28 (70%) Probanden sind männlich und 12 (30%) weiblich. Niemand hat divers angegeben oder keine Angabe gemacht. Die aktuelle berufliche Situation der Probanden ist in folgender Abbildung dargestellt.

![Berufliche Situation der Probanden](./pics/evaluation/subjects_job.png){#fig:subjects_job}

25 (62,5%) der Probanten geben als aktuelle berufliche Situation Student und 15 (37,5%) berufstätig an. Wie oft die Probanden Videospiele spielen ist in folgender Abbildung dargestellt.

![Probandenfahrung der Probanden](./pics/evaluation/subjects_playtime.png)

16 (40%) Probanden gaben an täglich und sechs (15%) mehrmals pro Woche Videospiele zu spielen. Jeweils neun (22,5%) gaben an mehrmals im Monat sowie weniger als einmal pro Monat zu Videospiele zu spielen. Mit dem ersten Fragebogen wird der Bartletyp der Probanden festgestellt. Die Verteilung der Bartletypen der Probanden ist in folgender Abbildung dargestellt.

![Ausprägungen der Bartletypen der Probanden](./pics/evaluation/subjects_preplayertype.png)

Nach dem Bartletest ergibt sich eine Aufteilung in sieben (17,5%) Socializer, neun (22,5%) Achiever, 22 (55%) Explorer und zwei Killer (5%). Da im Rahmen der Evaluierung die Ausprägungen Achiever und Explorer untersucht werden sollen, werden die Socializer und Killer entsprechend ihrer stärksten Ausprägung zu den Achievern oder Explorern gezählt. Die daraus resultierende Verteilung ist in folgender Abbildung dargestellt.

![Finale Bartletypen der Probanden](./pics/evaluation/subjects_finalplayertype.png){#fig:age_subjects}

Damit ergibt sich eine Aufteilung in 13 (32,5%) Achiever und 27 (67,5%) Explorer. Mit dieser Aufteilung werden die folgenden Forschungsfragen bzw. Hypothesen evaluiert.

### Forschungsfrage 1 Hypothese 1 ###

Die erste Forschungsfrage (F1) untersucht welchen Einfluss Navigationshilfen auf die Erkundung in einem 3D Level haben. Die erste Hypothese (H1) nimmt an, dass Probanden mit der Basisminimap mehr Collectibles finden, als mit der erweiterten Minimap. Die gefundenen Collectibles in Abhängigkeit der Minimap sind in folgender Abbildung dargestellt.

![Gesammelte Collectibles nach Minimapart](./pics/evaluation/all_collectibles.png){#fig:all_collectibles}

In jedem Level können 6 Collectibles gefunden werden. Die Mittelwerte bzw. Mediane der gesammelten Collectibles in Abhängigkeit der verwendete Minimap sind in folgender Tabelle dargestellt.

| Basisminimap |        | Erweiterte Minimap |        |
|--------------|--------|--------------------|--------|
| Mittelwert   | Median | Mittelwert         | Median |
| 4,55         | 5      | 3,12               | 3,5    |

Bei den Durchläufen mit der Basisminimap beträgt der Mittelwert der gesammelten Collectibles 4,55 und der Median 5. Bei der erweiterten Minimap beträgt der Mittelwert der gesammelten Collectibles 3,12 und der Median 3,5. Es zeigt sich eine Tendenz, dass mit der Basisminimap mehr Collectibles gefunden werden, als mit der erweiterten Minimap. Des Weiteren lassen die Messwerte die Vermutung zu, dass die Probanden insbesondere bei der Basisminimap Collectibles gesucht haben. Durch die Ermittlung des Bartletyps lassen sich die gesammelten Collectibles in Abhängigkeit des Bartletyps untersuchen.

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

Der Mittelwert und Median bei der Basisminimap liegt bei den Achievern bei 5,23 bzw. 6 Collectibles. Bei der erweiterten Minimap liegen diese Werte bei 3,85 bzw. 5 Collectibles. Bei den Explorern beträgt die Anzahl der Collectibles 4,22 bzw. 5 bei der Basisminimap und 2,78 bzw. 2 bei der erweiterten Minimap. Auch bei der Unterteilung nach Achiever und Explorer zeigt sich die Tendenz, dass mit der Basisminimap mehr Collectibles gefunden werden, als mit der erweiterten Minimap. Des Weiteren zeigen die Messwerte Tendenzen, dass die Collectibles insbesondere von den Achievern gesucht wurden.

### Forschungsfrage 1 Hypothese 2 ###

Die zweite Hypothese (H2) nimmt an, dass Probanden mit der erweiterten Minimap das Level schneller abschließen, als mit der Basisminimap. Die Dauer, die ein Probanden für das Absolvieren eines Levels benötigt, wird vom Spiel gemessen. Die Spieldauer in Abhängigkeit der Minimap ist in folgender Abbildung dargestellt.

![Spielzeit nach Minimapart](./pics/evaluation/playtime_b.png){#fig:all_playtime}

 Der Mittelwert sowie Median für die gemessenen Spielzeiten sind in folgender Tabelle dargestellt.

| Basisminimap |        | Erweiterte Minimap |        |
|--------------|--------|--------------------|--------|
| Mittelwert   | Median | Mittelwert         | Median |
| 387,06       | 463,10 | 303,67             | 358,97 |

Der Mittelwert und Median beträgt für die Basisminimap 387,06 bzw 463,10 Sekunden. Bei der erweiterten Minimap betragen diese Werte 303,67 bzw 358,97 Sekunden. Es zeigt sich eine leichte Tedenz, dass Probanden mit der erweiterten Minimap das Level abschließen.

Wie bei der Betrachtung der Collectibles lässt sich die gemessene Spielzeit auch in Hinblick auf den Bartletypen evaluieren.

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


Der Mittelwert und Median betragen für die Basisminimap bei den Achievern 432,24 bzw. 368,00 Sekunden. Bei der erweiterten Minimap betragen diese Werte 376,15 bzw. 325,22 Sekunden. Bei den Explorern betragen diese Werte 477,95 bzw 390,30 Sekunden bei der Basisminimap und 350,70 bzw. 292,57 Sekunden bei der erweiterten Minimap. Auch bei der Unterteilung nach Achiever und Explorer zeigt sich die Tendenz, dass mit der erweiterten Minimap das Level schneller abgeschlossen wird, als mit der Basisminimap.

### Forschungsfrage 2 Hypothese 3 ###

Die zweite Forschungsfrage (F2) untersucht welchen Einfluss Navigationshilfen auf die Orientierung in einem 3D Level haben. Die dritte Hypothese (H3) nimmt an, dass Probanden mit der erweiterten Minimap die Orientierung von den Probandenn als einfacher wahrgenommen wird, als mit der Basisminimap. Das Empfinden der Probanden bezüglich der Orientierung in Abhängigkeit der Minimap ist in folgender Abbildung dargestellt.

![Die Minimap hat mir bei der Ortientierung geholfen](./pics/evaluation/orientation_b_ae.png){#fig:ortientation_b_ae}

| Basisminimap |        | Erweiterte Minimap |        |
|--------------|--------|--------------------|--------|
| Mittelwert   | Median | Mittelwert         | Median |
| 1,38         | 1,50   | 1,52               | 2,00   |

Der Mittelwert beträgt bei der Basisminimap 1,38 und 1,52 bei der erweiterten Minimap. Der Median beträgt bei der Basisminimap 1,5 und bei der erweiterten MInimap 2,0. Es liegt keine klare Tendenz vor, mit welcher Minimap die Orientierung als einfacher empfunden wird. Damit findet sich keine Tendenz zur Bestätigung der Hypothese. Es zeigt aber die Tendenz auf, dass beide Minimaparten für die Orientierung im Spiel geeeignet sind.
Die empfundene Hilfe bei der Orientierung ist in folgender Abbildung nach Bartletyp dargestellt. Wie bei der Betrachtung der Collectibles lässt sich die gemessene Spielzeit auch in Hinblick auf den Bartletypen evaluieren.

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

Bei der Aufteilung nach den Bartletypen lässt sich die Tendenz festellen, dass beide Probandengruppen sich mit beiden Minimaparten orientieren können. Nur bei den Explorern mit Basisminimap fällt auf, dass die Tendenz beim Median um einen Punkt geringer ausfällt als bei den Achievern.

### Forschungsfrage 3 Hypothese 4 und 5 ###

Die dritte Forschungsfrage (F3) untersucht welche Navigationshilfen von den Nutzern bevorzugt wird. Die vierte Hypothese nimmt an, dass Probanden mit Achiever-Ausprägung die ausgebaute Minimap bevorzugen. Die fünfte Hypothese (H5) nimmt an, dass Probanden mit Explorer-Ausprägung die Basisminimap bevorzugen. Die Präferenzen der Probanden sind in folgender Abbildung dargestellt

![](./pics/evaluation/prefer_b.png){width=50%}
![](./pics/evaluation/fun_b.png){width=50%}
\begin{figure}[!h]
\caption{Minimap-Präferenzen der Probanden}
\end{figure}

26 Probanden (65%) gaben an, dass sie die Basisminimap bevorzugen, während 14 Probanden (35%) die erweiterte Minimap bevorzugen. 27 Probanden (67,5%) gaben an, dass sie mit der Basisminimap und 13 Probanden  mit der erweiterten Minimap mehr Spaß gehabt zu haben. Die Ergebnisse zeigen Tendezen, dass die Mehrheit der Probanden die Basisminimap bevorzugen. Eine Aufteilung nach Spielertyp ist in folgender Abbildung dargestellt.

![](./pics/evaluation/prefer_b_ae.png){width=50%}
![](./pics/evaluation/fun_b_ae.png){width=50%}
\begin{figure}[!h]
\caption{Minimap-Präferenzen der Probanden nach Spielertyp}
\end{figure}

Bei der Aufteilung nach Spielertyp zeigt sich eine Tendenz, dass beide Spielertypen die Basisminimap bevorzugen. Es gibt eine leichte Tendenz, dass die Achiever die Basisminimap stärker bevorzugen als die Explorer. Damit liegt keine Tendenz zur Bestätigung der Hypothese H4 und Hypothese H5 vor.

### Allgemeines zum Spiel ###

Neben den Fragen zur Evaluierung der Hypothesen werden die Probanden zu allgemeinen Aspekten zum Spiel befragt. Zunächst sollten die Probanden angeben, wie anspruchsvoll sie das Spiel empfanden haben. Die Ergebnisse sind in folgender Abbildung dargestellt.

![Das Spiel war anspruchsvoll](./pics/evaluation/gamehard_b.png){#fig:gamehard}

|          |  |         |
|----------|-----------------------------|---------|
|          | Mittelwert                  | Median  |
| Achiever | -0,31                       | 0       |
|          | Mittelwert                  | Median  |
| Explorer | 0,41                        | 1       |

Es lässt sich eine leichte Tendenz erkennen, dass Explorer das Spiel als eher anspruchsvoll bewerten. Bei den Achievern hingegen gibt es eine leichte Tendenz, dass das Spiel als eher nicht anspruchsvoll bewertet wird. Als nächstes wurden die Probanden gefragt, ob es Momente gab, in denen sie aufgeben wollten. Die Ergebnisse sind in folgender Abbildung dargestellt.

![Es gab Momente in denen aufgeben wollte](./pics/evaluation/all_giveup_ae.png){#fig:wingame}

|          ||         |
|----------|-----------------------------|---------|
|          | Mittelwert                  | Median  |
| Achiever | -1                          | -2      |
|          | Mittelwert                  | Median  |
| Explorer | -1,15                       | -2      |

Bei beiden Spielergruppen gibt es eine Tendenz dazu, dass es keine Momente gab, in denen sie aufgeben wollten. Dieses Ergebnis deckt sich mit den Antworten zu der Frage, ob das Spiel unbedingt absolviert werden wollte.

![Ich wollte das Spiel unbedingt erfolgreich absolvieren](./pics/evaluation/all_wingame_ae.png){#fig:wingame}

|          | |         |
|----------|-----------------------------|---------|
|          | Mittelwert                  | Median  |
| Achiever | 1,46                        | 2       |
|          | Mittelwert                  | Median  |
| Explorer | 1,37                        | 2       |

Diese Ergebnisse zeigen Tendenzen, dass die Achiever und Explorer gleichermaßen motiviert waren das Spiel abzuschließen und nicht aufzugeben. Diese Tendenzen spiegeln sich auch im den Anspruch wieder, das Spiel mit möglichst wenig Toden zu beenden.

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

Die Ergebnisse zeigen Tendenzen, dass das Spiel den Probanten, unabhängig vom Spielertyp, Spaß gemacht hat. Dabei gibt es eine leichte Tendenz, dass das Spiel den Achievern mehr Spaß gehabt hat, da der Median um einen Punkt höher ist. Als nächstes wurden die Probanden befragt, ob sie das Spiel erneut spielen wollen würden. Die Ergebnisse sind in folgender Abbildung dargestellt.

![Ich würde das Spiel erneut spielen wollen](./pics/evaluation/all_playagain.png){#fig:playagain}

|          ||         |
|----------|-----------------------------|---------|
|          | Mittelwert                  | Median  |
| Achiever | 0,08                        | 0       |
|          | Mittelwert                  | Median  |
| Explorer | 0,56                        | 1       |

Hier lässt sich eine leichte Tendenz erkennen, dass die Explorer eher gewillt wären, das Spiel noch mal zu spielen. Neben den Likert-Skala-Fragen werden auch die Freitextfragen evaluiert. In den Freitextfragen zu Verbesserungsvorschlägen zur Basisminimap wurden folgende Punkte genannt:

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
