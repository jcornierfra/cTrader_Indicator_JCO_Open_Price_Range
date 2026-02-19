# JCO - OPR Levels Trading Range DST with RSI EMA SL TP Infos

Indicateur cTrader tout-en-un pour une stratégie intraday basée sur l'**Opening Price Range (OPR)**.
Il combine la détection de range, un RSI filtré, une EMA, et un calcul automatique des niveaux SL/TP avec money management.

> **Version :** 5.1
> **Plateforme :** cTrader
> **GitHub :** https://github.com/jcornierfra/cTrader_Indicator_JCO_Open_Price_Range

---

## Fonctionnalités

### 1. Opening Price Range (OPR)
L'OPR est le range formé pendant une courte fenêtre en début de session (par défaut 09h30–09h35 heure de New York).

- Calcul du **High** et du **Low** de la période OPR à partir d'un timeframe configurable (défaut : M1)
- **Mise à jour en temps réel** des lignes OPR pendant la période active
- Les lignes OPR s'étendent jusqu'à l'heure de fin de trading (*Trading Stop*)
- Affichage sur les 30 derniers jours de trading (jours de semaine uniquement)
- Couleurs et style de ligne configurables séparément pour le High et le Low

### 2. Lignes verticales de session
Cinq lignes verticales colorées délimitent les phases clés de la journée :

| Ligne | Défaut (NY) | Couleur par défaut |
|---|---|---|
| OPR Start | 09h30 | Rouge |
| OPR Stop | 09h35 | Rouge |
| Trading Start | 10h30 | Vert |
| Trading Stop | 13h30 | Jaune |
| Trading Close | 16h00 | Orange |

Chaque heure, minute et couleur est configurable indépendamment.

### 3. Support DST (Daylight Saving Time)
L'indicateur gère automatiquement les changements d'heure grâce à la conversion de fuseau horaire Windows.
Fuseaux préconfigurés dans la description du paramètre :

| Place | Timezone ID | UTC |
|---|---|---|
| New York | `Eastern Standard Time` | UTC-5 / UTC-4 |
| Londres | `GMT Standard Time` | UTC+0 / UTC+1 |
| Paris | `Romance Standard Time` | UTC+1 / UTC+2 |
| Tokyo | `Tokyo Standard Time` | UTC+9 (pas de DST) |
| Sydney | `AUS Eastern Standard Time` | UTC+10 / UTC+11 |
| Francfort | `W. Europe Standard Time` | UTC+1 / UTC+2 |

### 4. RSI avec signaux filtrés
Le RSI (période 14 par défaut) génère des signaux visuels uniquement dans des conditions précises :

- **Signal standard** (point Lime / Rouge) : RSI au-dessus du niveau de surachat ou en dessous du niveau de survente, dans la fenêtre de trading
- **Premier signal du jour** (point Jaune / Orange, plus grand) : premier dépassement **ET** prix en dehors du range OPR
  - Surachat + clôture > OPR High → signal de **vente** potentielle
  - Survente + clôture < OPR Low → signal d'**achat** potentiel

Les signaux premiers sont mis en évidence car ils correspondent à la logique de cassure de range.

### 5. EMA (Exponential Moving Average)
- EMA calculée en interne (sans dépendance externe)
- Algorithme : SMA sur la période d'initialisation, puis lissage exponentiel standard
- Affichée en bleu ciel (`DeepSkyBlue`) en overlay sur le graphique
- Période configurable (défaut : 50)

### 6. Niveaux SL/TP automatiques
Au déclenchement du **premier signal du jour**, l'indicateur calcule automatiquement les niveaux SL et TP :

**Stop Loss :**
- Recherche le swing point le plus récent dans les 20 dernières bougies
  - Signal de vente → swing **Low** le plus récent
  - Signal d'achat → swing **High** le plus récent
- Application d'une **marge de sécurité** configurable en pips
- Si le swing dépasse le range OPR, le SL est plafonné au niveau OPR correspondant

**Take Profit :**
- Calculé à partir du prix d'entrée (clôture du signal) et de la distance SL
- `TP = Entrée ± (Distance SL × R:R Ratio)`
- R:R Ratio configurable (défaut : 1.5)

Les lignes SL (rouge) et TP (vert) sont tracées sur le graphique avec un commentaire indiquant les valeurs.

### 7. Panneau Money Management
Affiché en bas à gauche du graphique lors du déclenchement d'un signal, il indique :

- Prix d'entrée, SL et TP
- Distance SL en pips et en ticks
- Tick Size et Tick Value
- Risque en devise par lot standard
- **Deux configurations capital/risque indépendantes** :
  - Taille de lot calculée automatiquement
  - Gain potentiel au TP

