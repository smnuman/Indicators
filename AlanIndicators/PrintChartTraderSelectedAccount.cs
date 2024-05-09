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
namespace NinjaTrader.NinjaScript.Indicators.AlanIndicators
{
	public class PrintChartTraderSelectedAccount : Indicator
	{

		NinjaTrader.Gui.Tools.AccountSelector xAlselector;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "PrintChartTraderSelectedAccount";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{

            }
		
			
		}
	

		protected override void OnBarUpdate()
		{
			ChartControl.Dispatcher.InvokeAsync((Action)(() =>
			{
						//You have to put the stuff below within this ChartControl.Dispatcher.InvokeAsync((Action)(() =>, because you are trying to access something on a different thread.
						xAlselector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
						Print(xAlselector.SelectedAccount.ToString());
			}));
		
	
		}
	
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlanIndicators.PrintChartTraderSelectedAccount[] cachePrintChartTraderSelectedAccount;
		public AlanIndicators.PrintChartTraderSelectedAccount PrintChartTraderSelectedAccount()
		{
			return PrintChartTraderSelectedAccount(Input);
		}

		public AlanIndicators.PrintChartTraderSelectedAccount PrintChartTraderSelectedAccount(ISeries<double> input)
		{
			if (cachePrintChartTraderSelectedAccount != null)
				for (int idx = 0; idx < cachePrintChartTraderSelectedAccount.Length; idx++)
					if (cachePrintChartTraderSelectedAccount[idx] != null &&  cachePrintChartTraderSelectedAccount[idx].EqualsInput(input))
						return cachePrintChartTraderSelectedAccount[idx];
			return CacheIndicator<AlanIndicators.PrintChartTraderSelectedAccount>(new AlanIndicators.PrintChartTraderSelectedAccount(), input, ref cachePrintChartTraderSelectedAccount);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlanIndicators.PrintChartTraderSelectedAccount PrintChartTraderSelectedAccount()
		{
			return indicator.PrintChartTraderSelectedAccount(Input);
		}

		public Indicators.AlanIndicators.PrintChartTraderSelectedAccount PrintChartTraderSelectedAccount(ISeries<double> input )
		{
			return indicator.PrintChartTraderSelectedAccount(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlanIndicators.PrintChartTraderSelectedAccount PrintChartTraderSelectedAccount()
		{
			return indicator.PrintChartTraderSelectedAccount(Input);
		}

		public Indicators.AlanIndicators.PrintChartTraderSelectedAccount PrintChartTraderSelectedAccount(ISeries<double> input )
		{
			return indicator.PrintChartTraderSelectedAccount(input);
		}
	}
}

#endregion
