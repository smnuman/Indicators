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
namespace NinjaTrader.NinjaScript.Indicators.NMN
{
	public class WAE_Mod : Indicator
	{
		private NinjaTrader.NinjaScript.Indicators.ATR dzATR;	/// mod - ADD -- Numan
		
		public double DeadZoneBtm, zeroLine = 0.0;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"WAE Mod by Numan to display up bars for Bull market and down bars for Bear market";
				Name										= "WAE_Mod";
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
				Sensitivity						= 150;
				
				FastLength						= 10;
				FastSmooth						= true;
				FastSmoothLength				= 9;
				
				SlowLength						= 30;
				SlowSmooth						= true;
				SlowSmoothLength				= 9;
				
				ChannelLength					= 30;
				Mult							= 2.0;
				DeadZone						= 200;
				
//				DZATRPeriod						= 21;
								
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Bar, "TrendUp");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "TrendDown");
				
				AddPlot(Brushes.BlueViolet, "ExplosionLine");
				AddPlot(Brushes.SaddleBrown, "ExplosionLineDn");
				
//				AddPlot(Brushes.CornflowerBlue, "DeadZoneTop");
//				AddPlot(Brushes.CornflowerBlue, "DeadZoneBtm");
				
			}
			else if (State == State.Configure)
			{
				DeadZoneBtm						= -1 * DeadZone;
				
				AddLine(Brushes.Gray, zeroLine, "zeroLine");
				AddLine(Brushes.Red, DeadZone, "DeadZoneLine");
				AddLine(Brushes.Blue, DeadZoneBtm, "DeadZoneBtmLine");
			}
			else if (State == State.DataLoaded)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if(CurrentBar<5) return;
			
			double t1 	= (calc_macd(0) - calc_macd(1)) * Sensitivity;
			
			ExplosionLine[0] 	= (t1 >= 0) ? ((calc_BBUpper(0, ChannelLength, Mult) - calc_BBLower(0, ChannelLength, Mult))) : 0 ;  /// Mod Numan
			ExplosionLineDn[0] 	= (t1 < 0) ? ((-calc_BBUpper(0, ChannelLength, Mult) + calc_BBLower(0, ChannelLength, Mult))) : 0 ; /// Mod Numan
			
			TrendUp[0] 			= (t1 >= 0) ? t1 : 0;
			TrendDown[0] 		= (t1 < 0) ? t1 : 0; /// Mod by Numan for up down visualisation
//			TrendDown[0] = (t1 < 0) ? (-1*t1) : 0; // original
			
			PlotBrushes[0][0] 	= (TrendUp[0]<TrendUp[1]) ? Brushes.Lime : Brushes.Green ; /// Mod by Numan
			PlotBrushes[1][0] 	= (TrendDown[0]>TrendDown[1])? Brushes.Orange : Brushes.Red;  /// Mod by Numan
			
		}
		
		double calc_macd(int bar)
		{
    		//Code to close working orders
			double fastMA;
			if (FastSmooth == true)
				fastMA = EMA(EMA(FastLength),FastSmoothLength)[bar];
			else
				fastMA = EMA(FastLength)[bar];
			
			double slowMA;
			if (SlowSmooth == true)
				slowMA = EMA(EMA(SlowLength),SlowSmoothLength)[bar];
			else
				slowMA = EMA(SlowLength)[bar];			

			return fastMA - slowMA;
		}
		
		double calc_BBUpper(int source, int length, double mult)
		{
    		//Code to close working orders
			double basis = SMA(length)[source];
			double dev = mult * StdDev(length)[source];
			return basis + dev;
		}
		double calc_BBLower(int source, int length, double mult)
		{
    		//Code to close working orders
			double basis = SMA(length)[source];
			double dev = mult * StdDev(length)[source];
			return basis - dev;
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Sensitivity", Description="Sensitivity", Order=1, GroupName="Parameters")]
		public int Sensitivity
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastLength", Description="FastEMA Length", Order=2, GroupName="Parameters")]
		public int FastLength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="FastSmooth", Description="Smoothen FastEMA", Order=3, GroupName="Parameters")]
		public bool FastSmooth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastSmoothLength", Description="FastEMA Smooth Length", Order=4, GroupName="Parameters")]
		public int FastSmoothLength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowLength", Description="SlowEMA Length", Order=5, GroupName="Parameters")]
		public int SlowLength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="SlowSmooth", Description="Smoothen SlowEMA", Order=6, GroupName="Parameters")]
		public bool SlowSmooth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowSmoothLength", Description="Smoothen SlowEMA Length", Order=7, GroupName="Parameters")]
		public int SlowSmoothLength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ChannelLength", Description="BB Channel Length", Order=8, GroupName="Parameters")]
		public int ChannelLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="Mult", Description="BB Stdev Multiplier", Order=9, GroupName="Parameters")]
		public double Mult
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, int.MaxValue)]
		[Display(Name="DeadZone", Description="No trade zone threshold", Order=10, GroupName="Parameters")]
		public double DeadZone
		{ get; set; }

