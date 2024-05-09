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
	public class DStochZeroLag : Indicator
	{
		private DoubleStochastics DS;
		private bool bStochRising;		
		private double aa;
		private double bb;
		private double CB;
		private double CC;
		private double CA;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "DStochZeroLag";
				Calculate									= Calculate.OnEachTick;
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
				DSTLen										= 10;
				PriceactionFilter							= 5;
				AddPlot(Brushes.Aqua, "ZeroLag");
				AddLine(Brushes.Blue, 90, "Upper");
				AddLine(Brushes.Blue, 10, "Lower");
				
			}
			else if (State == State.Configure)
			{
				Lines[0].DashStyleHelper = DashStyleHelper.Dash;
				Lines[1].DashStyleHelper = DashStyleHelper.Dash;
				
				DS = DoubleStochastics(Input,DSTLen);
			}
			else if (State == State.DataLoaded)
			{				
				bStochRising = false;
				
				aa = Math.Exp(((-1*Math.Sqrt(2))*Math.PI) / PriceactionFilter);
				bb = 2*aa*Math.Cos((Math.Sqrt(2)*180) / PriceactionFilter);
				CB = bb;
				CC = -aa*aa;
				CA = 1 - CB - CC;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar>2)
			{			
				ZeroLag[0] = (CA*DS[0]) + (CB*DS[1]) + (CC*DS[2]);
				
				if (CurrentBar>3)
				{
					if (IsFalling(ZeroLag))
					{
						PlotBrushes[0][0] = Brushes.Fuchsia;
						
						if (bStochRising)
							PlotBrushes[0][0] = Brushes.Aqua;
						
						bStochRising = false;
					}
					else
					{
						PlotBrushes[0][0] = Brushes.Aqua;
						
						if (!bStochRising)
							PlotBrushes[0][0] = Brushes.Fuchsia;
						
						bStochRising = true;
					}				
				}
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="DSTLen", Order=1, GroupName="Parameters")]
		public int DSTLen
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PriceactionFilter", Order=2, GroupName="Parameters")]
		public int PriceactionFilter
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ZeroLag
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DStochZeroLag[] cacheDStochZeroLag;
		public DStochZeroLag DStochZeroLag(int dSTLen, int priceactionFilter)
		{
			return DStochZeroLag(Input, dSTLen, priceactionFilter);
		}

		public DStochZeroLag DStochZeroLag(ISeries<double> input, int dSTLen, int priceactionFilter)
		{
			if (cacheDStochZeroLag != null)
				for (int idx = 0; idx < cacheDStochZeroLag.Length; idx++)
					if (cacheDStochZeroLag[idx] != null && cacheDStochZeroLag[idx].DSTLen == dSTLen && cacheDStochZeroLag[idx].PriceactionFilter == priceactionFilter && cacheDStochZeroLag[idx].EqualsInput(input))
						return cacheDStochZeroLag[idx];
			return CacheIndicator<DStochZeroLag>(new DStochZeroLag(){ DSTLen = dSTLen, PriceactionFilter = priceactionFilter }, input, ref cacheDStochZeroLag);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DStochZeroLag DStochZeroLag(int dSTLen, int priceactionFilter)
		{
			return indicator.DStochZeroLag(Input, dSTLen, priceactionFilter);
		}

		public Indicators.DStochZeroLag DStochZeroLag(ISeries<double> input , int dSTLen, int priceactionFilter)
		{
			return indicator.DStochZeroLag(input, dSTLen, priceactionFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DStochZeroLag DStochZeroLag(int dSTLen, int priceactionFilter)
		{
			return indicator.DStochZeroLag(Input, dSTLen, priceactionFilter);
		}

		public Indicators.DStochZeroLag DStochZeroLag(ISeries<double> input , int dSTLen, int priceactionFilter)
		{
			return indicator.DStochZeroLag(input, dSTLen, priceactionFilter);
		}
	}
}

#endregion
