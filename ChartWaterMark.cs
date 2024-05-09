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
	/// <summary>
	/// This is a copy of the original indicator "myWaterMarkv2" by Wdickerson from the forum:
	/// https://forum.ninjatrader.com/forum/ninjatrader-8/platform-technical-support-aa/100302-stock-symbol-watermark-in-chart-background?p=794694#post794694
	/// I have modified and upgraded it to this version. -- Numan
	/// </summary>
	public class ChartWaterMark : Indicator
	{
		private string 	_wm_Name, 
						_wm_3LineSpacer	= "\n\n\n",
						_wm_Period 		= "", 
						_wm_Value 		= "", 
						_wm_Account 	= "";
		private double 	_wm_Opacity;
		private double 	_wm_TickSize 	= 1;
		private bool 	_customText 	= false, 
						_showPeriod 	= true,
						_showBigName	= true,
						_showValue		= true,
						_showRollup		= false;
		private AccountSelector xAlselector;
		private Account myAccount;
				
		protected override void OnStateChange()
		{								
			if (State == State.SetDefaults)
			{
				Description									= @"Set Chart Water Mark - Instrument name + optionally value &/or period, or a custom text.";
				Name										= "Chart Water Mark";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				#region ChartWaterMark user variables
				
				CustomTextOn					= false;
				ShowPeriodOn					= true;
				showValue 						= false;
				showBigName						= true;
				CustomText						= @"Custom text here";
				TextColour						= Brushes.DarkSlateGray;
				TextSize						= 70;
				TextOpacity						= 30;
				TextBold						= true;
				
				#endregion
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{			
				_wm_Period	= "";
				
				if (CustomTextOn)
				{
					_wm_Name 	= CustomText;
				}
				else if (!(ShowPeriodOn || showBigName || showValue))
				{
					_wm_Name 	= ChartControl.Instrument.FullName; /// Showing Instrument RollOver
				}
				else
				{		/// Name: Author used -- ChartControl.Instrument.FullName = shows name with rollover
						/// Name: I use -- ChartControl.Instrument.MasterInstrument.Name = shows name without rollover <- saves a bit of screen space
					_wm_TickSize	= (1/ChartControl.Instrument.MasterInstrument.TickSize);		// May be no need of any specialization
					
					_wm_Value 		+= showValue ? ( ChartControl.Instrument.MasterInstrument.PointValue + "/" + _wm_TickSize + (_wm_TickSize < 2 ? " tick /point" : " ticks /point")):"";
					_wm_Value		=  "$" + _wm_Value; 
					_wm_Value 		= _wm_3LineSpacer + _wm_Value;
					
					_wm_Period  	= (ShowPeriodOn ? (ChartControl.BarsPeriod + " ") : "");
					
					_wm_Name 		= showBigName ? ChartControl.Instrument.MasterInstrument.Description : ((showRollup? ChartControl.Instrument.FullName : ChartControl.Instrument.MasterInstrument.Name) + (ShowPeriodOn ? ", " +_wm_Period : ""));
				}
				
				/// Visual - set text opacity -- Mod Numan
				_wm_Opacity = (TextOpacity / 100);
				
				ClearOutputWindow();
				
				Brush myWaterMarkBrush 		= new SolidColorBrush();
				myWaterMarkBrush 			= TextColour.Clone();
				myWaterMarkBrush.Opacity 	= _wm_Opacity;
				myWaterMarkBrush.Freeze();
				
				NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont(ChartControl.Properties.LabelFont.FamilySerialize, TextSize) {Size = TextSize / (showBigName ? 1.25 : 1), Bold = TextBold};
				Draw.TextFixed(this, "CWM_txt1", _wm_Name, TextPosition.Center, myWaterMarkBrush, myFont, Brushes.Transparent, Brushes.Transparent, 1);		
				if (showValue)
				{
					NinjaTrader.Gui.Tools.SimpleFont myFont2 = new NinjaTrader.Gui.Tools.SimpleFont(ChartControl.Properties.LabelFont.FamilySerialize, TextSize) {Size = TextSize/2, Bold = TextBold};
					Draw.TextFixed(this, "CWM_txt2", _wm_Value, TextPosition.Center, myWaterMarkBrush, myFont2, Brushes.Transparent, Brushes.Transparent, 1);		
				}
				if (showBigName)
				{
					NinjaTrader.Gui.Tools.SimpleFont myFont2 = new NinjaTrader.Gui.Tools.SimpleFont(ChartControl.Properties.LabelFont.FamilySerialize, TextSize) {Size = TextSize/2.5, Bold = TextBold};
					Draw.TextFixed(this, "CWM_txt3", ("\n\n" +_wm_Period), TextPosition.Center, myWaterMarkBrush, myFont2, Brushes.Transparent, Brushes.Transparent, 1);		
				}
			}
		}

		protected override void OnBarUpdate()
		{
			// Add code here
		}

		#region Properties
		
		#region A. Custom Text
		
		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]
		[Display(Name="CustomTextOn", Description="If you want to see a customised text in the chart background.", Order=0, GroupName="A. Watermark - Custom")]
		public bool CustomTextOn
		{
		    get
		   {
		      return _customText;
		   }
		   set
		   {
		      if (value == true)
		      {
		         ShowPeriodOn 	= false;
				 showBigName	= false;
				 showValue		= false;
				 showRollup		= false;
		      }
		      _customText = value;
		    }
		}

		[NinjaScriptProperty]
		[Display(Name="Custom text", Description="If you like, you can enter your intended text here.", Order=1, GroupName="A. Watermark - Custom")]
		public string CustomText
		{ get; set; }

		#endregion
		
		#region B. Settings for Default Text to display
		
		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]		
		[Display(Name="Show Value", Description="To see Ticks or points value", Order=3, GroupName="B. Watermark - Default")]
		public bool showValue
