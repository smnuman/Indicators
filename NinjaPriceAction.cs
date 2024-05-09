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
//using System.Speech.AudioFormat; 
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class NinjaPriceAction : Indicator
	{
		private int 	barsback;
		private double 	prevhigh;
		private double 	prevlow;
		private double 	curhigh;
		private double 	curlow;
		private double 	offset;
		private double 	curATR;
		private double 	toffset;
		private ATR 	myATR;
		private Swing 	mySwing;
		private double 	tOffset;		
		private int		myBarUp, myBarDown;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Labels the swing points";
				Name										= "NinjaPriceAction";
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
				Strength					= 3;
				TextOffset					= 3;
				LookBack					= 50;
				DTBStrength					= 15;
				TextFont					= new NinjaTrader.Gui.Tools.SimpleFont("Arial", 12); 
				DBcolor						= Brushes.Gold;
				DTcolor						= Brushes.Gold;
				HHcolor						= Brushes.Green;
				HLcolor						= Brushes.Green;
				LLcolor						= Brushes.Red;
				LHcolor						= Brushes.Red;
			}
			else if (State == State.DataLoaded)
			{
				myATR = ATR(14);			
				mySwing = Swing(Strength);
				tOffset = TextOffset * TickSize;
			}			
		}

		protected override void OnBarUpdate()
		{
					
			if (CurrentBar < LookBack + 1) return;
									
			barsback = mySwing.SwingHighBar(0,2,LookBack);
		
			if (barsback == -1) 
				return; 
			
			prevhigh = mySwing.SwingHigh[barsback]; 
						
			barsback = mySwing.SwingHighBar(0,1,LookBack);
			
			if (barsback == -1) 
				return; 
			
			curhigh = mySwing.SwingHigh[barsback]; 
			
			curATR	= myATR[barsback];	
			offset	= curATR * DTBStrength / 100;
					
			if (curhigh < prevhigh + offset && curhigh > prevhigh - offset && CurrentBar - barsback != myBarUp)
			{
				myBarUp = CurrentBar - barsback;
				Draw.Text(this, "DT"+CurrentBar, true, "DT", barsback, High[barsback] + tOffset, 0, DTcolor, 
					TextFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);	
			}
			else if (curhigh > prevhigh && CurrentBar - barsback != myBarUp)
			{
				myBarUp = CurrentBar - barsback;
				Draw.Text(this, "HH"+CurrentBar, true, "HH", barsback, High[barsback] + tOffset, 0, HHcolor,
					TextFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			}
			else if (CurrentBar - barsback != myBarUp)
			{
				myBarUp = CurrentBar - barsback;
				Draw.Text(this, "LH"+CurrentBar, true, "LH",barsback, High[barsback] + tOffset, 0,  LHcolor, 
					TextFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			}
			
			
			barsback = mySwing.SwingLowBar(0,2,LookBack);
			
			if (barsback == -1) 
				return; 
			
			prevlow = mySwing.SwingLow[barsback]; 
						
			barsback = mySwing.SwingLowBar(0,1,LookBack);
			
			if (barsback == -1) 
				return; 
			
			curlow = mySwing.SwingLow[barsback];
			
			curATR	= myATR[barsback];	
			offset	= curATR * DTBStrength / 100;

			
			if (curlow > prevlow - offset && curlow < prevlow + offset && CurrentBar - barsback != myBarDown)
			{
				myBarDown = CurrentBar - barsback;
				Draw.Text(this, "DB"+CurrentBar, true, "DB", barsback, Low[barsback] - tOffset, 0, DBcolor,
					TextFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			}
			else if (curlow < prevlow && CurrentBar - barsback != myBarDown)
			{
				myBarDown = CurrentBar - barsback;
				Draw.Text(this, "LL"+CurrentBar, true, "LL", barsback, Low[barsback] - tOffset, 0,  LLcolor,
					TextFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			}
			else if (CurrentBar - barsback != myBarDown)
			{
				myBarDown = CurrentBar - barsback;
				Draw.Text(this, "HL"+CurrentBar, true, "HL", barsback, Low[barsback] - tOffset, 0, HLcolor,
					TextFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Strength", Description="Swing strength, number of bars", Order=1, GroupName="Parameters")]
		public int Strength
		{ get; set; }

		[Range(0, int.MaxValue)]
		[Display(Name="TextOffset", Description="Number of ticks to offset text from high/low", Order=2, GroupName="Parameters")]
		public int TextOffset
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LookBack", Description="Number of bars ago to check", Order=3, GroupName="Parameters")]
		public int LookBack
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="DTBStrength", Order=4, GroupName="Parameters")]
		public int DTBStrength
		{ get; set; }	

		[Display(Name	= "Font, size, type, style",
		Description		= "select font, style, size to display on chart",
		GroupName		= "Text",
		Order			= 1)]
		public Gui.Tools.SimpleFont TextFont
		{ get; set; }	
		
		[XmlIgnore]
		[Display(Name="HH Color", Description="(text) Higher High color", Order=2, GroupName="Text")]
		public Brush HHcolor
		{ get; set; }	

		[Browsable(false)]
		public string HHcolorSerializable
		{
			get { return Serialize.BrushToString(HHcolor); }
			set { HHcolor = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="HL Color", Description="(text) Higher Low color", Order=3, GroupName="Text")]
		public Brush HLcolor
		{ get; set; }	

		[Browsable(false)]
		public string HLcolorSerializable
		{
			get { return Serialize.BrushToString(HLcolor); }
			set { HLcolor = Serialize.StringToBrush(value); }
		}		
		
		[XmlIgnore]
		[Display(Name="DT Color", Description="(text) Double Top color", Order=6, GroupName="Text")]
		public Brush DTcolor
		{ get; set; }	

		[Browsable(false)]
		public string DTcolorSerializable
		{
			get { return Serialize.BrushToString(DTcolor); }
			set { DTcolor = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="LL Color", Description="(text) Lower Low color", Order=4, GroupName="Text")]
		public Brush LLcolor
		{ get; set; }	

		[Browsable(false)]
		public string LLcolorSerializable
		{
			get { return Serialize.BrushToString(LLcolor); }
			set { LLcolor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="LH Color", Description="(text) Lower High color", Order=5, GroupName="Text")]
		public Brush LHcolor
		{ get; set; }	

		[Browsable(false)]
		public string LHcolorSerializable
		{
			get { return Serialize.BrushToString(LHcolor); }
			set { LHcolor = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="DB Color", Description="(text) Double Bottom color", Order=7, GroupName="Text")]
		public Brush DBcolor
		{ get; set; }	

		[Browsable(false)]
		public string DBcolorSerializable
		{
			get { return Serialize.BrushToString(DBcolor); }
			set { DBcolor = Serialize.StringToBrush(value); }
		}		
		#endregion
		

		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private NinjaPriceAction[] cacheNinjaPriceAction;
		public NinjaPriceAction NinjaPriceAction(int strength, int lookBack, int dTBStrength)
		{
			return NinjaPriceAction(Input, strength, lookBack, dTBStrength);
		}

		public NinjaPriceAction NinjaPriceAction(ISeries<double> input, int strength, int lookBack, int dTBStrength)
		{
			if (cacheNinjaPriceAction != null)
				for (int idx = 0; idx < cacheNinjaPriceAction.Length; idx++)
					if (cacheNinjaPriceAction[idx] != null && cacheNinjaPriceAction[idx].Strength == strength && cacheNinjaPriceAction[idx].LookBack == lookBack && cacheNinjaPriceAction[idx].DTBStrength == dTBStrength && cacheNinjaPriceAction[idx].EqualsInput(input))
						return cacheNinjaPriceAction[idx];
			return CacheIndicator<NinjaPriceAction>(new NinjaPriceAction(){ Strength = strength, LookBack = lookBack, DTBStrength = dTBStrength }, input, ref cacheNinjaPriceAction);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.NinjaPriceAction NinjaPriceAction(int strength, int lookBack, int dTBStrength)
		{
			return indicator.NinjaPriceAction(Input, strength, lookBack, dTBStrength);
		}

		public Indicators.NinjaPriceAction NinjaPriceAction(ISeries<double> input , int strength, int lookBack, int dTBStrength)
		{
			return indicator.NinjaPriceAction(input, strength, lookBack, dTBStrength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.NinjaPriceAction NinjaPriceAction(int strength, int lookBack, int dTBStrength)
		{
			return indicator.NinjaPriceAction(Input, strength, lookBack, dTBStrength);
		}

		public Indicators.NinjaPriceAction NinjaPriceAction(ISeries<double> input , int strength, int lookBack, int dTBStrength)
		{
			return indicator.NinjaPriceAction(input, strength, lookBack, dTBStrength);
		}
	}
}

#endregion
