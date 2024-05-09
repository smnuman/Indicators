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
using System.Windows.Controls;
#endregion

#region Notes
/*Notes - Cross below Line
 Must be enabled on price change or each tick
Turned off by clicking the toolbar button 22/11/2022
Developed from https://ninjatraderecosystem.com/user-app-share-download/labeled-horizontal-lines/
Change the Alert sound file to anything in your default sound directory
*/
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Mindset
{
	public class ShortLine : Indicator
	{
		#region Variables
		private System.Windows.Media.Brush	textBrush;
		private System.Windows.Media.Brush	lineBrush;

		private float fontSize = 15.0f ;
		private bool SetUp_Line_bool = true;
		private double last = 0;
		string pricestr = "";
		double  myPrice1 = 0;
		private HorizontalLine  shortline;
		private bool 	SetAlarm = true;
		#region Button Variables
		private NinjaTrader.Gui.Chart.Chart					chartWindow;
		private NinjaTrader.Gui.Chart.ChartTab				chartTab;
		private System.Windows.Style						chartTraderButtonStyle, systemMenuStyle;
		private System.Windows.Controls.Menu				ntBarMenu;
		private NinjaTrader.Gui.Tools.NTMenuItem			myButton;
		private bool										ntBarActive;
		private System.Windows.Controls.TabItem				tabItem;
		#endregion
		
		#endregion
		
		#region OnStateChange	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= "";
				Name										= "ShortLine";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				SetAlarm									= true;
				NChar										= 0;
				//LineThickness 								= 1;

				TextBrush = System.Windows.Media.Brushes.Crimson;
				LineBrush = System.Windows.Media.Brushes.HotPink;

			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
							if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						CreateWPFControls();
					}));
				}
			}
							 else if (State == State.Terminated)
				 {
					 
if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						DisposeWPFControls();
					}));
				}  
        }	
		}
