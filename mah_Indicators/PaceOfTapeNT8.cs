

#region Using declarations
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;
//using System.Diagnostics.CodeAnalysis;
//using System.Windows;
//using System.Reflection;
//using System.Windows.Media;
//using System.Xml.Serialization;
//using NinjaTrader.Cbi;
//using NinjaTrader.Data;
//using NinjaTrader.Gui;
//using NinjaTrader.Gui.Chart;
//using NinjaTrader.Gui.Tools;
//using NinjaTrader.NinjaScript.DrawingTools;
//using NinjaTrader.NinjaScript.Indicators;


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

///  Coverted to NT 8.0.16.1  - October 19, 1018 
///  By: aligator at https://ninjatrader.com/support/forum/member/19582-aligator
/// 		This is a direct conversion to NT8 - No changes to origial logic made.	
/// 
///  Original NT7 Version Written By cunparis. 
/*
This indicator measures the intensity of the trades on the time & sales.
High intensity is often an indication of exhaustion volume and often
precedes turning points.

For more information please see: http://www.tradewiththeflow.com/tools/pace-of-tape/

20100517 1.0.4
Cleaned up the code

20100318 1.0.4
Added paintbar

20100317 1.0.2
Added audio alert
Uses Richard's algorithm which is much more efficient

20100315 1.0.1
Added threshold
Fixed bug where period was hard coded

20100314 1.0
Initial version
*/

namespace NinjaTrader.NinjaScript.Indicators
{
     public class PaceOfTapeNT8 : Indicator
    {
		private int 	period 					= 30;
		private int 	threshold 				= 1500;
		private bool 	audioAlert 				= true;
		private bool 	audioAlertRepeat 		= true;
		private int 	lastSound 				= 0;	
		private bool 	paintbars 				= true;
		private 		String audioAlertFile 	= NinjaTrader.Core.Globals.InstallDir+@"\sounds\paceoftape.wav"; //audio alert
		
        protected override void OnStateChange()
			
        {
            if (State == State.SetDefaults)
            {
	            Name = "PaceOfTapeNT8";
	            Description = "This indicator measures the intensity of the trades on the time & sales. High intensity"+
							  " is often an indication of exhaustion volume and often precedes turning points."+
							  " For more information see: http://www.tradewiththeflow.com/tools/pace-of-tape/";
				
	            AddPlot(new Stroke(Brushes.Cyan, 1), PlotStyle.Bar, "POT Normal");
	            AddPlot(new Stroke(Brushes.Orange, 1), PlotStyle.Bar, "POT High");
				
	            Calculate	= Calculate.OnEachTick;
	            IsOverlay	= false;
            }
        }

        protected override void OnBarUpdate()
        {
			int pace = 0;
			int i = 0;
			while(i < CurrentBar)
			{
				TimeSpan ts = Time[0] - Time[i];
				if (ts.TotalSeconds < period)
					pace += Bars.BarsPeriod.Value;					

				else 	break;   ++i;	
			}
			
			if (pace >= threshold)
			{	
				Values[1][0] = pace;
			}
		
			else Values[0][0] = pace;
			
			if (paintbars)
			{
				if (pace >= threshold)
		
					BarBrush = Plots[1].Brush;
			}

			if(audioAlert)
			{
				if (audioAlertRepeat)
				{
					if(Values[1].IsValidDataPoint(0))

						PlaySound(audioAlertFile);	
				 
					else if (Values[1].IsValidDataPoint(0) && Values[0].IsValidDataPoint(1))
						
						PlaySound(audioAlertFile);	
				}
			}			
        }

		#region Properties
		[NinjaScriptProperty]
		[Display(Description = "True=show paintbars, false=leave price bars alone.", GroupName = "Parameters", Order = 1)]
		public bool Paintbars
        {
            get { return paintbars; }
            set { paintbars = value; }
        }

		
        [NinjaScriptProperty]
		
        [Display(Description = "AudioAlertFile", GroupName = "Parameters", Order = 1)]
		
        public string AudioAlertFile
        {
            get { return audioAlertFile; }
            set { audioAlertFile = value; }
        }

