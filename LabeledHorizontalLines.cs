#region Using declarations
using System;
using System.Collections;
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
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class LabeledHorizontalLines : Indicator
	{
		private System.Windows.Media.Brush	textBrush;
		private HorizLabelSideType labelSide = HorizLabelSideType.Right;
		private HorizLabelAreaType labelArea = HorizLabelAreaType.Above;
		private float fontSize = 20.0f ;
        private Boolean _init = false;
        private string _priceFormat;
		private ArrayList lineInfoArray = new ArrayList();
		HorizontalLine l1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "LabeledHorizontalLines";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				TextBrush = System.Windows.Media.Brushes.DodgerBlue;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			int j ;
			String beginMessage = Instrument.FullName + " - Horizontal Line Price Triggered on " ;
			
			// Determine the Period Value
			switch ( BarsPeriod.BarsPeriodType )
			{
				case BarsPeriodType.Minute:
					beginMessage += BarsPeriod.Value + "M Timeframe @ " ;
					break ;
					
				case BarsPeriodType.Day:
					beginMessage += BarsPeriod.Value + "D Timeframe @ " ;
					break ;
					
				case BarsPeriodType.Week:
					beginMessage += BarsPeriod.Value + "W Timeframe @ " ;
					break ;
					
				case BarsPeriodType.Month:
					beginMessage += BarsPeriod.Value + "MO Timeframe @ " ;
					break ;
					
				case BarsPeriodType.Year:
					beginMessage += BarsPeriod.Value + "Y Timeframe @ " ;
					break ;
					
				case BarsPeriodType.Tick:
					beginMessage += BarsPeriod.Value + "Tick Timeframe @ " ;
					break ;
					
			}
            if (!_init)
            {
                _init = true;
                int Digits = 0;
				if (TickSize.ToString().StartsWith("1E-"))
				{
					Digits=Convert.ToInt32(TickSize.ToString().Substring(3));
				}
				else if (TickSize.ToString().IndexOf(".")>0)
				{
					Digits=TickSize.ToString().Substring(TickSize.ToString().IndexOf("."),TickSize.ToString().Length-1).Length-1;
				}
                _priceFormat = string.Format("F{0}", Digits);
				
				// Build a larger font size
				//largerFont = new Font( ChartControl.Font.Name, fontSize, FontStyle.Bold ) ;
            }   
			

		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			SharpDX.Vector2 startPoint;
			SharpDX.Vector2 endPoint;
			
			startPoint = new SharpDX.Vector2(ChartPanel.X, ChartPanel.Y);
			endPoint = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
			
			SharpDX.Vector2 startPoint1 = new System.Windows.Point(ChartPanel.X, ChartPanel.Y + ChartPanel.H).ToVector2();
			SharpDX.Vector2 endPoint1 = new System.Windows.Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y).ToVector2();
			
			float width = endPoint.X - startPoint.X;
			float height = endPoint.Y - startPoint.Y;
			
			SharpDX.Vector2 center = (startPoint + endPoint) / 2;
			
			SharpDX.Direct2D1.Brush textBrushDx;
			
			textBrushDx = textBrush.ToDxBrush(RenderTarget);
			
			
			NinjaTrader.Gui.Tools.SimpleFont simpleFont = chartControl.Properties.LabelFont ??  new NinjaTrader.Gui.Tools.SimpleFont("Arial", 12);
			
			SharpDX.DirectWrite.TextFormat textFormat1 = simpleFont.ToDirectWriteTextFormat();
			
			SharpDX.DirectWrite.TextFormat textFormat2 =
				new SharpDX.DirectWrite.TextFormat(NinjaTrader.Core.Globals.DirectWriteFactory, "Century Gothic", SharpDX.DirectWrite.FontWeight.Bold,
					SharpDX.DirectWrite.FontStyle.Italic, fontSize);
			
			foreach (Gui.NinjaScript.IChartObject thisObject in ChartPanel.ChartObjects)
		  	{
			  
			  if(thisObject is NinjaTrader.NinjaScript.DrawingTools.HorizontalLine)
			  {
			  	 l1 = thisObject as NinjaTrader.NinjaScript.DrawingTools.HorizontalLine;
				  
				 SharpDX.DirectWrite.TextLayout textLayout2 =
				 new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				 l1.StartAnchor.Price.ToString(_priceFormat), textFormat2, 400, textFormat1.FontSize);
				  
				 if(LabelSide == HorizLabelSideType.Right)
				 {
				 	if(labelArea == HorizLabelAreaType.Above)
				 	{
				 	SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(ChartPanel.W - textLayout2.Metrics.Width - 5,
				 	ChartPanel.Y + ((float)l1.StartAnchor.GetPoint(chartControl, ChartPanel, chartScale).Y)-(float)textLayout2.Metrics.Height);
					RenderTarget.DrawTextLayout(lowerTextPoint, textLayout2, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
				 	}
					if(labelArea == HorizLabelAreaType.Below)
					{
					 SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(ChartPanel.W - textLayout2.Metrics.Width - 5,
					 ChartPanel.Y + ((float)l1.StartAnchor.GetPoint(chartControl, ChartPanel, chartScale).Y));
					 RenderTarget.DrawTextLayout(lowerTextPoint, textLayout2, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					}
				 }
				 
				 if(LabelSide == HorizLabelSideType.Left)
				 {
				 	if(labelArea == HorizLabelAreaType.Above)
				 	{
				 	SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(0,
				 	ChartPanel.Y + ((float)l1.StartAnchor.GetPoint(chartControl, ChartPanel, chartScale).Y)-(float)textLayout2.Metrics.Height);
					RenderTarget.DrawTextLayout(lowerTextPoint, textLayout2, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
				 	}
					if(labelArea == HorizLabelAreaType.Below)
					{
					 SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(0,
					 ChartPanel.Y + ((float)l1.StartAnchor.GetPoint(chartControl, ChartPanel, chartScale).Y));
					 RenderTarget.DrawTextLayout(lowerTextPoint, textLayout2, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					}
				 }
			  
			  }
			  

			  
		 	 }
			
		}
		
		[Description("Set the size of the display font")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Font Size", GroupName = "NinjaScriptParameters", Order = 0)]
		public float FontSize
		{
			get { return fontSize; }
			set { fontSize = value; }
		}
		
		[Description("Display on the left side of the line or the right side")]
        [NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Label Side", GroupName = "NinjaScriptParameters", Order = 0)]
        public HorizLabelSideType LabelSide
        {
            get { return labelSide; }
            set { labelSide = value; }
        }
		
		[Description("Display the label Above or Below the line")]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Above/Below", GroupName = "NinjaScriptParameters", Order = 0)]
		public HorizLabelAreaType LabelArea
		{
			get { return labelArea; }
			set { labelArea = value ; }
		}
	
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TextColor", GroupName = "NinjaScriptGeneral")]
		public System.Windows.Media.Brush TextBrush
		{
			get { return textBrush; }
			set { textBrush = value; }
		}
	
		[Browsable(false)]
		public string TextBrushSerialize
		{
			get { return Serialize.BrushToString(TextBrush); }
			set { TextBrush = Serialize.StringToBrush(value); }
		}
		
	
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LabeledHorizontalLines[] cacheLabeledHorizontalLines;
		public LabeledHorizontalLines LabeledHorizontalLines(float fontSize, HorizLabelSideType labelSide, HorizLabelAreaType labelArea)
		{
			return LabeledHorizontalLines(Input, fontSize, labelSide, labelArea);
		}

		public LabeledHorizontalLines LabeledHorizontalLines(ISeries<double> input, float fontSize, HorizLabelSideType labelSide, HorizLabelAreaType labelArea)
		{
			if (cacheLabeledHorizontalLines != null)
				for (int idx = 0; idx < cacheLabeledHorizontalLines.Length; idx++)
					if (cacheLabeledHorizontalLines[idx] != null && cacheLabeledHorizontalLines[idx].FontSize == fontSize && cacheLabeledHorizontalLines[idx].LabelSide == labelSide && cacheLabeledHorizontalLines[idx].LabelArea == labelArea && cacheLabeledHorizontalLines[idx].EqualsInput(input))
						return cacheLabeledHorizontalLines[idx];
			return CacheIndicator<LabeledHorizontalLines>(new LabeledHorizontalLines(){ FontSize = fontSize, LabelSide = labelSide, LabelArea = labelArea }, input, ref cacheLabeledHorizontalLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LabeledHorizontalLines LabeledHorizontalLines(float fontSize, HorizLabelSideType labelSide, HorizLabelAreaType labelArea)
		{
			return indicator.LabeledHorizontalLines(Input, fontSize, labelSide, labelArea);
		}

		public Indicators.LabeledHorizontalLines LabeledHorizontalLines(ISeries<double> input , float fontSize, HorizLabelSideType labelSide, HorizLabelAreaType labelArea)
		{
			return indicator.LabeledHorizontalLines(input, fontSize, labelSide, labelArea);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LabeledHorizontalLines LabeledHorizontalLines(float fontSize, HorizLabelSideType labelSide, HorizLabelAreaType labelArea)
		{
			return indicator.LabeledHorizontalLines(Input, fontSize, labelSide, labelArea);
		}

		public Indicators.LabeledHorizontalLines LabeledHorizontalLines(ISeries<double> input , float fontSize, HorizLabelSideType labelSide, HorizLabelAreaType labelArea)
		{
			return indicator.LabeledHorizontalLines(input, fontSize, labelSide, labelArea);
		}
	}
}

#endregion
