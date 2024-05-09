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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class CurrentDayOHLPlotsExample : Indicator
	{
		private double cdOpen;
		private double cdHigh;
		private double cdLow;
		
		protected override void OnStateChange()
		{
			
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "CurrentDayOHLPlotsExample";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				AddPlot(Brushes.DodgerBlue, "CDOpen");
				AddPlot(Brushes.HotPink, "CDHigh");
				AddPlot(Brushes.Yellow, "CDLow");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			cdOpen = CurrentDayOHL().CurrentOpen[0];
			cdHigh = CurrentDayOHL().CurrentHigh[0];
			cdLow  = CurrentDayOHL().CurrentLow[0];
			
			Values[0][0] = cdOpen;
			Values[1][0] = cdHigh;
			Values[2][0] = cdLow;
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CDOpen
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CDHigh
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CDLow
		{
			get { return Values[2]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CurrentDayOHLPlotsExample[] cacheCurrentDayOHLPlotsExample;
		public CurrentDayOHLPlotsExample CurrentDayOHLPlotsExample()
		{
			return CurrentDayOHLPlotsExample(Input);
		}

		public CurrentDayOHLPlotsExample CurrentDayOHLPlotsExample(ISeries<double> input)
		{
			if (cacheCurrentDayOHLPlotsExample != null)
				for (int idx = 0; idx < cacheCurrentDayOHLPlotsExample.Length; idx++)
					if (cacheCurrentDayOHLPlotsExample[idx] != null &&  cacheCurrentDayOHLPlotsExample[idx].EqualsInput(input))
						return cacheCurrentDayOHLPlotsExample[idx];
			return CacheIndicator<CurrentDayOHLPlotsExample>(new CurrentDayOHLPlotsExample(), input, ref cacheCurrentDayOHLPlotsExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CurrentDayOHLPlotsExample CurrentDayOHLPlotsExample()
		{
			return indicator.CurrentDayOHLPlotsExample(Input);
		}

		public Indicators.CurrentDayOHLPlotsExample CurrentDayOHLPlotsExample(ISeries<double> input )
		{
			return indicator.CurrentDayOHLPlotsExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CurrentDayOHLPlotsExample CurrentDayOHLPlotsExample()
		{
			return indicator.CurrentDayOHLPlotsExample(Input);
		}

		public Indicators.CurrentDayOHLPlotsExample CurrentDayOHLPlotsExample(ISeries<double> input )
		{
			return indicator.CurrentDayOHLPlotsExample(input);
		}
	}
}

#endregion