//		[NinjaScriptProperty]
//		[Range(1, int.MaxValue)]
//		[Display(Name="DZATRPeriod", Description="ATR period for 'No trade'/Deadzone threshold", Order=11, GroupName="Parameters")]
//		public int DZATRPeriod
//		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrendUp
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrendDown
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ExplosionLine
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ExplosionLineDn
		{
			get { return Values[3]; }
		}
		
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> DeadZoneTop
//		{
//			get { return Values[4]; }
//			set { Values[4] = dzATR(DZATRPeriod); }
//		}
		
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> DeadZoneBtm
//		{
//			get { return Values[5]; }
//			set { Values[5] = -dzATR(DZATRPeriod); }
//		}

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private NMN.WAE_Mod[] cacheWAE_Mod;
		public NMN.WAE_Mod WAE_Mod(int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			return WAE_Mod(Input, sensitivity, fastLength, fastSmooth, fastSmoothLength, slowLength, slowSmooth, slowSmoothLength, channelLength, mult, deadZone);
		}

		public NMN.WAE_Mod WAE_Mod(ISeries<double> input, int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			if (cacheWAE_Mod != null)
				for (int idx = 0; idx < cacheWAE_Mod.Length; idx++)
					if (cacheWAE_Mod[idx] != null && cacheWAE_Mod[idx].Sensitivity == sensitivity && cacheWAE_Mod[idx].FastLength == fastLength && cacheWAE_Mod[idx].FastSmooth == fastSmooth && cacheWAE_Mod[idx].FastSmoothLength == fastSmoothLength && cacheWAE_Mod[idx].SlowLength == slowLength && cacheWAE_Mod[idx].SlowSmooth == slowSmooth && cacheWAE_Mod[idx].SlowSmoothLength == slowSmoothLength && cacheWAE_Mod[idx].ChannelLength == channelLength && cacheWAE_Mod[idx].Mult == mult && cacheWAE_Mod[idx].DeadZone == deadZone && cacheWAE_Mod[idx].EqualsInput(input))
						return cacheWAE_Mod[idx];
			return CacheIndicator<NMN.WAE_Mod>(new NMN.WAE_Mod(){ Sensitivity = sensitivity, FastLength = fastLength, FastSmooth = fastSmooth, FastSmoothLength = fastSmoothLength, SlowLength = slowLength, SlowSmooth = slowSmooth, SlowSmoothLength = slowSmoothLength, ChannelLength = channelLength, Mult = mult, DeadZone = deadZone }, input, ref cacheWAE_Mod);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.NMN.WAE_Mod WAE_Mod(int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			return indicator.WAE_Mod(Input, sensitivity, fastLength, fastSmooth, fastSmoothLength, slowLength, slowSmooth, slowSmoothLength, channelLength, mult, deadZone);
		}

		public Indicators.NMN.WAE_Mod WAE_Mod(ISeries<double> input , int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			return indicator.WAE_Mod(input, sensitivity, fastLength, fastSmooth, fastSmoothLength, slowLength, slowSmooth, slowSmoothLength, channelLength, mult, deadZone);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.NMN.WAE_Mod WAE_Mod(int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			return indicator.WAE_Mod(Input, sensitivity, fastLength, fastSmooth, fastSmoothLength, slowLength, slowSmooth, slowSmoothLength, channelLength, mult, deadZone);
		}

		public Indicators.NMN.WAE_Mod WAE_Mod(ISeries<double> input , int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			return indicator.WAE_Mod(input, sensitivity, fastLength, fastSmooth, fastSmoothLength, slowLength, slowSmooth, slowSmoothLength, channelLength, mult, deadZone);
		}
	}
}

#endregion
