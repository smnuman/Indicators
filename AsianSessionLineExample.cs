#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	public class AsianSessionLineExample : Indicator
	{
		DateTime AsiaSessionBeginTime;
		DateTime AsiaSessionEndTime;
		DateTime AsiaSessionEndTimeLine;

		double AsiaSessionMax;
		double AsiaSessionMin;

		DateTime AsiaSessionTime = DateTime.Parse("14:00", System.Globalization.CultureInfo.InvariantCulture) ;
		DateTime EuropeSessionTime = DateTime.Parse("03:00", System.Globalization.CultureInfo.InvariantCulture);
		private int BeginBar;
		private double ClosePrice;
		private string LineTag;
		private string RectangleTag;
		private bool TagsSet;

		private SessionIterator sessionIterator;

		protected override void OnStateChange()
		{
			
			if (State == State.SetDefaults)
			{
				Description = @".";
				Name = "AsiaSessionLineExample";
				Calculate = Calculate.OnBarClose;
				DisplayInDataBox = false;
				DrawOnPricePanel = true;
				DrawHorizontalGridLines = true;
				DrawVerticalGridLines = true;
				IsOverlay = true;
				IsAutoScale = false;
				IsSuspendedWhileInactive = true;
				PaintPriceMarkers = false;
				ScaleJustification = ScaleJustification.Right;
				TagsSet = false;
				
			}
			else if (State == State.DataLoaded)
			{
				sessionIterator = new SessionIterator(Bars);
				
			}
		}

		protected override void OnBarUpdate()
		{
			Print(Time[0] + " Current Bar: " + CurrentBar);
			DateTime priortradingDay = sessionIterator.ActualTradingDayExchange;
			
			Print("PriorTradingDay: " + priortradingDay);
			
			AsiaSessionTime = priortradingDay + AsiaSessionTime.TimeOfDay;
			Print("AsiaSessionTime: " + AsiaSessionTime);
			AsiaSessionBeginTime = AsiaSessionTime.AddMinutes(BarsPeriod.Value);
			Print("AsiaSessionBeginTime: " + AsiaSessionBeginTime);

			if (Bars.IsFirstBarOfSession)
			{
				sessionIterator.GetNextSession(Time[0], true);
				DateTime tradingDay = sessionIterator.ActualTradingDayExchange;

				DateTime NewAsiaSessionStartTime = tradingDay + AsiaSessionTime.TimeOfDay;
				AsiaSessionEndTimeLine = NewAsiaSessionStartTime.AddMinutes(BarsPeriod.Value);

				EuropeSessionTime = tradingDay + EuropeSessionTime.TimeOfDay;
				AsiaSessionEndTime = EuropeSessionTime.AddMinutes(BarsPeriod.Value);
			}

			if(Time[0] == AsiaSessionBeginTime)
			{
				ClosePrice = Close[0];
				BeginBar = CurrentBar;
				AsiaSessionMax = High[0];
				AsiaSessionMin = Low[0];
				LineTag = "AmericaCloseLine " + Time[0];
				RectangleTag = "AsiaRange" + Time[0];
				TagsSet = true;
			}
			if (High[0] > AsiaSessionMax)
			{
			AsiaSessionMax = High[0];
			}
			if (Low[0] < AsiaSessionMin)
			{
			AsiaSessionMin = Low[0];
			}
			if (TagsSet)
			{
				Draw.Line(this, LineTag, false, CurrentBar - BeginBar, ClosePrice, 0, ClosePrice, Brushes.Gold, DashStyleHelper.Dash, 2);
			
				if(Time[0] > AsiaSessionBeginTime || Time[0] < AsiaSessionEndTime)
				{
					Draw.Rectangle(this, RectangleTag, false, CurrentBar - BeginBar, AsiaSessionMax, 0, AsiaSessionMin, Brushes.Transparent, Brushes.Gray, 15);
				}
				
			}
			
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AsianSessionLineExample[] cacheAsianSessionLineExample;
		public AsianSessionLineExample AsianSessionLineExample()
		{
			return AsianSessionLineExample(Input);
		}

		public AsianSessionLineExample AsianSessionLineExample(ISeries<double> input)
		{
			if (cacheAsianSessionLineExample != null)
				for (int idx = 0; idx < cacheAsianSessionLineExample.Length; idx++)
					if (cacheAsianSessionLineExample[idx] != null &&  cacheAsianSessionLineExample[idx].EqualsInput(input))
						return cacheAsianSessionLineExample[idx];
			return CacheIndicator<AsianSessionLineExample>(new AsianSessionLineExample(), input, ref cacheAsianSessionLineExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AsianSessionLineExample AsianSessionLineExample()
		{
			return indicator.AsianSessionLineExample(Input);
		}

		public Indicators.AsianSessionLineExample AsianSessionLineExample(ISeries<double> input )
		{
			return indicator.AsianSessionLineExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AsianSessionLineExample AsianSessionLineExample()
		{
			return indicator.AsianSessionLineExample(Input);
		}

		public Indicators.AsianSessionLineExample AsianSessionLineExample(ISeries<double> input )
		{
			return indicator.AsianSessionLineExample(input);
		}
	}
}

#endregion
