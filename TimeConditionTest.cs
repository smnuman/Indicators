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
	public class TimeConditionTest : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name										= "TimeConditionTest";
				Calculate									= Calculate.OnBarClose;
			}
		}

		protected override void OnBarUpdate()
		{
			Print(string.Format("{0} | Times[0][0].TimeOfDay: {1} >= new TimeSpan(08, 30, 00): {2} && Times[0][0].TimeOfDay: {1} <= new TimeSpan(17,00, 00): {3}", Time[0], Times[0][0].TimeOfDay, new TimeSpan(08, 30, 00), new TimeSpan(17,00, 00)));
			
			if (Times[0][0].TimeOfDay >= new TimeSpan(08, 30, 00) && Times[0][0].TimeOfDay <= new TimeSpan(17,00, 00))
			{
				Print(Time[0] + " | Condition true");
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TimeConditionTest[] cacheTimeConditionTest;
		public TimeConditionTest TimeConditionTest()
		{
			return TimeConditionTest(Input);
		}

		public TimeConditionTest TimeConditionTest(ISeries<double> input)
		{
			if (cacheTimeConditionTest != null)
				for (int idx = 0; idx < cacheTimeConditionTest.Length; idx++)
					if (cacheTimeConditionTest[idx] != null &&  cacheTimeConditionTest[idx].EqualsInput(input))
						return cacheTimeConditionTest[idx];
			return CacheIndicator<TimeConditionTest>(new TimeConditionTest(), input, ref cacheTimeConditionTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TimeConditionTest TimeConditionTest()
		{
			return indicator.TimeConditionTest(Input);
		}

		public Indicators.TimeConditionTest TimeConditionTest(ISeries<double> input )
		{
			return indicator.TimeConditionTest(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TimeConditionTest TimeConditionTest()
		{
			return indicator.TimeConditionTest(Input);
		}

		public Indicators.TimeConditionTest TimeConditionTest(ISeries<double> input )
		{
			return indicator.TimeConditionTest(input);
		}
	}
}

#endregion
