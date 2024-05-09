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
	public class _TradingHoursSessionsZones : Indicator
	{
		protected override void OnStateChange()
		{
			#region State.SetDefaults
			
				if (State == State.SetDefaults)
				{
					Description									= @"Enter the description for your new custom Indicator here.";
					Name										= "_TradingHoursSessionsZones";
					Calculate									= Calculate.OnPriceChange;
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
						
					#region 00. Sessions Times
					
						TYRegionFrom  							= DateTime.Parse("01:00");
						TYRegionTo								= DateTime.Parse("10:00");
						
						LDRegionFrom  							= DateTime.Parse("09:00");
						LDRegionTo								= DateTime.Parse("18:00");
						
						NYRegionFrom  							= DateTime.Parse("14:00");
						NYRegionTo								= DateTime.Parse("23:00");
					
					#endregion
						
					#region 01. Sessions Colors On/Off Switches
					
						TYSessionsColorsOnOff					= true;
						LDSessionsColorsOnOff					= true;
						NYSessionsColorsOnOff					= true;
					
					#endregion
						
					#region 02. Sessions Colors Selectors

						ColorTokyo						= Brushes.Cyan;
						ColorLondon						= Brushes.Yellow;
						ColorNewYork					= Brushes.Fuchsia;
			
					#endregion
						
					#region 03. Sessions Opacities Selectors

						TYOpacity						= 50;
						LDOpacity						= 50;
						NYOpacity						= 50;
			
					#endregion
				}
			
			#endregion
			
			#region State.Configure
			
				else if (State == State.Configure)
				{
				}
			
			#endregion
			
			#region State.DataLoaded
			
				else if (State == State.DataLoaded)
				{
				}
				
			#endregion
			
			#region State.Historical
			
				else if (State == State.Historical)
				{
					#region Display Order
					
						// Make sure our object plots behind the chart bars
						SetZOrder(-1); // SetZOrder(int.MaxValue); = to draw your object topmost
					
					#endregion
				}
				
			#endregion
			
			#region State.Terminated
			
				else if (State == State.Terminated)
				{
				}
				
			#endregion
		}

		protected override void OnBarUpdate()
		{	
			#region Sessions Rectangles Drawings
			
				int year 	= Convert.ToInt16(Time[0].ToString("yyyy"));
				int month	= Convert.ToInt16(Time[0].ToString("MM"));
				int day 	= Convert.ToInt16(Time[0].ToString("dd"));

				if(TYSessionsColorsOnOff)
				{
					if (ToTime(Time[0]) >= ToTime(TYRegionFrom) && ToTime(Time[0]) <= ToTime(TYRegionTo)) 
					{
						int barsAgo = CurrentBar - Bars.GetBar(new DateTime(year, month, day, (ToTime(TYRegionFrom)/10000), 0, 0));
						Draw.Rectangle(this, "Tokyo" + Time[0].ToString("MM/dd/yyyy"), false, barsAgo, Highs[0][HighestBar(High, barsAgo)], 0, Lows[0][LowestBar(Low, barsAgo)], Brushes.Transparent, ColorTokyo, TYOpacity);
					}
				}


				if(LDSessionsColorsOnOff)
				{
					if (ToTime(Time[0]) >= ToTime(LDRegionFrom) && ToTime(Time[0]) <= ToTime(LDRegionTo)) 
					{
						int barsAgo = CurrentBar - Bars.GetBar(new DateTime(year, month, day, (ToTime(LDRegionFrom)/10000), 0, 0));
						Draw.Rectangle(this, "London" + Time[0].ToString("MM/dd/yyyy"), false, barsAgo, Highs[0][HighestBar(High, barsAgo)], 0, Lows[0][LowestBar(Low, barsAgo)], Brushes.Transparent, ColorLondon, LDOpacity);
					}
				}


				if(NYSessionsColorsOnOff)
				{
					if (ToTime(Time[0]) >= ToTime(NYRegionFrom) && ToTime(Time[0]) <= ToTime(NYRegionTo)) 
					{
						int barsAgo = CurrentBar - Bars.GetBar(new DateTime(year, month, day, (ToTime(NYRegionFrom)/10000), 0, 0));
						Draw.Rectangle(this, "NewYork" + Time[0].ToString("MM/dd/yyyy"), false, barsAgo, Highs[0][HighestBar(High, barsAgo)], 0, Lows[0][LowestBar(Low, barsAgo)], Brushes.Transparent, ColorNewYork, NYOpacity);
					}
				}
			
			#endregion
		}

        #region Properties
		
			#region 00. Sessions Times
			
				[NinjaScriptProperty]
				[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
				[Display(Name="Tokyo Session Start Time", Description="Start time for painted session in local time", Order=0, GroupName="00. Sessions Times")]
				public DateTime TYRegionFrom
				{ get; set; }
				
				[NinjaScriptProperty]
				[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
				[Display(Name="Tokyo Session End Time", Description="End time for painted session in local time", Order=1, GroupName="00. Sessions Times")]
				public DateTime TYRegionTo
				{ get; set; }
				
				
				
				[NinjaScriptProperty]
				[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
				[Display(Name="London Session Start Time", Description="Start time for painted session in local time", Order=2, GroupName="00. Sessions Times")]
				public DateTime LDRegionFrom
				{ get; set; }
				
				[NinjaScriptProperty]
				[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
				[Display(Name="London Session End Time", Description="End time for painted session in local time", Order=3, GroupName="00. Sessions Times")]
				public DateTime LDRegionTo
				{ get; set; }
				
				
				
				[NinjaScriptProperty]
				[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
				[Display(Name="New York Session Start Time", Description="Start time for painted session in local time", Order=4, GroupName="00. Sessions Times")]
				public DateTime NYRegionFrom
				{ get; set; }
				
				[NinjaScriptProperty]
				[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
				[Display(Name="New York Session End Time", Description="End time for painted session in local time", Order=5, GroupName="00. Sessions Times")]
				public DateTime NYRegionTo
				{ get; set; }
			
			#endregion


			#region 01. Sessions Colors On/Off Switches
				
				[NinjaScriptProperty]
				[Display(Name="Tokyo Session On/Off Switch", Order=0, GroupName="02. Sessions Colors On/Off Switches")]
				public bool TYSessionsColorsOnOff
				{ get; set; }
				
				[NinjaScriptProperty]
				[Display(Name="London Session On/Off Switch", Order=1, GroupName="02. Sessions Colors On/Off Switches")]
				public bool LDSessionsColorsOnOff
				{ get; set; }
				
				[NinjaScriptProperty]
				[Display(Name="New York Session On/Off Switch", Order=2, GroupName="02. Sessions Colors On/Off Switches")]
				public bool NYSessionsColorsOnOff
				{ get; set; }
			
			#endregion
			
			
			#region 02. Sessions Colors
		
				[NinjaScriptProperty]
				[XmlIgnore]
				[Display(Name="Tokyo Session", Description="Color for painted session", Order=0, GroupName="01. Sessions Colors")]
				public Brush ColorTokyo
				{ get; set; }

				[Browsable(false)]
				public string ColorTokyoSerialize
				{
					get { return Serialize.BrushToString(ColorTokyo); }
					set { ColorTokyo = Serialize.StringToBrush(value); }
				}
				
				

				[NinjaScriptProperty]
				[XmlIgnore]
				[Display(Name="London Session", Description="Color for painted session", Order=1, GroupName="01. Sessions Colors")]
				public Brush ColorLondon
				{ get; set; }

				[Browsable(false)]
				public string ColorLondonSerialize
				{
					get { return Serialize.BrushToString(ColorLondon); }
					set { ColorLondon = Serialize.StringToBrush(value); }
				}
				
				

				[NinjaScriptProperty]
				[XmlIgnore]
				[Display(Name="NewYork Session", Description="Color for painted session", Order=2, GroupName="01. Sessions Colors")]
				public Brush ColorNewYork
				{ get; set; }

				[Browsable(false)]
				public string ColorNewYorkSerialize
				{
					get { return Serialize.BrushToString(ColorNewYork); }
					set { ColorNewYork = Serialize.StringToBrush(value); }
				}
			
			#endregion
			
				
			#region 03. Sessions Opacities
				
				[NinjaScriptProperty]
				[Range(0, 100)]
				[Display(Name="Tokyo Session Opacity", Description="Sessions Opacities", Order=0, GroupName="02. Sessions Opacities")]
				public int TYOpacity
				{ get; set; }
				
				[NinjaScriptProperty]
				[Range(0, 100)]
				[Display(Name="London Session Opacity", Description="Sessions Opacities", Order=1, GroupName="02. Sessions Opacities")]
				public int LDOpacity
				{ get; set; }
				
				
				[NinjaScriptProperty]
				[Range(0, 100)]
				[Display(Name="New York Session Opacity", Description="Sessions Opacities", Order=2, GroupName="02. Sessions Opacities")]
				public int NYOpacity
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
		private _TradingHoursSessionsZones[] cache_TradingHoursSessionsZones;
		public _TradingHoursSessionsZones _TradingHoursSessionsZones(DateTime tYRegionFrom, DateTime tYRegionTo, DateTime lDRegionFrom, DateTime lDRegionTo, DateTime nYRegionFrom, DateTime nYRegionTo, bool tYSessionsColorsOnOff, bool lDSessionsColorsOnOff, bool nYSessionsColorsOnOff, Brush colorTokyo, Brush colorLondon, Brush colorNewYork, int tYOpacity, int lDOpacity, int nYOpacity)
		{
			return _TradingHoursSessionsZones(Input, tYRegionFrom, tYRegionTo, lDRegionFrom, lDRegionTo, nYRegionFrom, nYRegionTo, tYSessionsColorsOnOff, lDSessionsColorsOnOff, nYSessionsColorsOnOff, colorTokyo, colorLondon, colorNewYork, tYOpacity, lDOpacity, nYOpacity);
		}

		public _TradingHoursSessionsZones _TradingHoursSessionsZones(ISeries<double> input, DateTime tYRegionFrom, DateTime tYRegionTo, DateTime lDRegionFrom, DateTime lDRegionTo, DateTime nYRegionFrom, DateTime nYRegionTo, bool tYSessionsColorsOnOff, bool lDSessionsColorsOnOff, bool nYSessionsColorsOnOff, Brush colorTokyo, Brush colorLondon, Brush colorNewYork, int tYOpacity, int lDOpacity, int nYOpacity)
		{
			if (cache_TradingHoursSessionsZones != null)
				for (int idx = 0; idx < cache_TradingHoursSessionsZones.Length; idx++)
					if (cache_TradingHoursSessionsZones[idx] != null && cache_TradingHoursSessionsZones[idx].TYRegionFrom == tYRegionFrom && cache_TradingHoursSessionsZones[idx].TYRegionTo == tYRegionTo && cache_TradingHoursSessionsZones[idx].LDRegionFrom == lDRegionFrom && cache_TradingHoursSessionsZones[idx].LDRegionTo == lDRegionTo && cache_TradingHoursSessionsZones[idx].NYRegionFrom == nYRegionFrom && cache_TradingHoursSessionsZones[idx].NYRegionTo == nYRegionTo && cache_TradingHoursSessionsZones[idx].TYSessionsColorsOnOff == tYSessionsColorsOnOff && cache_TradingHoursSessionsZones[idx].LDSessionsColorsOnOff == lDSessionsColorsOnOff && cache_TradingHoursSessionsZones[idx].NYSessionsColorsOnOff == nYSessionsColorsOnOff && cache_TradingHoursSessionsZones[idx].ColorTokyo == colorTokyo && cache_TradingHoursSessionsZones[idx].ColorLondon == colorLondon && cache_TradingHoursSessionsZones[idx].ColorNewYork == colorNewYork && cache_TradingHoursSessionsZones[idx].TYOpacity == tYOpacity && cache_TradingHoursSessionsZones[idx].LDOpacity == lDOpacity && cache_TradingHoursSessionsZones[idx].NYOpacity == nYOpacity && cache_TradingHoursSessionsZones[idx].EqualsInput(input))
						return cache_TradingHoursSessionsZones[idx];
			return CacheIndicator<_TradingHoursSessionsZones>(new _TradingHoursSessionsZones(){ TYRegionFrom = tYRegionFrom, TYRegionTo = tYRegionTo, LDRegionFrom = lDRegionFrom, LDRegionTo = lDRegionTo, NYRegionFrom = nYRegionFrom, NYRegionTo = nYRegionTo, TYSessionsColorsOnOff = tYSessionsColorsOnOff, LDSessionsColorsOnOff = lDSessionsColorsOnOff, NYSessionsColorsOnOff = nYSessionsColorsOnOff, ColorTokyo = colorTokyo, ColorLondon = colorLondon, ColorNewYork = colorNewYork, TYOpacity = tYOpacity, LDOpacity = lDOpacity, NYOpacity = nYOpacity }, input, ref cache_TradingHoursSessionsZones);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators._TradingHoursSessionsZones _TradingHoursSessionsZones(DateTime tYRegionFrom, DateTime tYRegionTo, DateTime lDRegionFrom, DateTime lDRegionTo, DateTime nYRegionFrom, DateTime nYRegionTo, bool tYSessionsColorsOnOff, bool lDSessionsColorsOnOff, bool nYSessionsColorsOnOff, Brush colorTokyo, Brush colorLondon, Brush colorNewYork, int tYOpacity, int lDOpacity, int nYOpacity)
		{
			return indicator._TradingHoursSessionsZones(Input, tYRegionFrom, tYRegionTo, lDRegionFrom, lDRegionTo, nYRegionFrom, nYRegionTo, tYSessionsColorsOnOff, lDSessionsColorsOnOff, nYSessionsColorsOnOff, colorTokyo, colorLondon, colorNewYork, tYOpacity, lDOpacity, nYOpacity);
		}

		public Indicators._TradingHoursSessionsZones _TradingHoursSessionsZones(ISeries<double> input , DateTime tYRegionFrom, DateTime tYRegionTo, DateTime lDRegionFrom, DateTime lDRegionTo, DateTime nYRegionFrom, DateTime nYRegionTo, bool tYSessionsColorsOnOff, bool lDSessionsColorsOnOff, bool nYSessionsColorsOnOff, Brush colorTokyo, Brush colorLondon, Brush colorNewYork, int tYOpacity, int lDOpacity, int nYOpacity)
		{
			return indicator._TradingHoursSessionsZones(input, tYRegionFrom, tYRegionTo, lDRegionFrom, lDRegionTo, nYRegionFrom, nYRegionTo, tYSessionsColorsOnOff, lDSessionsColorsOnOff, nYSessionsColorsOnOff, colorTokyo, colorLondon, colorNewYork, tYOpacity, lDOpacity, nYOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators._TradingHoursSessionsZones _TradingHoursSessionsZones(DateTime tYRegionFrom, DateTime tYRegionTo, DateTime lDRegionFrom, DateTime lDRegionTo, DateTime nYRegionFrom, DateTime nYRegionTo, bool tYSessionsColorsOnOff, bool lDSessionsColorsOnOff, bool nYSessionsColorsOnOff, Brush colorTokyo, Brush colorLondon, Brush colorNewYork, int tYOpacity, int lDOpacity, int nYOpacity)
		{
			return indicator._TradingHoursSessionsZones(Input, tYRegionFrom, tYRegionTo, lDRegionFrom, lDRegionTo, nYRegionFrom, nYRegionTo, tYSessionsColorsOnOff, lDSessionsColorsOnOff, nYSessionsColorsOnOff, colorTokyo, colorLondon, colorNewYork, tYOpacity, lDOpacity, nYOpacity);
		}

		public Indicators._TradingHoursSessionsZones _TradingHoursSessionsZones(ISeries<double> input , DateTime tYRegionFrom, DateTime tYRegionTo, DateTime lDRegionFrom, DateTime lDRegionTo, DateTime nYRegionFrom, DateTime nYRegionTo, bool tYSessionsColorsOnOff, bool lDSessionsColorsOnOff, bool nYSessionsColorsOnOff, Brush colorTokyo, Brush colorLondon, Brush colorNewYork, int tYOpacity, int lDOpacity, int nYOpacity)
		{
			return indicator._TradingHoursSessionsZones(input, tYRegionFrom, tYRegionTo, lDRegionFrom, lDRegionTo, nYRegionFrom, nYRegionTo, tYSessionsColorsOnOff, lDSessionsColorsOnOff, nYSessionsColorsOnOff, colorTokyo, colorLondon, colorNewYork, tYOpacity, lDOpacity, nYOpacity);
		}
	}
}

#endregion