#endregion		


		#region OnBarUpdate
		protected override void OnBarUpdate()
		{
			if (CurrentBar < ChartBars.ToIndex)				
				return;
			if (SetUp_Line_bool)
	{
			if(CurrentBar == ChartBars.ToIndex)
		{
			shortline = Draw.HorizontalLine(this,"shortline",Close[0] - TickSize * 10,LineBrush,DashStyleHelper.DashDotDot,2);
			//shortline = Draw.HorizontalLine(this,"shortline",Close[0]-TickSize *10,Brushes.Crimson);
			shortline.IsLocked = false;
			SetUp_Line_bool = false;		
		}
	}
				last = Close[0];//required
		}///onbarupdate
	#endregion	
		
	
		
		#region Add Button as Menu
		
				#region Create Buttons ----  WPF  Controls
		protected void CreateWPFControls()
		{
			// the main chart window
			chartWindow			= System.Windows.Window.GetWindow(ChartControl.Parent) as Chart;
			if (chartWindow == null)
				return;
	
			ntBarMenu = new System.Windows.Controls.Menu
			{
				// important to set the alignment, otherwise you will never see the menu populated
				VerticalAlignment			= VerticalAlignment.Top,
				VerticalContentAlignment	= VerticalAlignment.Top,
				// make sure to style as a System Menu	
				Style = System.Windows.Application.Current.TryFindResource("SystemMenuStyle") as Style
				
			};

			// this is the menu item which will appear on the chart's Main Menu
			myButton = new NTMenuItem()
			{
				Header				= "S",
				//Icon				= topMenuItem1Icon,
				Margin				= new Thickness(0),
				Padding				= new Thickness(1),
				VerticalAlignment	= VerticalAlignment.Center,
				//Style				= mainMenuItemStyle
				Foreground 			= Brushes.Red,
			};
			ntBarMenu.Items.Add(myButton);
			myButton.Click += OnMouseClick;

			// add the menu which contains all menu items to the chart
			chartWindow.MainMenu.Add(ntBarMenu);		
			if (TabSelected())
				ShowWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}
			
				#endregion
		
				#region remove handlers / dispose objects
		private void DisposeWPFControls()
		{
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			HideWPFControls();
			myButton.Click -= OnMouseClick;

			if (ntBarMenu != null)
			{
				chartWindow.MainMenu.Remove(ntBarMenu);
				ntBarActive = false;
			}
		}
			#endregion

		
				#region On Mouse Click better? - Disable Line/Alarm
		
				private void OnMouseClick(object s, EventArgs e)
	{	
				if (DrawObjects["shortline"] == null)
					shortline = Draw.HorizontalLine(this,"shortline",Close[0] - TickSize * 10,Brushes.HotPink,DashStyleHelper.DashDotDot,2);
	
		
	if (DrawObjects["shortline"] != null && DrawObjects["shortline"] is DrawingTools.HorizontalLine)
  {
	  if(DrawObjects["shortline"].IsVisible)
	  {
		  DrawObjects["shortline"].IsVisible = false;
		  IsVisible = false;//removes the price text
		  myButton.Foreground = Brushes.Red;
	  }
	  else
	  {
		  DrawObjects["shortline"].IsVisible = true;
		  IsVisible = true;
		  myButton.Foreground = Brushes.Gray;
	  }
  }  

				  ForceRefresh();
	
	}
	#endregion

				
				#region  hide controls.
		private void HideWPFControls()
		{
			if (ntBarActive)
			{
				myButton.Visibility	= Visibility.Collapsed;
				ntBarActive					= false;
			}
		}
			#endregion
			
				#region insert controls


		private void ShowWPFControls()
		{
			if (!ntBarActive)
			{
				myButton.Visibility	= Visibility.Visible;
				ntBarActive					= true;
			}
		}
			#endregion
		
				#region Tab selection and handler
		private bool TabSelected()
		{
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			// full qualified namespaces used here to show where these tools are
			foreach (TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}

		// Runs ShowWPFControls if this is the selected chart tab, other wise runs HideWPFControls()
		private void TabChangedHandler(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as ChartTab;
			if (chartTab == null)
				return;

//			if (TabSelected())
//				try { ShowWPFControls(); }
//				catch (System.Exception ef) { Print(ef.ToString()); }
//			else
//				HideWPFControls();
		}
#endregion
#endregion
		
		#region OnRender	
	protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
				foreach (DrawingTool draw in DrawObjects.ToList())
		{
if (draw is DrawingTools.HorizontalLine)
{
DrawingTools.HorizontalLine temp = draw as DrawingTools.HorizontalLine;
	
	if(DrawObjects["shortline"] != null && 
		DrawObjects["shortline"] is DrawingTools.HorizontalLine)
	{
		if(draw.Tag == "shortline")		
		{
	      	 myPrice1 = Instrument.MasterInstrument.RoundToTickSize(temp.StartAnchor.Price);
			pricestr = myPrice1.ToString().Remove(0,NChar);

			SharpDX.Direct2D1.Brush textBrushDx;
			textBrushDx = textBrush.ToDxBrush(RenderTarget);
			
			NinjaTrader.Gui.Tools.SimpleFont simpleFont = chartControl.Properties.LabelFont ??  new NinjaTrader.Gui.Tools.SimpleFont("Arial", 12);
			SharpDX.DirectWrite.TextFormat textFormat1 = simpleFont.ToDirectWriteTextFormat();
			
			SharpDX.DirectWrite.TextFormat textFormat2 =
			new SharpDX.DirectWrite.TextFormat(NinjaTrader.Core.Globals.DirectWriteFactory, "Verdana", SharpDX.DirectWrite.FontWeight.Bold,
			SharpDX.DirectWrite.FontStyle.Italic, fontSize);
			
			SharpDX.DirectWrite.TextLayout textLayout2 =
			new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
			pricestr, textFormat2, 400, textFormat1.FontSize);
			
		 	SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(5,
		 	ChartPanel.Y + ((float)temp.StartAnchor.GetPoint(chartControl, ChartPanel, chartScale).Y)-(float)textLayout2.Metrics.Height);
			RenderTarget.DrawTextLayout(lowerTextPoint, textLayout2, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
	}
	}		
		}	///ishorizontalline	
		}///foreach
					
		if ( last < myPrice1)
	{
		PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav");
			//PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\handbell.wav");	

	}/// if last> price
		}///onrender
		
		#endregion	
		
		#region Properties
		[Description("Set the size of the display font")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Font Size", GroupName = "Parameters", Order = 0)]
		public float FontSize
		{
			get { return fontSize; }
			set { fontSize = value; }
		}
	    [Display(Name="Price characters to remove", Description="", Order=8, GroupName="Parameters")]
		[RefreshProperties(RefreshProperties.All)]
		public int NChar
		{ get; set; }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TextColor", GroupName = "Parameters")]
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
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Line Color", GroupName = "Parameters")]
		public System.Windows.Media.Brush LineBrush
		{
			get { return lineBrush; }
			set { lineBrush = value; }
		}
	
		[Browsable(false)]
		public string LineBrushSerialize
		{
			get { return Serialize.BrushToString(LineBrush); }
			set { LineBrush = Serialize.StringToBrush(value); }
		}
	#endregion 
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Mindset.ShortLine[] cacheShortLine;
		public Mindset.ShortLine ShortLine(float fontSize)
		{
			return ShortLine(Input, fontSize);
		}

		public Mindset.ShortLine ShortLine(ISeries<double> input, float fontSize)
		{
			if (cacheShortLine != null)
				for (int idx = 0; idx < cacheShortLine.Length; idx++)
					if (cacheShortLine[idx] != null && cacheShortLine[idx].FontSize == fontSize && cacheShortLine[idx].EqualsInput(input))
						return cacheShortLine[idx];
			return CacheIndicator<Mindset.ShortLine>(new Mindset.ShortLine(){ FontSize = fontSize }, input, ref cacheShortLine);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Mindset.ShortLine ShortLine(float fontSize)
		{
			return indicator.ShortLine(Input, fontSize);
		}

		public Indicators.Mindset.ShortLine ShortLine(ISeries<double> input , float fontSize)
		{
			return indicator.ShortLine(input, fontSize);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Mindset.ShortLine ShortLine(float fontSize)
		{
			return indicator.ShortLine(Input, fontSize);
		}

		public Indicators.Mindset.ShortLine ShortLine(ISeries<double> input , float fontSize)
		{
			return indicator.ShortLine(input, fontSize);
		}
	}
}

#endregion
