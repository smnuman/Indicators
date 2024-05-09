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
namespace NinjaTrader.NinjaScript.Indicators.Lo
{
	public class WaddahAttarExplosion : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "WaddahAttarExplosion";
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
				
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Bar, "TrendUp");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "TrendDown");
				AddPlot(Brushes.SaddleBrown, "ExplosionLine");
				
			}
			else if (State == State.Configure)
			{
				AddLine(Brushes.Blue, DeadZone, "DeadZoneLine");
			}
			else if (State == State.DataLoaded)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if(CurrentBar<5) return;
			
			double t1 = (calc_macd(0) - calc_macd(1))*Sensitivity;
			
			ExplosionLine[0] = (calc_BBUpper(0, ChannelLength, Mult) - calc_BBLower(0, ChannelLength, Mult));
			
			TrendUp[0] = (t1 >= 0) ? t1 : 0;
			TrendDown[0] = (t1 < 0) ? (-1*t1) : 0;
			
			if(TrendUp[0]<TrendUp[1])
			{PlotBrushes[0][0] = Brushes.Lime;}
			else
			{PlotBrushes[0][0] = Brushes.Green;}
			

			if(TrendDown[0]<TrendDown[1])
			{PlotBrushes[1][0] = Brushes.Orange;}
			else
			{PlotBrushes[1][0] = Brushes.Red;}

			
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
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Lo.WaddahAttarExplosion[] cacheWaddahAttarExplosion;
		public Lo.WaddahAttarExplosion WaddahAttarExplosion(int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			return WaddahAttarExplosion(Input, sensitivity, fastLength, fastSmooth, fastSmoothLength, slowLength, slowSmooth, slowSmoothLength, channelLength, mult, deadZone);
		}

		public Lo.WaddahAttarExplosion WaddahAttarExplosion(ISeries<double> input, int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			if (cacheWaddahAttarExplosion != null)
				for (int idx = 0; idx < cacheWaddahAttarExplosion.Length; idx++)
					if (cacheWaddahAttarExplosion[idx] != null && cacheWaddahAttarExplosion[idx].Sensitivity == sensitivity && cacheWaddahAttarExplosion[idx].FastLength == fastLength && cacheWaddahAttarExplosion[idx].FastSmooth == fastSmooth && cacheWaddahAttarExplosion[idx].FastSmoothLength == fastSmoothLength && cacheWaddahAttarExplosion[idx].SlowLength == slowLength && cacheWaddahAttarExplosion[idx].SlowSmooth == slowSmooth && cacheWaddahAttarExplosion[idx].SlowSmoothLength == slowSmoothLength && cacheWaddahAttarExplosion[idx].ChannelLength == channelLength && cacheWaddahAttarExplosion[idx].Mult == mult && cacheWaddahAttarExplosion[idx].DeadZone == deadZone && cacheWaddahAttarExplosion[idx].EqualsInput(input))
						return cacheWaddahAttarExplosion[idx];
			return CacheIndicator<Lo.WaddahAttarExplosion>(new Lo.WaddahAttarExplosion(){ Sensitivity = sensitivity, FastLength = fastLength, FastSmooth = fastSmooth, FastSmoothLength = fastSmoothLength, SlowLength = slowLength, SlowSmooth = slowSmooth, SlowSmoothLength = slowSmoothLength, ChannelLength = channelLength, Mult = mult, DeadZone = deadZone }, input, ref cacheWaddahAttarExplosion);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Lo.WaddahAttarExplosion WaddahAttarExplosion(int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			return indicator.WaddahAttarExplosion(Input, sensitivity, fastLength, fastSmooth, fastSmoothLength, slowLength, slowSmooth, slowSmoothLength, channelLength, mult, deadZone);
		}

		public Indicators.Lo.WaddahAttarExplosion WaddahAttarExplosion(ISeries<double> input , int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			return indicator.WaddahAttarExplosion(input, sensitivity, fastLength, fastSmooth, fastSmoothLength, slowLength, slowSmooth, slowSmoothLength, channelLength, mult, deadZone);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Lo.WaddahAttarExplosion WaddahAttarExplosion(int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			return indicator.WaddahAttarExplosion(Input, sensitivity, fastLength, fastSmooth, fastSmoothLength, slowLength, slowSmooth, slowSmoothLength, channelLength, mult, deadZone);
		}

		public Indicators.Lo.WaddahAttarExplosion WaddahAttarExplosion(ISeries<double> input , int sensitivity, int fastLength, bool fastSmooth, int fastSmoothLength, int slowLength, bool slowSmooth, int slowSmoothLength, int channelLength, double mult, double deadZone)
		{
			return indicator.WaddahAttarExplosion(input, sensitivity, fastLength, fastSmooth, fastSmoothLength, slowLength, slowSmooth, slowSmoothLength, channelLength, mult, deadZone);
		}
	}
}

#endregion
