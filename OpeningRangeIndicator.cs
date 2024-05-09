// 
// Copyright (C) 2016, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	
	/// <summary>
	/// </summary>
	public class OpeningRangeIndicator : Indicator
	{
		double highestPrice = 0.0;
		double lowestPrice = 0.0;
		bool   bFirst = true;
		double d30minbar = 0.0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "Opening Range Indicator";
				Name						= "Opening Range Indicator";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;

				AddPlot(Brushes.Red,		"Opening Range High");
				AddPlot(Brushes.Red,		"Opening Range Low");
				
			}
			else if (State == State.Configure)
			{
        		AddDataSeries(BarsPeriodType.Minute, 30);
			}
		}

		protected override void OnBarUpdate()
		{	

			if (BarsPeriod.BarsPeriodType == BarsPeriodType.Minute )
   			{
				if (bFirst)
				{
					d30minbar = (30/BarsPeriod.Value)-1;
					bFirst = false;
				}
				
				if (CurrentBar < 20)
				{
					return;
				}
				
				
				if( BarsPeriod.Value == 30)
				{
				    if( Bars.BarsSinceNewTradingDay == 0)
					{
						highestPrice = Highs[1][0];
						lowestPrice = Lows[1][0];
					}
				}
				
				if (Bars.BarsSinceNewTradingDay > d30minbar)
				{
					HighOfRange[0] = highestPrice;
					LowOfRange[0] = lowestPrice;
				}
   			}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HighOfRange
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowOfRange
		{
			get { return Values[1]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OpeningRangeIndicator[] cacheOpeningRangeIndicator;
		public OpeningRangeIndicator OpeningRangeIndicator()
		{
			return OpeningRangeIndicator(Input);
		}

		public OpeningRangeIndicator OpeningRangeIndicator(ISeries<double> input)
		{
			if (cacheOpeningRangeIndicator != null)
				for (int idx = 0; idx < cacheOpeningRangeIndicator.Length; idx++)
					if (cacheOpeningRangeIndicator[idx] != null &&  cacheOpeningRangeIndicator[idx].EqualsInput(input))
						return cacheOpeningRangeIndicator[idx];
			return CacheIndicator<OpeningRangeIndicator>(new OpeningRangeIndicator(), input, ref cacheOpeningRangeIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OpeningRangeIndicator OpeningRangeIndicator()
		{
			return indicator.OpeningRangeIndicator(Input);
		}

		public Indicators.OpeningRangeIndicator OpeningRangeIndicator(ISeries<double> input )
		{
			return indicator.OpeningRangeIndicator(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OpeningRangeIndicator OpeningRangeIndicator()
		{
			return indicator.OpeningRangeIndicator(Input);
		}

		public Indicators.OpeningRangeIndicator OpeningRangeIndicator(ISeries<double> input )
		{
			return indicator.OpeningRangeIndicator(input);
		}
	}
}

#endregion
