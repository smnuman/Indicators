
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
using SharpDX.DirectWrite;
using NinjaTrader.Core;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	//version 1.1 - Added routine to check if no day is showing on charft, to show the date of the right most bar in center of the chart.
	
	public class dayofweek : Indicator
	{
		private string lastDay ="";
		private bool yah = false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Prints day of week on chart";
				Name										= "Dayofweekv1.1";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				TextFontta 									= new Gui.Tools.SimpleFont("Arial", 12);
				TextOffset									= 2;
				DrawAtTop									= false;
				FNTcolor									= Brushes.Gold;
				TextWidth									= 100;
			}
		}

		protected override void OnBarUpdate()
		{
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{ 
			base.OnRender(chartControl, chartScale);
			SharpDX.Direct2D1.Brush dxBrush = FNTcolor.ToDxBrush(RenderTarget);
			TextFormat	textFormat			= TextFontta.ToDirectWriteTextFormat();
			double y						= DrawAtTop ? 10 + TextOffset * textFormat.FontSize : ChartPanel.H - 30- TextOffset * textFormat.FontSize;
			double	startX					= -1;
			int daysOnChart					= 0;
			
			for (int barIndex = ChartBars.FromIndex; barIndex <= ChartBars.ToIndex; barIndex++)
			{
				if (barIndex > 0 && barIndex < ChartBars.ToIndex  && (Time.GetValueAt(barIndex).DayOfWeek != Time.GetValueAt(barIndex-1).DayOfWeek))
				{
					daysOnChart++;			
					string day 				= Time.GetValueAt(barIndex).DayOfWeek.ToString();
					lastDay 				= day;
					startX					= chartControl.GetXByBarIndex(ChartBars, barIndex); 
					Point startPoint		= new Point(startX, y);										
					TextLayout textLayout 	= new TextLayout(Globals.DirectWriteFactory, day, textFormat, TextWidth , textFormat.FontSize);
					RenderTarget.DrawTextLayout(startPoint.ToVector2(), textLayout, dxBrush); 
					textLayout.Dispose();
				}	
			}
			
			if (daysOnChart == 0)  // if no days of week are showing on the chart
			{
				lastDay	= Bars.GetTime(ChartBars.ToIndex).DayOfWeek.ToString();  // get the last bar on the right to determine the day			
				startX 					= ChartPanel.W / 2; // put in center of chart window
				Point sPoint			= new Point(startX, y);										
				TextLayout tLayout 		= new TextLayout(Globals.DirectWriteFactory, lastDay, textFormat, TextWidth , textFormat.FontSize);
				RenderTarget.DrawTextLayout(sPoint.ToVector2(), tLayout, dxBrush); 
				tLayout.Dispose();
			}
			
			textFormat.Dispose();
		}
		
		[Display(Name	= "Pivot Label Font",
		Description		= "select font, style, size to display on chart",
		GroupName		= "Display",
		Order			= 1)]
		public Gui.Tools.SimpleFont TextFontta
		{ get; set; }	
		
		[Range(1, int.MaxValue)]
		[Display(Name = "Text line offset", 
		GroupName = "Display", 
		Order = 3)]
		public int TextOffset
		{ get; set; }	
		
		[Range(1, int.MaxValue)]
		[Display(Name = "Text width", 
		GroupName = "Display", 
		Order = 4)]
		public int TextWidth
		{ get; set; }			
		
		[Display(Name	= "Show at top",
		Description		= "Set true to show week day label at top of chart, default is false to show at bottom",
		GroupName		= "Display",
		Order			= 5)]
		public bool DrawAtTop
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Font Color", Description="Color to use for the display", Order=2, GroupName="Display")]
		public Brush FNTcolor
		{ get; set; }
		
		[Browsable(false)]
		public string FNTcolorSerializable
		{
			get { return Serialize.BrushToString(FNTcolor); }
			set { FNTcolor = Serialize.StringToBrush(value); }
		}			
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private dayofweek[] cachedayofweek;
		public dayofweek dayofweek()
		{
			return dayofweek(Input);
		}

		public dayofweek dayofweek(ISeries<double> input)
		{
			if (cachedayofweek != null)
				for (int idx = 0; idx < cachedayofweek.Length; idx++)
					if (cachedayofweek[idx] != null &&  cachedayofweek[idx].EqualsInput(input))
						return cachedayofweek[idx];
			return CacheIndicator<dayofweek>(new dayofweek(), input, ref cachedayofweek);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.dayofweek dayofweek()
		{
			return indicator.dayofweek(Input);
		}

		public Indicators.dayofweek dayofweek(ISeries<double> input )
		{
			return indicator.dayofweek(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.dayofweek dayofweek()
		{
			return indicator.dayofweek(Input);
		}

		public Indicators.dayofweek dayofweek(ISeries<double> input )
		{
			return indicator.dayofweek(input);
		}
	}
}

#endregion