//		{ get; set; }
		{
		    get
		   {
		      return _showValue;
		   }
		   set
		   {
		      if (value == true)
		      {
		         CustomTextOn = false;
		      }
		      _showValue = value;
		    }
		}

		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]
		[Display(Name="Show Period", Description="If you want to see the period also in the chart background.", Order=2, GroupName="B. Watermark - Default")]
		public bool ShowPeriodOn
		{
		    get
		   {
		      return _showPeriod;
		   }
		   set
		   {
		      if (value == true)
		      {
		         CustomTextOn = false;
		      }
		      _showPeriod = value;
		    }
		}
		
		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]		
		[Display(Name="Show full name", Description="To see Ticks or points value", Order=3, GroupName="B. Watermark - Default")]
		public bool showBigName
//		{ get; set; }
		{
		    get
		   {
		      return _showBigName;
		   }
		   set
		   {
		      if (value == true)
		      {
		         CustomTextOn	= false;
				 showRollup 	= false;
		      }
		      _showBigName = value;
		    }
		}
		//_showRollup
		
		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]		
		[Display(Name="Show rollup", Description="To see the rollup period", Order=3, GroupName="B. Watermark - Default")]
		public bool showRollup
//		{ get; set; }
		{
		    get
		   {
		      return _showRollup;
		   }
		   set
		   {
		      if (value == true)
		      {
		         CustomTextOn	= false;
				 showBigName	= false;
		      }
		      _showRollup = value;
		    }
		}
		//_showRollup
				
		#endregion

		#region C. Font settings
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Size", Description="Size of the text to view", Order=4, GroupName="C. Watermark - Font")]
		public int TextSize
		{ get; set; }

		[NinjaScriptProperty]		
		[Display(Name="Bold ?", Description="Boldness of the text to view", Order=5, GroupName="C. Watermark - Font")]
		public bool TextBold
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Text Colour", Description="Choose the color that stands out on your chart", Order=6, GroupName="C. Watermark - Font")]
		public Brush TextColour
		{ get; set; }

		[Browsable(false)]
		public string TextColourSerializable
		{
			get { return Serialize.BrushToString(TextColour); }
			set { TextColour = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Text Opacity", Description="Percent of the colour visibility", Order=7, GroupName="C. Watermark - Font")]
		public double TextOpacity
		{ get; set; }
		
		#endregion
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ChartWaterMark[] cacheChartWaterMark;
		public ChartWaterMark ChartWaterMark(bool customTextOn, string customText, bool showValue, bool showPeriodOn, bool showBigName, bool showRollup, int textSize, bool textBold, Brush textColour, double textOpacity)
		{
			return ChartWaterMark(Input, customTextOn, customText, showValue, showPeriodOn, showBigName, showRollup, textSize, textBold, textColour, textOpacity);
		}

		public ChartWaterMark ChartWaterMark(ISeries<double> input, bool customTextOn, string customText, bool showValue, bool showPeriodOn, bool showBigName, bool showRollup, int textSize, bool textBold, Brush textColour, double textOpacity)
		{
			if (cacheChartWaterMark != null)
				for (int idx = 0; idx < cacheChartWaterMark.Length; idx++)
					if (cacheChartWaterMark[idx] != null && cacheChartWaterMark[idx].CustomTextOn == customTextOn && cacheChartWaterMark[idx].CustomText == customText && cacheChartWaterMark[idx].showValue == showValue && cacheChartWaterMark[idx].ShowPeriodOn == showPeriodOn && cacheChartWaterMark[idx].showBigName == showBigName && cacheChartWaterMark[idx].showRollup == showRollup && cacheChartWaterMark[idx].TextSize == textSize && cacheChartWaterMark[idx].TextBold == textBold && cacheChartWaterMark[idx].TextColour == textColour && cacheChartWaterMark[idx].TextOpacity == textOpacity && cacheChartWaterMark[idx].EqualsInput(input))
						return cacheChartWaterMark[idx];
			return CacheIndicator<ChartWaterMark>(new ChartWaterMark(){ CustomTextOn = customTextOn, CustomText = customText, showValue = showValue, ShowPeriodOn = showPeriodOn, showBigName = showBigName, showRollup = showRollup, TextSize = textSize, TextBold = textBold, TextColour = textColour, TextOpacity = textOpacity }, input, ref cacheChartWaterMark);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChartWaterMark ChartWaterMark(bool customTextOn, string customText, bool showValue, bool showPeriodOn, bool showBigName, bool showRollup, int textSize, bool textBold, Brush textColour, double textOpacity)
		{
			return indicator.ChartWaterMark(Input, customTextOn, customText, showValue, showPeriodOn, showBigName, showRollup, textSize, textBold, textColour, textOpacity);
		}

		public Indicators.ChartWaterMark ChartWaterMark(ISeries<double> input , bool customTextOn, string customText, bool showValue, bool showPeriodOn, bool showBigName, bool showRollup, int textSize, bool textBold, Brush textColour, double textOpacity)
		{
			return indicator.ChartWaterMark(input, customTextOn, customText, showValue, showPeriodOn, showBigName, showRollup, textSize, textBold, textColour, textOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChartWaterMark ChartWaterMark(bool customTextOn, string customText, bool showValue, bool showPeriodOn, bool showBigName, bool showRollup, int textSize, bool textBold, Brush textColour, double textOpacity)
		{
			return indicator.ChartWaterMark(Input, customTextOn, customText, showValue, showPeriodOn, showBigName, showRollup, textSize, textBold, textColour, textOpacity);
		}

		public Indicators.ChartWaterMark ChartWaterMark(ISeries<double> input , bool customTextOn, string customText, bool showValue, bool showPeriodOn, bool showBigName, bool showRollup, int textSize, bool textBold, Brush textColour, double textOpacity)
		{
			return indicator.ChartWaterMark(input, customTextOn, customText, showValue, showPeriodOn, showBigName, showRollup, textSize, textBold, textColour, textOpacity);
		}
	}
}

#endregion
