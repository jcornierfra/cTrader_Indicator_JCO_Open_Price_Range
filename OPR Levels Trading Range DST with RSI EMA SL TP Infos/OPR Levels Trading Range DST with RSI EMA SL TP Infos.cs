// =====================================================
// JCO - OPR Levels Trading Range DST with RSI EMA SL TP Infos
// =====================================================
// Version: 5.1
// Date: 2026-02-19
//
// Changelog:
// v5.1 (2026-02-19)
//   - Niveaux OPR (Opening Price Range) avec heures de début/fin configurables
//   - Plage de trading avec heures de début/arrêt/clôture configurables
//   - Support DST avec conversion de fuseau horaire (New York, Londres, Paris, etc.)
//   - RSI avec signaux de surachat/survente et détection du premier signal
//   - EMA (Exponential Moving Average) en overlay
//   - Niveaux SL/TP automatiques basés sur les swing points avec marge configurable
//   - Panneau de money management avec double configuration capital/risque
//   - Mise à jour en temps réel des lignes OPR pendant la période OPR
//   - Contrôle d'affichage par timeframe avec timeframe maximum configurable
// =====================================================

using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class OprRange
    {
        public double High { get; set; }
        public double Low { get; set; }
    }

    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TimezoneVerticalLines : Indicator
    {
        // Groupe OPR Start
        [Parameter("Heure", Group = "OPR Start", DefaultValue = 9)]
        public int OprStartHour { get; set; }
        
        [Parameter("Minutes", Group = "OPR Start", DefaultValue = 30)]
        public int OprStartMinutes { get; set; }
        
        [Parameter("Couleur", Group = "OPR Start", DefaultValue = "Red")]
        public string OprStartColor { get; set; }

        // Groupe OPR Stop
        [Parameter("Heure", Group = "OPR Stop", DefaultValue = 9)]
        public int OprStopHour { get; set; }
        
        [Parameter("Minutes", Group = "OPR Stop", DefaultValue = 35)]
        public int OprStopMinutes { get; set; }
        
        [Parameter("Couleur", Group = "OPR Stop", DefaultValue = "Red")]
        public string OprStopColor { get; set; }

        // Groupe Trading Start
        [Parameter("Heure", Group = "Trading Start", DefaultValue = 10)]
        public int TradingStartHour { get; set; }
        
        [Parameter("Minutes", Group = "Trading Start", DefaultValue = 30)]
        public int TradingStartMinutes { get; set; }
        
        [Parameter("Couleur", Group = "Trading Start", DefaultValue = "Green")]
        public string TradingStartColor { get; set; }

        // Groupe Trading Stop
        [Parameter("Heure", Group = "Trading Stop", DefaultValue = 13)]
        public int TradingStopHour { get; set; }
        
        [Parameter("Minutes", Group = "Trading Stop", DefaultValue = 30)]
        public int TradingStopMinutes { get; set; }
        
        [Parameter("Couleur", Group = "Trading Stop", DefaultValue = "Yellow")]
        public string TradingStopColor { get; set; }

        // Groupe Trading Close
        [Parameter("Heure", Group = "Trading Close", DefaultValue = 16)]
        public int TradingCloseHour { get; set; }
        
        [Parameter("Minutes", Group = "Trading Close", DefaultValue = 0)]
        public int TradingCloseMinutes { get; set; }
        
        [Parameter("Couleur", Group = "Trading Close", DefaultValue = "Orange")]
        public string TradingCloseColor { get; set; }

        // Groupe OPR Lines
        [Parameter("Afficher lignes OPR", Group = "OPR Lines", DefaultValue = true)]
        public bool ShowOprLines { get; set; }
        
        [Parameter("Timeframe OPR", Group = "OPR Lines", DefaultValue = "m1")]
        public string OprTimeframe { get; set; }
        
        [Parameter("Couleur High OPR", Group = "OPR Lines", DefaultValue = "White")]
        public string OprHighColor { get; set; }
        
        [Parameter("Couleur Low OPR", Group = "OPR Lines", DefaultValue = "White")]
        public string OprLowColor { get; set; }
        
        [Parameter("Style lignes OPR", Group = "OPR Lines", DefaultValue = 2)]
        public LineStyle OprLineStyle { get; set; }
        
        [Parameter("Durée affichage OPR (heures)", Group = "OPR Lines", DefaultValue = 8)]
        public int OprDisplayHours { get; set; }

        // Groupe EMA
        [Parameter("Afficher EMA", Group = "EMA", DefaultValue = true)]
        public bool ShowEma { get; set; }
        
        [Parameter("Période EMA", Group = "EMA", DefaultValue = 50)]
        public int EmaPeriod { get; set; }

        // Groupe RSI
        [Parameter("Activer RSI", Group = "RSI", DefaultValue = true)]
        public bool EnableRsi { get; set; }
        
        [Parameter("RSI Period", Group = "RSI", DefaultValue = 14)]
        public int RsiPeriod { get; set; }

        [Parameter("Overbought Level", Group = "RSI", DefaultValue = 70)]
        public double OverboughtLevel { get; set; }

        [Parameter("Oversold Level", Group = "RSI", DefaultValue = 30)]
        public double OversoldLevel { get; set; }

        [Parameter("Signal Distance", Group = "RSI", DefaultValue = 5)]
        public int SignalDistance { get; set; }

        [Parameter("Signal Size", Group = "RSI", DefaultValue = 5)]
        public int SignalSize { get; set; }

        // Groupe General
        [Parameter("Fuseau Horaire", Group = "General", DefaultValue = "Eastern Standard Time",
                  Description = "Fuseaux horaires courants :\n" +
                               "• New York: \"Eastern Standard Time\" (UTC-5/-4)\n" +
                               "• Londres: \"GMT Standard Time\" (UTC+0/+1)\n" +
                               "• Paris: \"Romance Standard Time\" (UTC+1/+2)\n" +
                               "• Tokyo: \"Tokyo Standard Time\" (UTC+9, pas de DST)\n" +
                               "• Sydney: \"AUS Eastern Standard Time\" (UTC+10/+11)\n" +
                               "• Frankfurt: \"W. Europe Standard Time\" (UTC+1/+2)")]
        public string TimeZoneId { get; set; }
        
        [Parameter("Epaisseur des lignes", Group = "General", DefaultValue = 2)]
        public int LineThickness { get; set; }
        
        [Parameter("Style des lignes", Group = "General", DefaultValue = 2)]
        public LineStyle LineStyle { get; set; }
        
        [Parameter("Afficher uniquement aujourd hui", Group = "General", DefaultValue = false)]
        public bool ShowOnlyToday { get; set; }

        [Parameter("Afficher lignes verticales", Group = "General", DefaultValue = true)]
        public bool ShowVerticalLines { get; set; }

        [Parameter("Affichage infos trade", Group = "General", DefaultValue = 0,
                  Description = "0 = Toujours affiché\n1 = Uniquement pendant les heures de trading\n2 = Pas d'affichage des calculs")]
        public int TradeInfoDisplayMode { get; set; }

        [Parameter("Timeframe max (minutes)", Group = "General", DefaultValue = 60,
                  Description = "Timeframe maximale pour afficher les lignes (en minutes)\n" +
                               "60 = 1 heure, 240 = 4 heures, 1440 = Daily\n" +
                               "Les lignes ne s'afficheront pas sur des timeframes supérieures")]
        public int MaxTimeframeMinutes { get; set; }

        // Groupe Money Management
        [Parameter("Afficher lignes SL/TP", Group = "Money Management", DefaultValue = true)]
        public bool ShowSlTpLines { get; set; }

        [Parameter("Risk:Reward Ratio", Group = "Money Management", DefaultValue = 1.5)]
        public double RiskRewardRatio { get; set; }
        
        [Parameter("Marge SL (pips)", Group = "Money Management", DefaultValue = 2)]
        public int SlMarginPips { get; set; }
        
        [Parameter("Capital 1", Group = "Money Management", DefaultValue = 10000)]
        public double Capital1 { get; set; }
        
        [Parameter("Risk 1 (%)", Group = "Money Management", DefaultValue = 1.0)]
        public double Risk1 { get; set; }
        
        [Parameter("Capital 2", Group = "Money Management", DefaultValue = 150000)]
        public double Capital2 { get; set; }
        
        [Parameter("Risk 2 (%)", Group = "Money Management", DefaultValue = 0.5)]
        public double Risk2 { get; set; }

        [Output("EMA", LineColor = "DeepSkyBlue")]
        public IndicatorDataSeries EmaOutput { get; set; }

        [Output("Overbought Signal", LineColor = "Lime", PlotType = PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries OverboughtSignal { get; set; }

        [Output("Oversold Signal", LineColor = "Red", PlotType = PlotType.Points, Thickness = 5)]
        public IndicatorDataSeries OversoldSignal { get; set; }

        [Output("First Overbought Signal", LineColor = "Yellow", PlotType = PlotType.Points, Thickness = 7)]
        public IndicatorDataSeries FirstOverboughtSignal { get; set; }

        [Output("First Oversold Signal", LineColor = "Orange", PlotType = PlotType.Points, Thickness = 7)]
        public IndicatorDataSeries FirstOversoldSignal { get; set; }

        private TimeZoneInfo targetTimeZone;
        private TimeSpan[] targetTimes;
        private Color[] lineColors;
        private Bars oprBars;
        
        // Variables RSI
        private IndicatorDataSeries rsi;
        private IndicatorDataSeries gains;
        private IndicatorDataSeries losses;
        private IndicatorDataSeries avgGain;
        private IndicatorDataSeries avgLoss;
        
        // Variables EMA
        private IndicatorDataSeries emaValues;
        private double multiplier;
        
        // Variables pour premiers signaux RSI
        private DateTime lastTradingDate;
        private bool firstOverboughtToday;
        private bool firstOversoldToday;
        
        // Variables pour les niveaux OPR du jour
        private double todayOprHigh;
        private double todayOprLow;
        private bool todayOprCalculated;
        
        // Variables pour les niveaux SL/TP
        private bool firstSignalProcessedToday;
        private int swingLookback = 20;
        
        // Variables pour l'affichage texte
        private double tradeSL;
        private double tradeTP;
        private double tradeEntry;
        private double tradeSLPips;
        private bool hasTradeInfo = false;

        // Variables pour l'affichage OPR en temps réel
        private bool isCurrentlyInOprPeriod = false;
        private DateTime currentOprStartUtc;
        private DateTime currentOprStopUtc;
        private double currentOprHigh = double.MinValue;
        private double currentOprLow = double.MaxValue;
        private DateTime currentOprDate;

        public override void Calculate(int index)
        {
            if (index == 0)
            {
                Initialize();
            }
            
            // Vérifier si la timeframe actuelle permet l'affichage des lignes
            bool canShowLines = CanShowLinesOnCurrentTimeframe();
            
            // Vérifier et mettre à jour les lignes OPR en temps réel
            if (ShowOprLines && canShowLines)
            {
                UpdateOprLinesRealTime(index);
            }
            
            if (IsLastBar)
            {
                if (canShowLines)
                {
                    if (ShowVerticalLines)
                    {
                        DrawVerticalLines();
                    }

                    if (ShowOprLines)
                    {
                        DrawOprLines(); // Garder pour les jours précédents
                    }
                }
                else
                {
                    // Supprimer toutes les lignes si la timeframe est trop grande
                    RemoveAllLines();
                }
                
                UpdateTradeInfoDisplay();
            }
            
            if (EnableRsi)
            {
                CalculateRsi(index);
            }
            
            if (ShowEma)
            {
                CalculateEma(index);
            }
        }

        // Nouvelle méthode pour vérifier si on peut afficher les lignes sur la timeframe actuelle
        private bool CanShowLinesOnCurrentTimeframe()
        {
            // Obtenir la durée de la timeframe actuelle en minutes
            int currentTimeframeMinutes = GetTimeframeInMinutes(TimeFrame);
            
            // Retourner true si la timeframe actuelle est <= à la timeframe max
            return currentTimeframeMinutes <= MaxTimeframeMinutes;
        }

        // Méthode pour convertir une TimeFrame en minutes
        private int GetTimeframeInMinutes(TimeFrame tf)
        {
            if (tf == TimeFrame.Minute) return 1;
            if (tf == TimeFrame.Minute2) return 2;
            if (tf == TimeFrame.Minute3) return 3;
            if (tf == TimeFrame.Minute4) return 4;
            if (tf == TimeFrame.Minute5) return 5;
            if (tf == TimeFrame.Minute6) return 6;
            if (tf == TimeFrame.Minute7) return 7;
            if (tf == TimeFrame.Minute8) return 8;
            if (tf == TimeFrame.Minute9) return 9;
            if (tf == TimeFrame.Minute10) return 10;
            if (tf == TimeFrame.Minute15) return 15;
            if (tf == TimeFrame.Minute20) return 20;
            if (tf == TimeFrame.Minute30) return 30;
            if (tf == TimeFrame.Minute45) return 45;
            if (tf == TimeFrame.Hour) return 60;
            if (tf == TimeFrame.Hour2) return 120;
            if (tf == TimeFrame.Hour3) return 180;
            if (tf == TimeFrame.Hour4) return 240;
            if (tf == TimeFrame.Hour6) return 360;
            if (tf == TimeFrame.Hour8) return 480;
            if (tf == TimeFrame.Hour12) return 720;
            if (tf == TimeFrame.Daily) return 1440;
            if (tf == TimeFrame.Day2) return 2880;
            if (tf == TimeFrame.Day3) return 4320;
            if (tf == TimeFrame.Weekly) return 10080;
            if (tf == TimeFrame.Monthly) return 43200;
            
            return 1440; // Défaut = Daily
        }

        // Méthode pour supprimer toutes les lignes
        private void RemoveAllLines()
        {
            // Supprimer les lignes verticales
            DateTime today = Server.Time.Date;
            DateTime startDate = today.AddDays(-30);
            
            for (DateTime date = startDate; date <= today; date = date.AddDays(1))
            {
                for (int i = 0; i < 5; i++)
                {
                    string lineName = $"VLine_TZ_{date:yyyyMMdd}_{i}";
                    Chart.RemoveObject(lineName);
                }
                
                // Supprimer les lignes OPR
                string dateStr = date.ToString("yyyyMMdd");
                Chart.RemoveObject($"OPR_High_{dateStr}");
                Chart.RemoveObject($"OPR_Low_{dateStr}");
            }
            
            // Supprimer les lignes OPR en temps réel
            Chart.RemoveObject("OPR_High_RealTime");
            Chart.RemoveObject("OPR_Low_RealTime");
        }

        protected override void Initialize()
        {
            try
            {
                // Initialiser le fuseau horaire
                targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
            }
            catch (Exception)
            {
                Print($"Erreur: Fuseau horaire '{TimeZoneId}' non trouvé. Utilisation d'Eastern Standard Time par défaut.");
                targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            }

            // Créer les heures à partir des paramètres
            targetTimes = new TimeSpan[5];
            targetTimes[0] = new TimeSpan(OprStartHour, OprStartMinutes, 0);
            targetTimes[1] = new TimeSpan(OprStopHour, OprStopMinutes, 0);
            targetTimes[2] = new TimeSpan(TradingStartHour, TradingStartMinutes, 0);
            targetTimes[3] = new TimeSpan(TradingStopHour, TradingStopMinutes, 0);
            targetTimes[4] = new TimeSpan(TradingCloseHour, TradingCloseMinutes, 0);

            // Initialiser les couleurs
            lineColors = new Color[5];
            lineColors[0] = ParseColor(OprStartColor, Color.Red);
            lineColors[1] = ParseColor(OprStopColor, Color.Red);
            lineColors[2] = ParseColor(TradingStartColor, Color.Green);
            lineColors[3] = ParseColor(TradingStopColor, Color.Green);
            lineColors[4] = ParseColor(TradingCloseColor, Color.Orange);
            
            // Charger les données pour le calcul OPR
            if (ShowOprLines)
            {
                InitializeOprBars();
            }
            
            // Initialiser les séries RSI
            if (EnableRsi)
            {
                InitializeRsi();
            }
            
            // Initialiser l'EMA
            if (ShowEma)
            {
                InitializeEma();
            }
            
            // Afficher un message si la timeframe est trop grande
            if (!CanShowLinesOnCurrentTimeframe())
            {
                Print($"Info: Les lignes ne seront pas affichées sur cette timeframe ({TimeFrame}). Timeframe max autorisée: {MaxTimeframeMinutes} minutes.");
            }
        }
        
        private void InitializeOprBars()
        {
            try
            {
                TimeFrame timeframe = ParseTimeFrame(OprTimeframe);
                oprBars = MarketData.GetBars(timeframe);
                Print($"OPR Bars chargées : {timeframe}");
            }
            catch (Exception ex)
            {
                Print($"Erreur lors du chargement des données OPR : {ex.Message}");
                ShowOprLines = false;
            }
        }

        private void InitializeRsi()
        {
            rsi = CreateDataSeries();
            gains = CreateDataSeries();
            losses = CreateDataSeries();
            avgGain = CreateDataSeries();
            avgLoss = CreateDataSeries();
        }

        private void InitializeEma()
        {
            emaValues = CreateDataSeries();
            multiplier = 2.0 / (EmaPeriod + 1);
        }
        
        private TimeFrame ParseTimeFrame(string timeframeString)
        {
            switch (timeframeString.ToLower())
            {
                case "m1": return TimeFrame.Minute;
                case "m2": return TimeFrame.Minute2;
                case "m3": return TimeFrame.Minute3;
                case "m5": return TimeFrame.Minute5;
                case "m10": return TimeFrame.Minute10;
                case "m15": return TimeFrame.Minute15;
                case "m30": return TimeFrame.Minute30;
                case "h1": return TimeFrame.Hour;
                case "h2": return TimeFrame.Hour2;
                case "h3": return TimeFrame.Hour3;
                case "h4": return TimeFrame.Hour4;
                case "h6": return TimeFrame.Hour6;
                case "h8": return TimeFrame.Hour8;
                case "h12": return TimeFrame.Hour12;
                case "d1": return TimeFrame.Daily;
                case "w1": return TimeFrame.Weekly;
                case "mn1": return TimeFrame.Monthly;
                default:
                    Print($"Timeframe '{timeframeString}' non reconnu, utilisation de M1");
                    return TimeFrame.Minute;
            }
        }

        // Nouvelle méthode pour la mise à jour en temps réel
        private void UpdateOprLinesRealTime(int index)
        {
            if (oprBars == null) return;
            
            DateTime currentBarTime = Bars.OpenTimes[index];
            DateTime currentDate = currentBarTime.Date;
            
            // Vérifier si c'est un jour de semaine
            if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                return;
            
            // Calculer les heures OPR pour aujourd'hui
            DateTime oprStartLocal = currentDate.Add(targetTimes[0]);
            DateTime oprStopLocal = currentDate.Add(targetTimes[1]);
            DateTime oprStartUtc = ConvertToUtc(oprStartLocal);
            DateTime oprStopUtc = ConvertToUtc(oprStopLocal);
            
            // Nouveau jour ou première fois
            if (currentOprDate != currentDate)
            {
                currentOprDate = currentDate;
                currentOprStartUtc = oprStartUtc;
                currentOprStopUtc = oprStopUtc;
                currentOprHigh = double.MinValue;
                currentOprLow = double.MaxValue;
                isCurrentlyInOprPeriod = false;
                
                // Supprimer les anciennes lignes OPR en temps réel
                Chart.RemoveObject("OPR_High_RealTime");
                Chart.RemoveObject("OPR_Low_RealTime");
            }
            
            // Vérifier si on est dans la période OPR
            bool wasInOprPeriod = isCurrentlyInOprPeriod;
            isCurrentlyInOprPeriod = currentBarTime >= oprStartUtc && currentBarTime < oprStopUtc;
            
            if (isCurrentlyInOprPeriod)
            {
                // Mettre à jour les niveaux OPR avec la bougie courante
                double currentHigh = Bars.HighPrices[index];
                double currentLow = Bars.LowPrices[index];
                
                if (currentOprHigh == double.MinValue || currentHigh > currentOprHigh)
                {
                    currentOprHigh = currentHigh;
                }
                
                if (currentOprLow == double.MaxValue || currentLow < currentOprLow)
                {
                    currentOprLow = currentLow;
                }
                
                // Dessiner ou mettre à jour les lignes en temps réel
                UpdateRealtimeOprLines(currentDate);
            }
            else if (wasInOprPeriod && !isCurrentlyInOprPeriod)
            {
                // On vient de sortir de la période OPR, dessiner les lignes finales
                DrawFinalOprLines(currentDate);
                
                // Supprimer les lignes temps réel
                Chart.RemoveObject("OPR_High_RealTime");
                Chart.RemoveObject("OPR_Low_RealTime");
            }
        }

        // Méthode pour mettre à jour les lignes en temps réel
        private void UpdateRealtimeOprLines(DateTime currentDate)
        {
            if (currentOprHigh == double.MinValue || currentOprLow == double.MaxValue)
                return;
            
            // Calculer l'heure de fin d'affichage (Trading Stop)
            DateTime tradingStopLocal = currentDate.Add(targetTimes[3]);
            DateTime lineEndTime = ConvertToUtc(tradingStopLocal);
            
            // Supprimer les anciennes lignes
            Chart.RemoveObject("OPR_High_RealTime");
            Chart.RemoveObject("OPR_Low_RealTime");
            
            // Dessiner les nouvelles lignes avec les valeurs actuelles
            var highLine = Chart.DrawTrendLine("OPR_High_RealTime", 
                                              currentOprStartUtc, currentOprHigh,
                                              lineEndTime, currentOprHigh,
                                              ParseColor(OprHighColor, Color.Red),
                                              LineThickness, OprLineStyle);
            highLine.IsInteractive = false;
            highLine.Comment = $"OPR High (Live): {currentOprHigh:F5}";
            
            var lowLine = Chart.DrawTrendLine("OPR_Low_RealTime",
                                             currentOprStartUtc, currentOprLow,
                                             lineEndTime, currentOprLow,
                                             ParseColor(OprLowColor, Color.Red),
                                             LineThickness, OprLineStyle);
            lowLine.IsInteractive = false;
            lowLine.Comment = $"OPR Low (Live): {currentOprLow:F5}";
        }

        // Méthode pour dessiner les lignes finales à la fin de la période OPR
        private void DrawFinalOprLines(DateTime currentDate)
        {
            if (currentOprHigh == double.MinValue || currentOprLow == double.MaxValue)
                return;
            
            string dateStr = currentDate.ToString("yyyyMMdd");
            string highLineName = $"OPR_High_{dateStr}";
            string lowLineName = $"OPR_Low_{dateStr}";
            
            // Calculer l'heure de fin d'affichage
            DateTime tradingStopLocal = currentDate.Add(targetTimes[3]);
            DateTime lineEndTime = ConvertToUtc(tradingStopLocal);
            
            // Supprimer les anciennes lignes s'elles existent
            Chart.RemoveObject(highLineName);
            Chart.RemoveObject(lowLineName);
            
            // Dessiner les lignes finales
            var highLine = Chart.DrawTrendLine(highLineName,
                                              currentOprStartUtc, currentOprHigh,
                                              lineEndTime, currentOprHigh,
                                              ParseColor(OprHighColor, Color.Red),
                                              LineThickness, OprLineStyle);
            highLine.IsInteractive = false;
            highLine.Comment = $"OPR High Final: {currentOprHigh:F5}";
            
            var lowLine = Chart.DrawTrendLine(lowLineName,
                                             currentOprStartUtc, currentOprLow,
                                             lineEndTime, currentOprLow,
                                             ParseColor(OprLowColor, Color.Red),
                                             LineThickness, OprLineStyle);
            lowLine.IsInteractive = false;
            lowLine.Comment = $"OPR Low Final: {currentOprLow:F5}";
            
            Print($"Lignes OPR finales tracées - High: {currentOprHigh:F5}, Low: {currentOprLow:F5}");
        }

        private void DrawVerticalLines()
        {
            DateTime today = Server.Time.Date;
            DateTime startDate = ShowOnlyToday ? today : today.AddDays(-30);
            DateTime endDate = today.AddDays(1);

            for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
            {
                // Vérifier si c'est un jour de semaine (éviter weekend)
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                for (int i = 0; i < targetTimes.Length; i++)
                {
                    DateTime localDateTime = date.Add(targetTimes[i]);
                    DateTime utcDateTime = ConvertToUtc(localDateTime);
                    
                    // Vérifier que l'heure UTC est dans la plage des données
                    if (utcDateTime >= Bars.OpenTimes[0] && utcDateTime <= Bars.OpenTimes.Last(1))
                    {
                        string lineName = $"VLine_TZ_{date:yyyyMMdd}_{i}";
                        string lineLabel = "";
                        
                        switch (i)
                        {
                            case 0:
                                lineLabel = "OPR Start";
                                break;
                            case 1:
                                lineLabel = "OPR Stop";
                                break;
                            case 2:
                                lineLabel = "Trading Start";
                                break;
                            case 3:
                                lineLabel = "Trading Stop";
                                break;
                            case 4:
                                lineLabel = "Trading Close";
                                break;
                        }
                        
                        // Vérifier si la ligne existe déjà avant de la créer
                        if (Chart.FindObject(lineName) == null)
                        {
                            var vLine = Chart.DrawVerticalLine(lineName, utcDateTime, lineColors[i], LineThickness, LineStyle);
                            vLine.IsInteractive = false;
                            vLine.Comment = $"{lineLabel} - {localDateTime:HH:mm} {targetTimeZone.StandardName}";
                        }
                    }
                }
            }
        }

        private void DrawOprLines()
        {
            if (oprBars == null) return;
            
            DateTime today = Server.Time.Date;
            DateTime startDate = ShowOnlyToday ? today : today.AddDays(-30);
            DateTime endDate = today.AddDays(1);

            for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
            {
                // Vérifier si c'est un jour de semaine
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                // Ne pas redessiner le jour courant si on utilise l'affichage temps réel
                if (date == today)
                    continue;

                // Calculer les heures OPR Start et Stop en UTC
                DateTime oprStartLocal = date.Add(targetTimes[0]);
                DateTime oprStopLocal = date.Add(targetTimes[1]);
                
                DateTime oprStartUtc = ConvertToUtc(oprStartLocal);
                DateTime oprStopUtc = ConvertToUtc(oprStopLocal);

                // Calculer le range OPR
                var oprRange = CalculateOprRange(oprStartUtc, oprStopUtc);
                
                if (oprRange != null)
                {
                    string dateStr = date.ToString("yyyyMMdd");
                    string highLineName = $"OPR_High_{dateStr}";
                    string lowLineName = $"OPR_Low_{dateStr}";
                    
                    // Dessiner ligne du plus haut
                    if (Chart.FindObject(highLineName) == null)
                    {
                        DateTime tradingStopLocal = date.Add(targetTimes[3]); // Trading Stop
                        DateTime lineEndTime = ConvertToUtc(tradingStopLocal);
                        
                        var highLine = Chart.DrawTrendLine(highLineName, oprStartUtc, oprRange.High, 
                                                          lineEndTime, oprRange.High, 
                                                          ParseColor(OprHighColor, Color.Red), 
                                                          LineThickness, OprLineStyle);
                        highLine.IsInteractive = false;
                        highLine.Comment = $"OPR High: {oprRange.High:F5} ({oprStartLocal:HH:mm}-{oprStopLocal:HH:mm})";
                    }
                    
                    // Dessiner ligne du plus bas
                    if (Chart.FindObject(lowLineName) == null)
                    {
                        DateTime tradingStopLocal = date.Add(targetTimes[3]); // Trading Stop
                        DateTime lineEndTime = ConvertToUtc(tradingStopLocal);
                        
                        var lowLine = Chart.DrawTrendLine(lowLineName, oprStartUtc, oprRange.Low,
                                                         lineEndTime, oprRange.Low,
                                                         ParseColor(OprLowColor, Color.Red),
                                                         LineThickness, OprLineStyle);
                        lowLine.IsInteractive = false;
                        lowLine.Comment = $"OPR Low: {oprRange.Low:F5} ({oprStartLocal:HH:mm}-{oprStopLocal:HH:mm})";
                    }
                }
            }
        }
        
        private OprRange CalculateOprRange(DateTime startUtc, DateTime stopUtc)
        {
            if (oprBars == null) return null;
            
            double high = double.MinValue;
            double low = double.MaxValue;
            bool foundData = false;
            
            try
            {
                // Rechercher toutes les bougies dans la période OPR
                for (int i = oprBars.Count - 1; i >= 0; i--)
                {
                    DateTime barTime = oprBars.OpenTimes[i];
                    DateTime barCloseTime = barTime.Add(GetTimeFrameDuration());
                    
                    // Vérifier si la bougie chevauche avec la période OPR
                    if (barTime < stopUtc && barCloseTime > startUtc)
                    {
                        high = Math.Max(high, oprBars.HighPrices[i]);
                        low = Math.Min(low, oprBars.LowPrices[i]);
                        foundData = true;
                    }
                    
                    // Arrêter si on est trop loin dans le passé
                    if (barTime < startUtc.AddHours(-24))
                        break;
                }
                
                if (foundData)
                {
                    Print($"OPR Range trouvé: High={high:F5}, Low={low:F5} entre {startUtc:HH:mm} et {stopUtc:HH:mm}");
                    return new OprRange { High = high, Low = low };
                }
            }
            catch (Exception ex)
            {
                Print($"Erreur lors du calcul OPR Range: {ex.Message}");
            }
            
            return null;
        }
        
        private TimeSpan GetTimeFrameDuration()
        {
            string tf = OprTimeframe.ToLower();
            
            if (tf == "m1") return TimeSpan.FromMinutes(1);
            if (tf == "m2") return TimeSpan.FromMinutes(2);
            if (tf == "m3") return TimeSpan.FromMinutes(3);
            if (tf == "m5") return TimeSpan.FromMinutes(5);
            if (tf == "m10") return TimeSpan.FromMinutes(10);
            if (tf == "m15") return TimeSpan.FromMinutes(15);
            if (tf == "m30") return TimeSpan.FromMinutes(30);
            if (tf == "h1") return TimeSpan.FromHours(1);
            if (tf == "h2") return TimeSpan.FromHours(2);
            if (tf == "h3") return TimeSpan.FromHours(3);
            if (tf == "h4") return TimeSpan.FromHours(4);
            if (tf == "h6") return TimeSpan.FromHours(6);
            if (tf == "h8") return TimeSpan.FromHours(8);
            if (tf == "h12") return TimeSpan.FromHours(12);
            if (tf == "d1") return TimeSpan.FromDays(1);
            
            return TimeSpan.FromMinutes(1); // Défaut
        }

        private void CalculateRsi(int index)
        {
            // Première barre, pas de calcul possible
            if (index == 0)
                return;

            // Calcul des gains et pertes
            double change = Bars.ClosePrices[index] - Bars.ClosePrices[index - 1];
            gains[index] = change > 0 ? change : 0;
            losses[index] = change < 0 ? Math.Abs(change) : 0;

            // Calcul des moyennes mobiles modifiées (MMA)
            if (index < RsiPeriod)
            {
                // Période d'initialisation - moyenne simple
                double sumGains = 0;
                double sumLosses = 0;
                
                for (int i = 1; i <= index; i++)
                {
                    sumGains += gains[i];
                    sumLosses += losses[i];
                }
                
                avgGain[index] = sumGains / index;
                avgLoss[index] = sumLosses / index;
            }
            else if (index == RsiPeriod)
            {
                // Premier calcul complet - moyenne simple sur la période
                double sumGains = 0;
                double sumLosses = 0;
                
                for (int i = index - RsiPeriod + 1; i <= index; i++)
                {
                    sumGains += gains[i];
                    sumLosses += losses[i];
                }
                
                avgGain[index] = sumGains / RsiPeriod;
                avgLoss[index] = sumLosses / RsiPeriod;
            }
            else
            {
                // Moyenne mobile modifiée (lissage exponentiel modifié)
                avgGain[index] = (avgGain[index - 1] * (RsiPeriod - 1) + gains[index]) / RsiPeriod;
                avgLoss[index] = (avgLoss[index - 1] * (RsiPeriod - 1) + losses[index]) / RsiPeriod;
            }

            // Calcul du RSI
            if (avgLoss[index] != 0)
            {
                double rs = avgGain[index] / avgLoss[index];
                rsi[index] = 100 - (100 / (1 + rs));
            }
            else
            {
                rsi[index] = 100;
            }

            // Affichage des signaux selon les conditions
            if (index >= RsiPeriod)
            {
                // Vérifier si on est dans la fenêtre de trading
                DateTime currentBarTime = Bars.OpenTimes[index];
                DateTime currentDate = currentBarTime.Date;
                bool isInTradingWindow = IsInTradingWindow(currentBarTime, currentDate);
                
                // Reset des premiers signaux pour chaque nouvelle journée de trading
                if (lastTradingDate != currentDate)
                {
                    lastTradingDate = currentDate;
                    firstOverboughtToday = false;
                    firstOversoldToday = false;
                    todayOprCalculated = false;
                    firstSignalProcessedToday = false;
                    hasTradeInfo = false;
                    UpdateTodayOprLevels(currentDate);
                }
                
                if (isInTradingWindow && todayOprCalculated)
                {
                    double currentClose = Bars.ClosePrices[index];
                    
                    // Signal de surachat
                    if (rsi[index] > OverboughtLevel)
                    {
                        double signalPrice = Bars.HighPrices[index] + (Symbol.PipSize * SignalDistance);
                        
                        if (!firstSignalProcessedToday && !firstOverboughtToday && currentClose > todayOprHigh)
                        {
                            // Premier signal overbought de la journée ET sortie au-dessus de l'OPR high
                            FirstOverboughtSignal[index] = signalPrice;
                            firstOverboughtToday = true;
                            
                            // Tracer le niveau SL/TP pour la vente
                            if (!firstSignalProcessedToday)
                            {
                                DrawSwingLevel(index, false); // false = chercher swing LOW pour signal de vente
                                firstSignalProcessedToday = true;
                            }
                        }
                        else
                        {
                            // Signaux suivants ou dans le range - couleur normale
                            OverboughtSignal[index] = signalPrice;
                        }
                    }
                    else
                    {
                        // RSI ne dépasse plus OverboughtLevel - effacer les signaux
                        OverboughtSignal[index] = double.NaN;
                        FirstOverboughtSignal[index] = double.NaN;
                    }

                    // Signal de survente
                    if (rsi[index] < OversoldLevel)
                    {
                        double signalPrice = Bars.LowPrices[index] - (Symbol.PipSize * SignalDistance);
                        
                        if (!firstSignalProcessedToday && !firstOversoldToday && currentClose < todayOprLow)
                        {
                            // Premier signal oversold de la journée ET sortie en-dessous de l'OPR low
                            FirstOversoldSignal[index] = signalPrice;
                            firstOversoldToday = true;
                            
                            // Tracer le niveau SL/TP pour l'achat
                            if (!firstSignalProcessedToday)
                            {
                                DrawSwingLevel(index, true); // true = chercher swing HIGH pour signal d'achat
                                firstSignalProcessedToday = true;
                            }
                        }
                        else
                        {
                            // Signaux suivants ou dans le range - couleur normale
                            OversoldSignal[index] = signalPrice;
                        }
                    }
                    else
                    {
                        // RSI ne dépasse plus OversoldLevel - effacer les signaux
                        OversoldSignal[index] = double.NaN;
                        FirstOversoldSignal[index] = double.NaN;
                    }
                }
                else
                {
                    // En dehors de la fenêtre de trading ou OPR non calculé - signaux normaux uniquement
                    if (rsi[index] > OverboughtLevel)
                    {
                        OverboughtSignal[index] = Bars.HighPrices[index] + (Symbol.PipSize * SignalDistance);
                    }
                    else
                    {
                        OverboughtSignal[index] = double.NaN;
                    }

                    if (rsi[index] < OversoldLevel)
                    {
                        OversoldSignal[index] = Bars.LowPrices[index] - (Symbol.PipSize * SignalDistance);
                    }
                    else
                    {
                        OversoldSignal[index] = double.NaN;
                    }
                    
                    // Effacer les signaux spéciaux en dehors de la fenêtre de trading
                    FirstOverboughtSignal[index] = double.NaN;
                    FirstOversoldSignal[index] = double.NaN;
                }
            }
        }
        
        private bool IsInTradingWindow(DateTime barTimeUtc, DateTime currentDate)
        {
            // Convertir les heures de trading en UTC
            DateTime tradingStartLocal = currentDate.Add(targetTimes[2]); // Trading Start
            DateTime tradingStopLocal = currentDate.Add(targetTimes[3]);  // Trading Stop
            
            DateTime tradingStartUtc = ConvertToUtc(tradingStartLocal);
            DateTime tradingStopUtc = ConvertToUtc(tradingStopLocal);
            
            // Vérifier si la barre est dans la fenêtre de trading (inclus les bornes)
            return barTimeUtc >= tradingStartUtc && barTimeUtc <= tradingStopUtc;
        }

        private void UpdateTodayOprLevels(DateTime currentDate)
        {
            if (oprBars == null)
            {
                todayOprCalculated = false;
                return;
            }
            
            // Calculer les heures OPR Start et Stop en UTC pour aujourd'hui
            DateTime oprStartLocal = currentDate.Add(targetTimes[0]);
            DateTime oprStopLocal = currentDate.Add(targetTimes[1]);
            
            DateTime oprStartUtc = ConvertToUtc(oprStartLocal);
            DateTime oprStopUtc = ConvertToUtc(oprStopLocal);

            // Calculer le range OPR
            var oprRange = CalculateOprRange(oprStartUtc, oprStopUtc);
            
            if (oprRange != null)
            {
                todayOprHigh = oprRange.High;
                todayOprLow = oprRange.Low;
                todayOprCalculated = true;
            }
            else
            {
                todayOprCalculated = false;
            }
        }

        private void CalculateEma(int index)
        {
            double currentClose = Bars.ClosePrices[index];
            
            if (index == 0)
            {
                // Premier point : utiliser le prix de clôture
                emaValues[index] = currentClose;
            }
            else if (index < EmaPeriod)
            {
                // Période d'initialisation : calculer la moyenne simple
                double sum = 0;
                for (int i = 0; i <= index; i++)
                {
                    sum += Bars.ClosePrices[i];
                }
                emaValues[index] = sum / (index + 1);
            }
            else if (index == EmaPeriod)
            {
                // Première EMA complète : utiliser SMA comme base
                double sum = 0;
                for (int i = index - EmaPeriod + 1; i <= index; i++)
                {
                    sum += Bars.ClosePrices[i];
                }
                emaValues[index] = sum / EmaPeriod;
            }
            else
            {
                // Calcul EMA standard : EMA = (Prix * Multiplier) + (EMA précédente * (1 - Multiplier))
                emaValues[index] = (currentClose * multiplier) + (emaValues[index - 1] * (1 - multiplier));
            }
            
            // Appliquer le style et la couleur à l'output
            EmaOutput[index] = emaValues[index];
        }

        private Color ParseColor(string colorString, Color defaultColor)
        {
            try
            {
                var property = typeof(Color).GetProperty(colorString);
                if (property != null)
                {
                    return (Color)property.GetValue(null);
                }
            }
            catch (Exception)
            {
                // Ignorer l'erreur et utiliser la couleur par défaut
            }
            return defaultColor;
        }

        private void DrawSwingLevel(int signalIndex, bool lookForSwingHigh)
        {
            int swingIndex = FindSwingPoint(signalIndex, lookForSwingHigh);
            
            if (swingIndex != -1)
            {
                double swingLevel = lookForSwingHigh ? Bars.HighPrices[swingIndex] : Bars.LowPrices[swingIndex];
                
                // Appliquer la marge de sécurité SL
                double marginInPrice = SlMarginPips * Symbol.PipSize;
                if (lookForSwingHigh) // Signal d'achat, ajouter la marge au swing high
                {
                    swingLevel += marginInPrice;
                }
                else // Signal de vente, soustraire la marge du swing low
                {
                    swingLevel -= marginInPrice;
                }
                
                // Appliquer la condition de limitation au range OPR (après application de la marge)
                if (lookForSwingHigh && swingLevel > todayOprHigh)
                {
                    swingLevel = todayOprHigh;
                    Print($"Swing High avec marge limité au OPR High: {swingLevel:F5}");
                }
                else if (!lookForSwingHigh && swingLevel < todayOprLow)
                {
                    swingLevel = todayOprLow;
                    Print($"Swing Low avec marge limité au OPR Low: {swingLevel:F5}");
                }
                
                // Prix de clôture de la bougie du signal
                double signalClosePrice = Bars.ClosePrices[signalIndex];
                
                // Calculer la distance pour le SL
                double slDistance = Math.Abs(signalClosePrice - swingLevel);
                
                // Calculer le niveau TP (à l'opposé du SL)
                double tpLevel;
                if (lookForSwingHigh) // Signal d'achat, SL au-dessus, TP en-dessous
                {
                    tpLevel = signalClosePrice - (slDistance * RiskRewardRatio);
                }
                else // Signal de vente, SL en-dessous, TP au-dessus
                {
                    tpLevel = signalClosePrice + (slDistance * RiskRewardRatio);
                }
                
                DateTime swingTime = Bars.OpenTimes[swingIndex];
                DateTime lineEndTime = swingTime.AddHours(1); // Ligne d'1 heure
                
                // Tracer la ligne rouge (SL)
                string slLineName = $"SL_{swingTime:yyyyMMdd_HHmmss}_{(lookForSwingHigh ? "High" : "Low")}";
                if (ShowSlTpLines && Chart.FindObject(slLineName) == null)
                {
                    var slLine = Chart.DrawTrendLine(slLineName, swingTime, swingLevel,
                                                    lineEndTime, swingLevel,
                                                    Color.Red, LineThickness, LineStyle.Solid);
                    slLine.IsInteractive = false;
                    slLine.Comment = $"SL: {swingLevel:F5} (+{SlMarginPips} pips marge)";
                }

                // Tracer la ligne verte (TP)
                string tpLineName = $"TP_{swingTime:yyyyMMdd_HHmmss}_{(lookForSwingHigh ? "High" : "Low")}";
                if (ShowSlTpLines && Chart.FindObject(tpLineName) == null)
                {
                    var tpLine = Chart.DrawTrendLine(tpLineName, swingTime, tpLevel,
                                                    lineEndTime, tpLevel,
                                                    Color.Green, LineThickness, LineStyle.Solid);
                    tpLine.IsInteractive = false;
                    tpLine.Comment = $"TP: {tpLevel:F5} (R:R {RiskRewardRatio:F1})";
                }
                
                Print($"SL tracé à {swingLevel:F5} (avec marge {SlMarginPips} pips), TP tracé à {tpLevel:F5} (R:R {RiskRewardRatio:F1}) pour signal à {signalClosePrice:F5}");
                
                // Stocker les valeurs pour l'affichage texte
                tradeSL = swingLevel;
                tradeTP = tpLevel;
                tradeEntry = signalClosePrice;
                tradeSLPips = Math.Round(slDistance / Symbol.PipSize, 2);
                hasTradeInfo = true;
            }
        }
        
        private int FindSwingPoint(int currentIndex, bool lookForSwingHigh)
        {
            // Chercher dans les 20 dernières bougies clôturées
            int startIndex = Math.Max(0, currentIndex - swingLookback);
            int endIndex = currentIndex - 1; // Exclure la bougie courante
            
            // Parcourir de la plus récente vers la plus ancienne
            for (int i = endIndex; i >= startIndex; i--)
            {
                if (IsSwingPoint(i, lookForSwingHigh))
                {
                    return i;
                }
            }
            
            return -1; // Aucun swing trouvé
        }
        
        private bool IsSwingPoint(int index, bool lookForSwingHigh)
        {
            // Vérifier qu'on a assez de bougies avant et après
            if (index <= 1 || index >= Bars.Count - 2)
                return false;
            
            if (lookForSwingHigh)
            {
                // Pour un swing high : le high doit être plus haut que les highs précédents et suivants
                double currentHigh = Bars.HighPrices[index];
                double previousHigh = Bars.HighPrices[index - 1];
                double nextHigh = Bars.HighPrices[index + 1];
                
                // Vérifier également les bougies à +/- 2 pour une meilleure validation
                if (index >= 2 && index < Bars.Count - 2)
                {
                    double previous2High = Bars.HighPrices[index - 2];
                    double next2High = Bars.HighPrices[index + 2];
                    
                    return currentHigh > previousHigh && 
                           currentHigh > nextHigh && 
                           currentHigh > previous2High && 
                           currentHigh > next2High;
                }
                else
                {
                    return currentHigh > previousHigh && currentHigh > nextHigh;
                }
            }
            else
            {
                // Pour un swing low : le low doit être plus bas que les lows précédents et suivants
                double currentLow = Bars.LowPrices[index];
                double previousLow = Bars.LowPrices[index - 1];
                double nextLow = Bars.LowPrices[index + 1];
                
                // Vérifier également les bougies à +/- 2 pour une meilleure validation
                if (index >= 2 && index < Bars.Count - 2)
                {
                    double previous2Low = Bars.LowPrices[index - 2];
                    double next2Low = Bars.LowPrices[index + 2];
                    
                    return currentLow < previousLow && 
                           currentLow < nextLow && 
                           currentLow < previous2Low && 
                           currentLow < next2Low;
                }
                else
                {
                    return currentLow < previousLow && currentLow < nextLow;
                }
            }
        }

        private void UpdateTradeInfoDisplay()
        {
            // Forcer la suppression de tout affichage existant d'abord
            Chart.RemoveObject("TradeInfoDisplay");
            
            // Vérifier si on doit afficher les informations selon le mode choisi
            bool shouldDisplay = false;
            
            if (TradeInfoDisplayMode == 0)
            {
                // Mode 0: Toujours afficher si on a des infos de trade
                shouldDisplay = hasTradeInfo;
            }
            else if (TradeInfoDisplayMode == 1)
            {
                // Mode 1: Afficher uniquement pendant les heures de trading
                DateTime currentTime = Server.Time;
                DateTime currentDate = currentTime.Date;
                bool isCurrentlyInTradingWindow = IsInTradingWindow(currentTime, currentDate);
                
                // Afficher seulement si on est dans la fenêtre ET qu'on a des infos
                shouldDisplay = hasTradeInfo && isCurrentlyInTradingWindow;
                
                // Si on a des infos mais qu'on est sorti de la fenêtre de trading, les effacer
                if (hasTradeInfo && !isCurrentlyInTradingWindow)
                {
                    hasTradeInfo = false;
                }
            }
            // Pour toute autre valeur (2, 3, 4...), shouldDisplay reste false
            
            // Afficher seulement si les conditions sont remplies
            if (shouldDisplay)
            {
                DisplayTradeInfo();
            }
        }

        private void DisplayTradeInfo()
        {
            // Calculer les différentes unités de mesure avec les formules correctes
            double distancePrice = Math.Abs(tradeEntry - tradeSL);
            double distanceInTicks = Math.Abs(tradeEntry - tradeSL) / Symbol.TickSize;
            double riskValue = distanceInTicks * Symbol.TickValue;
            
            // Calculs de money management
            double riskAmount1 = Capital1 * (Risk1 / 100.0);
            double lotSize1 = Math.Round(riskAmount1 / riskValue, 2);
            double tpValue1 = RiskRewardRatio * riskAmount1;
            
            double riskAmount2 = Capital2 * (Risk2 / 100.0);
            double lotSize2 = Math.Round(riskAmount2 / riskValue, 2);
            double tpValue2 = RiskRewardRatio * riskAmount2;
            
            // Construire la chaîne avec les informations de trading
            string tradeInfoText = string.Format("Prix d'entrée: \t{0:F" + Symbol.Digits + "}", tradeEntry);
            tradeInfoText += string.Format("\nSL: \t\t{0:F" + Symbol.Digits + "}", tradeSL);
            tradeInfoText += string.Format("\nTP: \t\t{0:F" + Symbol.Digits + "}", tradeTP);
            tradeInfoText += string.Format("\nDistance SL: \t{0} pips", tradeSLPips);
            tradeInfoText += string.Format("\nDistance SL: \t{0:F0} ticks", distanceInTicks);
            tradeInfoText += string.Format("\nTick Size: \t{0:F" + Symbol.Digits + "}", Symbol.TickSize);
            tradeInfoText += string.Format("\nTick Value: \t{0:F2}", Symbol.TickValue);
            tradeInfoText += string.Format("\n");
            tradeInfoText += string.Format("\nRisque 1 lot: \t{0:F2} {1}", riskValue, Symbol.QuoteAsset);
            tradeInfoText += string.Format("\nR:R Ratio: \t{0:F1}", RiskRewardRatio);
            
            // Ajouter les informations de money management
            tradeInfoText += string.Format("\n");
            tradeInfoText += string.Format("\nCapital1: \t{0:F0} {1}", Capital1, Symbol.QuoteAsset);
            tradeInfoText += string.Format("\nRisk1: \t\t{0:F1}%", Risk1);
            tradeInfoText += string.Format("\nLot1: \t\t{0:F2}", lotSize1);
            tradeInfoText += string.Format("\nTP1: \t\t{0:F2} {1}", tpValue1, Symbol.QuoteAsset);
            
            tradeInfoText += string.Format("\n");
            tradeInfoText += string.Format("\nCapital2: \t{0:F0} {1}", Capital2, Symbol.QuoteAsset);
            tradeInfoText += string.Format("\nRisk2: \t\t{0:F1}%", Risk2);
            tradeInfoText += string.Format("\nLot2: \t\t{0:F2}", lotSize2);
            tradeInfoText += string.Format("\nTP2: \t\t{0:F2} {1}", tpValue2, Symbol.QuoteAsset);
            
            string dashBoard = $"\n\n{tradeInfoText}";
            
            // Afficher les informations en haut à droite du graphique
            Chart.DrawStaticText("TradeInfoDisplay", dashBoard, VerticalAlignment.Bottom, HorizontalAlignment.Left, Color.LightBlue);
        }

        private DateTime ConvertToUtc(DateTime localDateTime)
        {
            try
            {
                // Créer un DateTime "non spécifié" pour éviter les conflits
                DateTime unspecifiedDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
                
                // Convertir du fuseau horaire cible vers UTC
                DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(unspecifiedDateTime, targetTimeZone);
                
                return utcDateTime;
            }
            catch (Exception ex)
            {
                Print($"Erreur lors de la conversion vers UTC pour {localDateTime}: {ex.Message}");
                return localDateTime; // Retourner l'heure locale en cas d'erreur
            }
        }
    }
}