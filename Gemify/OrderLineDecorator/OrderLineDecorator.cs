#region Using declarations
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Account = NinjaTrader.Cbi.Account;
using Order = NinjaTrader.Cbi.Order;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Gemify
{
    public class OrderLineDecorator : Indicator
    {
        // ---------------------------------------
        // Update this list as you see fit
        // to support additional order types
        // categorized as either STOP or TARGET
        // ---------------------------------------
        private List<OrderType> RecognizedStopOrderTypes = new List<OrderType>() { OrderType.StopMarket, OrderType.StopLimit, OrderType.MIT };
        private List<OrderType> RecognizedTargetOrderTypes = new List<OrderType>() { OrderType.Limit };

        // For simplicity, we'll bunch orders as either stops or targets
        protected enum OrderLineDecoratorOrderType
        {
            STOP,
            TARGET,
            OTHER
        }

        class OrderTypeAndText
        {
            public OrderLineDecoratorOrderType orderType;
            public String text;
            public int nOrderTypes;
        }

        private Account gAccount;
        private AccountSelector gAccountSelector;
        private ConcurrentDictionary<string, int> orderQtyTracker;
        private ConcurrentDictionary<double, OrderTypeAndText> toRender;

        private bool isSubscribed;

        private const String sampleOrderLabelText = "XXXXXXXX XXX XXXX";
        private SimpleFont chartFont;
        private float sampleOrderLabelTextWidth;

        private Chart chartWindow;
        private bool gIsChartTraderOn;
        private bool chartTraderVisibilitySubscribed;
        private SharpDX.DirectWrite.TextFormat textFormat;

        private bool IsDebug;

        private Object _lock;
        private void Debug (String message, params object[] args) 
        {
            if (IsDebug) Print(String.Format("{0} : {1} : {2}", this.Name, DateTime.Now, String.Format(message, args)));
        }
        private void Debug(String message)
        {
            Debug(message, new object[0]);
        }

        protected override void OnStateChange()
        {
            Debug(">>>>>>> " + State);

            if (State == State.SetDefaults)
            {
                Description = @"Order Line Decorator";
                Name = "\"OrderLineDecorator\"";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                // Default values
                IsDebug = false;
                isSubscribed = false;
                DisplayTicks = true;
                DisplayCurrency = true;
                DisplayPoints = false;
                DisplayPercentOfAccount = true;

                chartFont = null;

                // This kludge is required to place the decorator UI element (box) 
                // just enough to the left of the built-in order element.
                // There's got to be a way to find that order UI element, determine it's X position
                // and then draw our decorator element to its immediate left.
                // Task for the reader :D
                FlexGapWidth = 90;

                StopFillBrush = Brushes.Maroon;
                TargetFillBrush = Brushes.DarkGreen;
                OutlineBrush = Brushes.AliceBlue;
                TextBrush = Brushes.White;

                chartTraderVisibilitySubscribed = false;

            }
            else if (State == State.Configure)
            {
                _lock = new object();

                orderQtyTracker = new ConcurrentDictionary<string, int>();
                toRender = new ConcurrentDictionary<double, OrderTypeAndText>();

                lock (Account.All)
                {
                    foreach (Account a in Account.All)
                    {
                        a.AccountItemUpdate += OnAccountItemUpdate;
                        a.OrderUpdate += OnOrderUpdate;
                        isSubscribed = true;
                    }
                }

                gIsChartTraderOn = IsChartTraderOn();

            }
            else if (State == State.Realtime)
            {
                ComputeValues();
                ForceRefresh();
            }
            else if (State == State.Terminated)
            {
                if (chartTraderVisibilitySubscribed && chartWindow != null)
                {
                    chartWindow.IsVisibleChanged -= OnChartTraderVisibilityChanged;
                }

                if (isSubscribed)
                {
                    foreach (Account a in Account.All)
                    {
                        a.AccountItemUpdate -= OnAccountItemUpdate;
                        a.OrderUpdate -= OnOrderUpdate;
                        isSubscribed = false;
                    }
                }
            }
        }

        private bool IsChartTraderOn()
        {
            bool chartTraderOn = false;

            ChartControl.Dispatcher.Invoke((Action)(() =>
            {
                // Main chart window
                if (chartWindow == null)
                {
                    chartWindow = System.Windows.Window.GetWindow(ChartControl.Parent) as Chart;
                }

                // If we can't find the main window, we might as well go home :)
                if (chartWindow != null)
                {
                    // If we haven't subscribed to the visibility event, do so.
                    if (!chartTraderVisibilitySubscribed)
                    {
                        chartWindow.ChartTrader.IsVisibleChanged += OnChartTraderVisibilityChanged;
                        chartTraderVisibilitySubscribed = true;
                    }
                    
                    chartTraderOn = chartWindow.ChartTrader.IsVisible;
                }

            }));

            return chartTraderOn;
        }

        private void OnChartTraderVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            gIsChartTraderOn = (bool)e.NewValue;            
            ComputeValues();
            ForceRefresh();
        }

        private float CalculateLabelSize(SimpleFont font, String label)
        {
            SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
            return new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, label, textFormat, ChartPanel.X + ChartPanel.W, textFormat.FontSize).Metrics.Width;
        }

        private void SetTextFont(SimpleFont font)
        {
            chartFont = font;
            textFormat = chartFont.ToDirectWriteTextFormat();
        }

        private void OnOrderUpdate(object Sender, OrderEventArgs e)
        {
            // Attempting to optimize for just order states that matter
            // Dunno - maybe other states make sense as well 
            if ((e.OrderState != OrderState.Accepted &&
                e.OrderState != OrderState.Working &&
                e.OrderState != OrderState.Filled &&
                e.OrderState != OrderState.PartFilled)
                )
            {
                return;
            }

            ComputeValues();
        }

        private void OnAccountItemUpdate(object Sender, AccountItemEventArgs E)
        {
            // Attempting to optimize for just account items that matter
            if (E.AccountItem != AccountItem.UnrealizedProfitLoss)
            {
                return;
            }

            ComputeValues();
        }

        protected override void OnBarUpdate()
        {
            // Compute Data Values (ex. prices, value etc)
            ComputeValues();
        }

        private void ComputeValues()
        {
            lock (_lock)
            {
                // Reset order text and positions
                orderQtyTracker.Clear();
                toRender.Clear();

                // We only care about realtime positions
                if (State == State.Historical || !gIsChartTraderOn) return;

                // Get the account for which we're monitoring positions
                ChartControl.Dispatcher.InvokeAsync((Action)(() =>
                {
                    gAccountSelector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
                    gAccount = gAccountSelector.SelectedAccount;
                }));

                // Nothing to do if we can't find the selected account
                if (gAccount == null) return;
                
                // Process only if we have positions
                foreach (Position p in gAccount.Positions)
                {
                    // If the position is in current instrument
                    if (p.Instrument == Instrument && p.MarketPosition != MarketPosition.Flat)
                    {
                        // Get our position size and entry price
                        double entryPrice = p.AveragePrice;
                        double positionSize = p.Quantity;

                        Debug("Found position at {0} with size {1}", entryPrice, positionSize);

                        // Check every order in selected account
                        foreach (Order order in gAccount.Orders)
                        {
                            // Ignore order if it's for a different instrument
                            if (order.Instrument != Instrument) continue;

                            Debug("Order state : {0}", order.OrderState);

                            // We're only concerned with "Accepted" / "Working" orders
                            if ((order.OrderState == OrderState.Accepted || order.OrderState == OrderState.Working) &&
                                // We're only concerned with Stop Loss and Target orders (ie, ignore scale-in orders)
                                (p.MarketPosition == MarketPosition.Long && !order.IsLong || p.MarketPosition == MarketPosition.Short && !order.IsShort))
                            {
                                // Only considering stop price at this time
                                double orderPrice = GetOrderPrice(order);
                                if (orderPrice == 0)
                                {
                                    Debug("Unsupported order type. [" + order.OrderType + "] Skipping.");
                                    continue;
                                }

                                Debug("Order type {0}", order.OrderType);
                                string key = orderPrice + GetOrderLineDecoratorOrderType(order.OrderType).ToString();

                                int orderQty = 0;

                                Debug("Looking for key in orderQtyTracker {0}", key);

                                // Attempt to count orders of the same type and same price 
                                if (orderQtyTracker.ContainsKey(key))
                                {
                                    Debug("Found key in tracker. Updating {0} orders by {1}", orderQtyTracker[key], order.Quantity);
                                    orderQtyTracker[key] += order.Quantity;
                                    // Use aggregated order quantity
                                    orderQty = orderQtyTracker[key];
                                    Debug("Key {0} now has {1} orders", key, orderQty);
                                }
                                else
                                {
                                    orderQtyTracker.TryAdd(key, order.Quantity);
                                    orderQty = order.Quantity;
                                    Debug("Key wasn't found in tracker. Adding new entry with {0} orders.", orderQty);
                                }

                                // Calculate ticks and currency value from entry
                                double priceDiff = (p.MarketPosition == MarketPosition.Long ? orderPrice - entryPrice : entryPrice - orderPrice);
                                int ticks = (int)Instrument.MasterInstrument.RoundToTickSize(priceDiff / TickSize);
                                double points = Instrument.MasterInstrument.RoundToTickSize(priceDiff * orderQty);
                                double currencyValue = priceDiff * Instrument.MasterInstrument.PointValue * orderQty;

                                double accCashValue = gAccount.GetAccountItem(AccountItem.CashValue, Currency.UsDollar).Value;

                                DisplayTicks = DisplayTicks && (DisplayTicks || DisplayPoints || DisplayCurrency);

                                // Generate text for decoration
                                string orderType = IsStopOrder(order) ? "STOP" : "TARGET";
                                string text = orderType + " (" + orderQty + ")" +
                                    (DisplayTicks ? "  :  " : "") +
                                    (DisplayTicks ? (IsStopOrder(order) && ticks > 0 ? "+" : "") + ticks + " T" : "") +
                                    (DisplayPoints ? "  :  " : "") +
                                    (DisplayPoints ? (IsStopOrder(order) && points > 0 ? "+" : "") + points + " P" : "") +
                                    (DisplayCurrency ? "  :  " : "") +
                                    (DisplayCurrency ? currencyValue.ToString("C2") : "") +
                                    (DisplayPercentOfAccount ? "  :  " : "") +
                                    (DisplayPercentOfAccount ? (currencyValue / accCashValue).ToString("P2") : "");

                                Debug("Text in toRender should be: {0}", text);

                                // Store order type and text against order price. This will be picked up and rendered by the OnRender call
                                OrderTypeAndText item = new OrderTypeAndText() { orderType = GetOrderLineDecoratorOrderType(order.OrderType), text = text };
                                toRender.AddOrUpdate(orderPrice, item, (k, v) => item);                                
                            }
                        }
                    }
                }
            }

            // Request a refresh
            ForceRefresh();

        }

        private bool IsStopOrder(Order order)
        {
            return RecognizedStopOrderTypes.Contains(order.OrderType);
        }

        private bool IsTargetOrder(Order order)
        {
            return RecognizedTargetOrderTypes.Contains(order.OrderType);
        }

        private OrderLineDecoratorOrderType GetOrderLineDecoratorOrderType(OrderType orderType)
        {
            if (RecognizedStopOrderTypes.Contains(orderType))
            {
                return OrderLineDecoratorOrderType.STOP;
            }
            else if (RecognizedTargetOrderTypes.Contains(orderType))
            {
                return OrderLineDecoratorOrderType.TARGET;
            }
            else
                return OrderLineDecoratorOrderType.OTHER;
        }

        private double GetOrderPrice(Order order)
        {
            double orderPrice = 0;

            // Only operates on recognized stop types
            if (RecognizedStopOrderTypes.Contains(order.OrderType)) orderPrice = order.StopPrice;
            // and target exits
            else if (RecognizedTargetOrderTypes.Contains(order.OrderType)) orderPrice = order.LimitPrice;

            return orderPrice;

        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            if (State == State.Historical || !gIsChartTraderOn || toRender.IsNullOrEmpty()) return;

            if (ChartControl.Properties.LabelFont != chartFont)
            {
                SetTextFont(ChartControl.Properties.LabelFont);
                sampleOrderLabelTextWidth = CalculateLabelSize(chartFont, sampleOrderLabelText);
            }

            using (SharpDX.Direct2D1.Brush borderBrushDx = OutlineBrush.ToDxBrush(RenderTarget))
            using (SharpDX.Direct2D1.Brush stopBrushDx = StopFillBrush.ToDxBrush(RenderTarget))
            using (SharpDX.Direct2D1.Brush targetBrushDx = TargetFillBrush.ToDxBrush(RenderTarget))
            using (SharpDX.Direct2D1.Brush textBrushDx = TextBrush.ToDxBrush(RenderTarget))
            {
                foreach (KeyValuePair<double, OrderTypeAndText> kvp in toRender)
                {
                    SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, kvp.Value.text + "", textFormat, ChartPanel.X + ChartPanel.W, textFormat.FontSize);

                    float textWidth = textLayout.Metrics.Width;
                    float textHeight = textLayout.Metrics.Height;

                    float x = (float)(ChartPanel.W - ((ChartPanel.W * (ChartControl.OwnerChart.ChartTrader.Properties.OrderDisplayBarLength / 100.0)) + textWidth + sampleOrderLabelTextWidth + FlexGapWidth));
                    int priceCoordinate = chartScale.GetYByValue(kvp.Key);
                    float y = priceCoordinate - ((textHeight + 7) / 2);

                    SharpDX.Vector2 startPoint = new SharpDX.Vector2(x, y);
                    SharpDX.Vector2 upperTextPoint = new SharpDX.Vector2(startPoint.X + 4, startPoint.Y + 3);
                    SharpDX.Vector2 lineStartPoint = new SharpDX.Vector2(startPoint.X + textWidth + 9, priceCoordinate);
                    SharpDX.Vector2 lineEndPoint = new SharpDX.Vector2(ChartPanel.W, priceCoordinate);

                    SharpDX.RectangleF rect = new SharpDX.RectangleF(startPoint.X, startPoint.Y, textWidth + 8, textHeight + 6);
                    RenderTarget.FillRectangle(rect, kvp.Value.orderType == OrderLineDecoratorOrderType.STOP ? stopBrushDx : targetBrushDx);
                    RenderTarget.DrawRectangle(rect, borderBrushDx, 1);
                    RenderTarget.DrawLine(lineStartPoint, lineEndPoint, borderBrushDx);

                    RenderTarget.DrawTextLayout(upperTextPoint, textLayout, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                    textLayout.Dispose();
                }

            }
        }

        #region Parameters

        [NinjaScriptProperty]
        [Display(Name = "Ticks", GroupName = "Display", Order = 100)]
        public bool DisplayTicks
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Points", GroupName = "Display", Order = 200)]
        public bool DisplayPoints
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Currency", GroupName = "Display", Order = 300)]
        public bool DisplayCurrency
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Percent (of Account Value)", GroupName = "Display", Order = 400)]
        public bool DisplayPercentOfAccount
        { get; set; }

        // Kludge-alert! Need a better (WPF) way to determine _where_ the order line ends.
        // In the interim, this kludge will keep this functional
        [NinjaScriptProperty]
        [Display(Name = "Decorator : Order Gap", Description = "Adjust the gap between the decorator and the order UI element.", GroupName = "UI Adjustments", Order = 100)]
        public int FlexGapWidth
        { get; set; }



        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Stop Fill Color", Order = 100, GroupName = "Colors")]
        public Brush StopFillBrush
        { get; set; }

        [Browsable(false)]
        public string StopFillBrushSerializable
        {
            get { return Serialize.BrushToString(StopFillBrush); }
            set { StopFillBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Target Fill Color", Order = 200, GroupName = "Colors")]
        public Brush TargetFillBrush
        { get; set; }

        [Browsable(false)]
        public string TargetFillBrushSerializable
        {
            get { return Serialize.BrushToString(TargetFillBrush); }
            set { TargetFillBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Outline Color", Order = 300, GroupName = "Colors")]
        public Brush OutlineBrush
        { get; set; }

        [Browsable(false)]
        public string OutlineBrushSerializable
        {
            get { return Serialize.BrushToString(OutlineBrush); }
            set { OutlineBrush = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Text Color", Order = 400, GroupName = "Colors")]
        public Brush TextBrush
        { get; set; }

        [Browsable(false)]
        public string TextBrushSerializable
        {
            get { return Serialize.BrushToString(TextBrush); }
            set { TextBrush = Serialize.StringToBrush(value); }
        }

        #endregion
    }

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Gemify.OrderLineDecorator[] cacheOrderLineDecorator;
		public Gemify.OrderLineDecorator OrderLineDecorator(bool displayTicks, bool displayPoints, bool displayCurrency, bool displayPercentOfAccount, int flexGapWidth, Brush stopFillBrush, Brush targetFillBrush, Brush outlineBrush, Brush textBrush)
		{
			return OrderLineDecorator(Input, displayTicks, displayPoints, displayCurrency, displayPercentOfAccount, flexGapWidth, stopFillBrush, targetFillBrush, outlineBrush, textBrush);
		}

		public Gemify.OrderLineDecorator OrderLineDecorator(ISeries<double> input, bool displayTicks, bool displayPoints, bool displayCurrency, bool displayPercentOfAccount, int flexGapWidth, Brush stopFillBrush, Brush targetFillBrush, Brush outlineBrush, Brush textBrush)
		{
			if (cacheOrderLineDecorator != null)
				for (int idx = 0; idx < cacheOrderLineDecorator.Length; idx++)
					if (cacheOrderLineDecorator[idx] != null && cacheOrderLineDecorator[idx].DisplayTicks == displayTicks && cacheOrderLineDecorator[idx].DisplayPoints == displayPoints && cacheOrderLineDecorator[idx].DisplayCurrency == displayCurrency && cacheOrderLineDecorator[idx].DisplayPercentOfAccount == displayPercentOfAccount && cacheOrderLineDecorator[idx].FlexGapWidth == flexGapWidth && cacheOrderLineDecorator[idx].StopFillBrush == stopFillBrush && cacheOrderLineDecorator[idx].TargetFillBrush == targetFillBrush && cacheOrderLineDecorator[idx].OutlineBrush == outlineBrush && cacheOrderLineDecorator[idx].TextBrush == textBrush && cacheOrderLineDecorator[idx].EqualsInput(input))
						return cacheOrderLineDecorator[idx];
			return CacheIndicator<Gemify.OrderLineDecorator>(new Gemify.OrderLineDecorator(){ DisplayTicks = displayTicks, DisplayPoints = displayPoints, DisplayCurrency = displayCurrency, DisplayPercentOfAccount = displayPercentOfAccount, FlexGapWidth = flexGapWidth, StopFillBrush = stopFillBrush, TargetFillBrush = targetFillBrush, OutlineBrush = outlineBrush, TextBrush = textBrush }, input, ref cacheOrderLineDecorator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Gemify.OrderLineDecorator OrderLineDecorator(bool displayTicks, bool displayPoints, bool displayCurrency, bool displayPercentOfAccount, int flexGapWidth, Brush stopFillBrush, Brush targetFillBrush, Brush outlineBrush, Brush textBrush)
		{
			return indicator.OrderLineDecorator(Input, displayTicks, displayPoints, displayCurrency, displayPercentOfAccount, flexGapWidth, stopFillBrush, targetFillBrush, outlineBrush, textBrush);
		}

		public Indicators.Gemify.OrderLineDecorator OrderLineDecorator(ISeries<double> input , bool displayTicks, bool displayPoints, bool displayCurrency, bool displayPercentOfAccount, int flexGapWidth, Brush stopFillBrush, Brush targetFillBrush, Brush outlineBrush, Brush textBrush)
		{
			return indicator.OrderLineDecorator(input, displayTicks, displayPoints, displayCurrency, displayPercentOfAccount, flexGapWidth, stopFillBrush, targetFillBrush, outlineBrush, textBrush);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Gemify.OrderLineDecorator OrderLineDecorator(bool displayTicks, bool displayPoints, bool displayCurrency, bool displayPercentOfAccount, int flexGapWidth, Brush stopFillBrush, Brush targetFillBrush, Brush outlineBrush, Brush textBrush)
		{
			return indicator.OrderLineDecorator(Input, displayTicks, displayPoints, displayCurrency, displayPercentOfAccount, flexGapWidth, stopFillBrush, targetFillBrush, outlineBrush, textBrush);
		}

		public Indicators.Gemify.OrderLineDecorator OrderLineDecorator(ISeries<double> input , bool displayTicks, bool displayPoints, bool displayCurrency, bool displayPercentOfAccount, int flexGapWidth, Brush stopFillBrush, Brush targetFillBrush, Brush outlineBrush, Brush textBrush)
		{
			return indicator.OrderLineDecorator(input, displayTicks, displayPoints, displayCurrency, displayPercentOfAccount, flexGapWidth, stopFillBrush, targetFillBrush, outlineBrush, textBrush);
		}
	}
}

#endregion
