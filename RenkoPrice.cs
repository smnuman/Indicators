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
	public class RenkoPrice : Indicator
	{
		private double 	BPV;
		private bool 	UpBar;
		private bool 	err = false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Display projected renko bar lines";
				Name										= "RenkoPrice";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				TrendColor									= Brushes.CornflowerBlue;
				RevColor									= Brushes.OrangeRed;
				RangeLineColor								= Brushes.Gold;
				RangeLine									= false;
				LineLength									= 2;
				myDash										= DashStyleHelper.Solid;
				Linewidth									= 2;
				
			}
			else if (State == State.DataLoaded)
			{
				if (BarsPeriod.BarsPeriodType != BarsPeriodType.Renko)
				{
					Draw.TextFixed (this, "test", "RenkoPrice will only work on NinjaTrader renko bars", TextPosition.BottomRight);
					err = true;
				}
				
				if (Calculate == Calculate.OnEachTick || Calculate == Calculate.OnPriceChange)
					Calculate = Calculate.OnBarClose;  // reset in case user changes.
				
				BPV = BarsPeriod.Value * TickSize;	// Sets the size of the bar
			}
		}

		protected override void OnBarUpdate()
		{	
			if (State != State.Realtime || err) return;  // no point in historical
			
			if (Close[0] > Open[0])  // determine direction
				UpBar = true;
			else
				UpBar = false;
			
			Draw.Line(this, "P1", true, 0, (Close[0] + (UpBar ? BPV : -BPV)), -LineLength,  (Close[0] + (UpBar ? BPV : -BPV)),  TrendColor, myDash, Linewidth);
			Draw.Line(this, "P2", true, 0, (Close[0] + (UpBar ?  -2 * BPV : 2 * BPV)), -LineLength,  (Close[0] + (UpBar ? -2 * BPV : 2 * BPV)), RevColor, myDash, Linewidth);
			
			if (RangeLine)
				Draw.Line(this, "p3", true, -1, (Close[0] + (UpBar ? BPV : -BPV)) , -1, (Close[0] + (UpBar ?  -2 * BPV : 2 * BPV)) , RangeLineColor, DashStyleHelper.Solid, Linewidth);
		}

		#region Properties		
		
		[Display(Name="High to low line", Description="Display a line between High and low projection lines", Order=1, GroupName="Options")]
		public bool RangeLine
		{ get; set; }
		
		[Range(1, 30)]
		[Display(Name="Line length", Description="Bars to project into the future", Order=3, GroupName="Price markers")]
		public int LineLength
		{ get; set; }
		
		[Range(1, 30)]
		[Display(Name="Line width", Description="how wide the line shows", Order=4, GroupName="Price markers")]
		public int Linewidth
		{ get; set; }			
		
		[XmlIgnore]
		[Display(Name="Trend Color", Description="Color of line when in trend", Order=1, GroupName="Price markers")]
		public Brush TrendColor
		{ get; set; }	
		
		[Browsable(false)]
		public string TrendColorSerializable
		{
			get { return Serialize.BrushToString(TrendColor); }
			set { TrendColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Reversal Color", Description="Color of reversal line", Order=2, GroupName="Price markers")]
		public Brush RevColor
		{ get; set; }
		
		[Browsable(false)]
		public string RevColorSerializable
		{
			get { return Serialize.BrushToString(RevColor); }
			set { RevColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="H to L color", Description="Color of vertical line", Order=2, GroupName="Options")]
		public Brush RangeLineColor
		{ get; set; }
		
		[Browsable(false)]
		public string RangeLineColorSerializable
		{
			get { return Serialize.BrushToString(RangeLineColor); }
			set { RangeLineColor = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Line type", Description="Choose dashstyle", Order=4, GroupName="Price markers")]
		public DashStyleHelper myDash
		{ get; set; }		
		#endregion		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RenkoPrice[] cacheRenkoPrice;
		public RenkoPrice RenkoPrice()
		{
			return RenkoPrice(Input);
		}

		public RenkoPrice RenkoPrice(ISeries<double> input)
		{
			if (cacheRenkoPrice != null)
				for (int idx = 0; idx < cacheRenkoPrice.Length; idx++)
					if (cacheRenkoPrice[idx] != null &&  cacheRenkoPrice[idx].EqualsInput(input))
						return cacheRenkoPrice[idx];
			return CacheIndicator<RenkoPrice>(new RenkoPrice(), input, ref cacheRenkoPrice);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RenkoPrice RenkoPrice()
		{
			return indicator.RenkoPrice(Input);
		}

		public Indicators.RenkoPrice RenkoPrice(ISeries<double> input )
		{
			return indicator.RenkoPrice(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RenkoPrice RenkoPrice()
		{
			return indicator.RenkoPrice(Input);
		}

		public Indicators.RenkoPrice RenkoPrice(ISeries<double> input )
		{
			return indicator.RenkoPrice(input);
		}
	}
}

#endregion