        [NinjaScriptProperty]
        [Display(Description = "AudioAlert", GroupName = "Parameters", Order = 1)]
        public bool AudioAlert
        {
            get { return audioAlert; }
            set { audioAlert = value; }
        }

        [NinjaScriptProperty]
        [Display(Description = "Play audio every bar or only first bar", GroupName = "Parameters", Order = 1)]
        public bool AudioAlertRepeat
        {
            get { return audioAlertRepeat; }
            set { audioAlertRepeat = value; }
        }
		
        [NinjaScriptProperty]		
        [Display(Description = "Lookback for pace in seconds", GroupName = "Parameters", Order = 1)]		
        public int Period
        {
            get { return period; }
            set { period = value; }
        }

        [NinjaScriptProperty]
        [Display(Description = "Threshold for coloring the histogram", GroupName = "Parameters", Order = 1)]
        public int Threshold
        {
            get { return threshold; }
            set { threshold = value; }
        }
		#endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PaceOfTapeNT8[] cachePaceOfTapeNT8;
		public PaceOfTapeNT8 PaceOfTapeNT8(bool paintbars, string audioAlertFile, bool audioAlert, bool audioAlertRepeat, int period, int threshold)
		{
			return PaceOfTapeNT8(Input, paintbars, audioAlertFile, audioAlert, audioAlertRepeat, period, threshold);
		}

		public PaceOfTapeNT8 PaceOfTapeNT8(ISeries<double> input, bool paintbars, string audioAlertFile, bool audioAlert, bool audioAlertRepeat, int period, int threshold)
		{
			if (cachePaceOfTapeNT8 != null)
				for (int idx = 0; idx < cachePaceOfTapeNT8.Length; idx++)
					if (cachePaceOfTapeNT8[idx] != null && cachePaceOfTapeNT8[idx].Paintbars == paintbars && cachePaceOfTapeNT8[idx].AudioAlertFile == audioAlertFile && cachePaceOfTapeNT8[idx].AudioAlert == audioAlert && cachePaceOfTapeNT8[idx].AudioAlertRepeat == audioAlertRepeat && cachePaceOfTapeNT8[idx].Period == period && cachePaceOfTapeNT8[idx].Threshold == threshold && cachePaceOfTapeNT8[idx].EqualsInput(input))
						return cachePaceOfTapeNT8[idx];
			return CacheIndicator<PaceOfTapeNT8>(new PaceOfTapeNT8(){ Paintbars = paintbars, AudioAlertFile = audioAlertFile, AudioAlert = audioAlert, AudioAlertRepeat = audioAlertRepeat, Period = period, Threshold = threshold }, input, ref cachePaceOfTapeNT8);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PaceOfTapeNT8 PaceOfTapeNT8(bool paintbars, string audioAlertFile, bool audioAlert, bool audioAlertRepeat, int period, int threshold)
		{
			return indicator.PaceOfTapeNT8(Input, paintbars, audioAlertFile, audioAlert, audioAlertRepeat, period, threshold);
		}

		public Indicators.PaceOfTapeNT8 PaceOfTapeNT8(ISeries<double> input , bool paintbars, string audioAlertFile, bool audioAlert, bool audioAlertRepeat, int period, int threshold)
		{
			return indicator.PaceOfTapeNT8(input, paintbars, audioAlertFile, audioAlert, audioAlertRepeat, period, threshold);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PaceOfTapeNT8 PaceOfTapeNT8(bool paintbars, string audioAlertFile, bool audioAlert, bool audioAlertRepeat, int period, int threshold)
		{
			return indicator.PaceOfTapeNT8(Input, paintbars, audioAlertFile, audioAlert, audioAlertRepeat, period, threshold);
		}

		public Indicators.PaceOfTapeNT8 PaceOfTapeNT8(ISeries<double> input , bool paintbars, string audioAlertFile, bool audioAlert, bool audioAlertRepeat, int period, int threshold)
		{
			return indicator.PaceOfTapeNT8(input, paintbars, audioAlertFile, audioAlert, audioAlertRepeat, period, threshold);
		}
	}
}

#endregion