**Modes d'affichage** (paramètre *Affichage infos trade*) :
- `0` : Toujours affiché (dès qu'un signal existe)
- `1` : Uniquement pendant les heures de trading
- `2` : Désactivé

### 8. Contrôle d'affichage par timeframe
Un paramètre *Timeframe max (minutes)* empêche l'affichage des lignes sur des timeframes trop élevées.
Exemple : valeur `60` → les lignes ne s'affichent pas en H2, H4, Daily, etc.
Cela évite l'encombrement du graphique en multi-timeframe.

---

## Paramètres

### Groupe : OPR Start
| Paramètre | Défaut | Description |
|---|---|---|
| Heure | 9 | Heure de début de l'OPR |
| Minutes | 30 | Minutes de début de l'OPR |
| Couleur | Red | Couleur de la ligne verticale OPR Start |

### Groupe : OPR Stop
| Paramètre | Défaut | Description |
|---|---|---|
| Heure | 9 | Heure de fin de l'OPR |
| Minutes | 35 | Minutes de fin de l'OPR |
| Couleur | Red | Couleur de la ligne verticale OPR Stop |

### Groupe : Trading Start
| Paramètre | Défaut | Description |
|---|---|---|
| Heure | 10 | Heure d'ouverture de la fenêtre de trading |
| Minutes | 30 | Minutes |
| Couleur | Green | Couleur de la ligne verticale |

### Groupe : Trading Stop
| Paramètre | Défaut | Description |
|---|---|---|
| Heure | 13 | Heure de fermeture de la fenêtre de trading |
| Minutes | 30 | Minutes |
| Couleur | Yellow | Couleur de la ligne verticale |

### Groupe : Trading Close
| Paramètre | Défaut | Description |
|---|---|---|
| Heure | 16 | Heure de clôture des positions |
| Minutes | 0 | Minutes |
| Couleur | Orange | Couleur de la ligne verticale |

### Groupe : OPR Lines
| Paramètre | Défaut | Description |
|---|---|---|
| Afficher lignes OPR | true | Active/désactive les lignes High/Low OPR |
| Timeframe OPR | m1 | Timeframe source pour le calcul OPR (`m1`, `m5`, `h1`, etc.) |
| Couleur High OPR | White | Couleur de la ligne OPR High |
| Couleur Low OPR | White | Couleur de la ligne OPR Low |
| Style lignes OPR | 2 | Style de ligne (0=Solid, 1=Dots, 2=Dashes, etc.) |
| Durée affichage OPR (heures) | 8 | Durée d'extension des lignes OPR (non utilisé en v5.1) |

### Groupe : EMA
| Paramètre | Défaut | Description |
|---|---|---|
| Afficher EMA | true | Active/désactive l'EMA |
| Période EMA | 50 | Nombre de bougies pour le calcul de l'EMA |

### Groupe : RSI
| Paramètre | Défaut | Description |
|---|---|---|
| Activer RSI | true | Active/désactive le RSI et ses signaux |
| RSI Period | 14 | Période du RSI |
| Overbought Level | 70 | Niveau de surachat |
| Oversold Level | 30 | Niveau de survente |
| Signal Distance | 5 | Distance en pips entre le signal et le prix |
| Signal Size | 5 | Taille des points de signal |

### Groupe : General
| Paramètre | Défaut | Description |
|---|---|---|
| Fuseau Horaire | Eastern Standard Time | ID Windows du fuseau horaire de référence |
| Epaisseur des lignes | 2 | Épaisseur des lignes verticales et SL/TP |
| Style des lignes | 2 | Style des lignes verticales |
| Afficher uniquement aujourd'hui | false | Si true, n'affiche les lignes que pour la journée courante |
| Afficher lignes verticales | true | Active/désactive les 5 lignes verticales de session |
| Affichage infos trade | 0 | Mode d'affichage du panneau money management (0/1/2) |
| Timeframe max (minutes) | 60 | Timeframe maximale d'affichage (en minutes) |

### Groupe : Money Management
| Paramètre | Défaut | Description |
|---|---|---|
| Afficher lignes SL/TP | true | Active/désactive le tracé des lignes SL et TP |
| Risk:Reward Ratio | 1.5 | Ratio risque/récompense pour le calcul du TP |
| Marge SL (pips) | 2 | Marge de sécurité ajoutée au swing point pour le SL |
| Capital 1 | 10 000 | Premier capital de référence |
| Risk 1 (%) | 1.0 | Risque en % du Capital 1 |
| Capital 2 | 150 000 | Deuxième capital de référence |
| Risk 2 (%) | 0.5 | Risque en % du Capital 2 |

---

## Sorties (Outputs)

| Sortie | Couleur | Type | Description |
|---|---|---|---|
| EMA | DeepSkyBlue | Ligne | Moyenne mobile exponentielle |
| Overbought Signal | Lime | Points (taille 5) | RSI en zone de surachat (dans fenêtre de trading) |
| Oversold Signal | Red | Points (taille 5) | RSI en zone de survente (dans fenêtre de trading) |
| First Overbought Signal | Yellow | Points (taille 7) | Premier signal surachat + cassure OPR High |
| First Oversold Signal | Orange | Points (taille 7) | Premier signal survente + cassure OPR Low |

---

## Logique de signal (résumé)

```
OPR calculé entre OPR Start et OPR Stop
         │
         ▼
   Fenêtre de trading (Trading Start → Trading Stop)
         │
         ├─ RSI > Overbought ET Close > OPR High → Premier signal VENTE (jaune)
         │         → SL au swing Low récent + marge
         │         → TP = Entrée + (SL distance × R:R)
         │
         └─ RSI < Oversold ET Close < OPR Low → Premier signal ACHAT (orange)
                   → SL au swing High récent + marge
                   → TP = Entrée - (SL distance × R:R)
```

Un seul signal premier par jour est pris en compte (`firstSignalProcessedToday`).

---

## Installation

1. Télécharger le fichier `.cs`
2. Dans cTrader, aller dans **Automate → Indicateurs → Importer**
3. Sélectionner le fichier `.cs` et compiler
4. Appliquer l'indicateur sur un graphique intraday (M1 à H1 recommandé)

---

## Licence

Projet open-source — utilisation libre à des fins personnelles et éducatives.
