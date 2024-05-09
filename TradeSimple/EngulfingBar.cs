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
namespace NinjaTrader.NinjaScript.Indicators.TradeSimple
{
	public class EngulfingBar : Indicator
	{
		private const string SystemVersion 					= " V1.0";
        private const string SystemName 					= "EngulfingBar";
		
		private string youtube								= "https://youtu.be/YChEpDxJF_A";
		private string discord								= "https://discord.gg/2YU9GDme8j";

		private double priceOffset 							= 0.01;
		private double percentageOffset 					= 0.00;
		private int tickOffset								= 1;
		
		private bool engulfBody;
		private bool currentBodyEngulfGreen;
		private bool currentBodyEngulfRed;
		
		private bool alertBodyEngulfGreen;
		private bool alertBodyEngulfRed;
		
		private bool colorOutsideBar;
		private bool setOBFlash;
		private bool setVolumeAlert;
		
		private bool currentOBGreen;
		private bool currentOBRed;
		
		private bool alertOBGreen;
		private bool alertOBRed;
		
		private bool setChartOB;
		private bool setMarketAnalyzer;
		
		
		#region TradeSimple Social
		
		private bool showSocials;
		
		private bool youtubeButtonClicked;
		private bool discordButtonClicked;
		
		private System.Windows.Controls.Button youtubeButton;
		private System.Windows.Controls.Button discordButton;
		
		
		private System.Windows.Controls.Grid myGrid29;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Will color Outside/Engulfing bars";
				Name										= SystemName + SystemVersion;
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
				
		
				///Default colors for Outside Bars
				colorOutsideBar								= true;
				GreenOutsideBar 							= Brushes.Aquamarine;
				RedOutsideBar								= Brushes.DarkSalmon;
				
				///Outside Bar Alerts
				CustomSoundFile								= @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert2.Wav";
				SetVolumeAlert								= true;
				SetOBFlash									= true;
				FlashBrush 									= Brushes.Orange;
				
				///Current or Previous Outside Bars condition is true
				currentOBGreen								= false;
				currentOBRed								= false;
				
				alertOBGreen								= false;
				alertOBRed								 	= false;
				
				engulfBody									= false;
				currentBodyEngulfGreen						= false;
				currentBodyEngulfRed						= false;
				
				alertBodyEngulfGreen						= false;
				alertBodyEngulfRed							= false;
				
				
				///Use a different setting when using Market analyzer to prevent interference. See explanation video linked
				setChartOB									= true;
				setMarketAnalyzer							= false;
				
				///Plot for Market Analyzer settings
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Dot, "CurrentOutsideBar");
				
				///Will show buttons with links to Youtube/Discord.
				showSocials									= true;
				
				
				
			}
			else if (State == State.Configure)
			{
			}
			
		#region Add Buttons with Links
			
		else if (State == State.Historical)
		{
			if (showSocials)
			{
				if (UserControlCollection.Contains(myGrid29))
					return;
				
				Dispatcher.InvokeAsync((() =>
				{
					myGrid29 = new System.Windows.Controls.Grid
					{
						Name = "MyCustomGrid", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom
					};
					
					System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
					System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
					
					myGrid29.ColumnDefinitions.Add(column1);
					myGrid29.ColumnDefinitions.Add(column2);
					
					youtubeButton = new System.Windows.Controls.Button
					{
						Name = "YoutubeButton", Content = "Youtube", Foreground = Brushes.White, Background = Brushes.Red
					};
					
					discordButton = new System.Windows.Controls.Button
					{
						Name = "DiscordButton", Content = "Discord", Foreground = Brushes.White, Background = Brushes.RoyalBlue
					};
					
					youtubeButton.Click += OnButtonClick;
					discordButton.Click += OnButtonClick;
					
					System.Windows.Controls.Grid.SetColumn(youtubeButton, 0);
					System.Windows.Controls.Grid.SetColumn(discordButton, 1);
					
					myGrid29.Children.Add(youtubeButton);
					myGrid29.Children.Add(discordButton);
					
					UserControlCollection.Add(myGrid29);
				}));
			}
			
			
			else if (State == State.Terminated)
			{
				Dispatcher.InvokeAsync((() =>
				{
					if (myGrid29 != null)
					{
						if (youtubeButton != null)
						{
							myGrid29.Children.Remove(youtubeButton);
							youtubeButton.Click -= OnButtonClick;
							youtubeButton = null;
						}
						
						if (discordButton != null)
						{
							myGrid29.Children.Remove(discordButton);
							discordButton.Click -= OnButtonClick;
							discordButton = null;
						}
								
					}
				}));
			}
		}
		#endregion	
			
		}
		
		protected override void OnBarUpdate()
		{
			
			if(CurrentBar < 2) return;
			
			BarBrush	= null;
			BackBrushes = null;
			
	#region offset Calculations
			
			//Percent for the previous Outside Bar
			double percentCalcAlert = (High[2] - Low[2]) * percentageOffset;
			 
			//Percent for the current OB forming
			double percentageCalc = (High[1] - Low[1]) * percentageOffset;
			
			//These are the same for both because they do not rely on another candle
			double priceCalc = 	priceOffset;
			double tickCalc = TickSize * tickOffset;
			
			//Picks Highest Offset for the Current OB
			double OutsideBarOffset = Math.Max(percentageCalc, Math.Max(priceCalc, tickCalc));
			
			//Picks Highest Offset for previous OB (Alert OB's)
			double OutsideBarOffsetAlert = Math.Max(percentCalcAlert, Math.Max(priceCalc, tickCalc));
	
	#endregion		
		
	#region Current Outside Bar Logic
		
		///Current Outside Bars 'Without' Engulf Body Selected	
			
			//Currently a Green Outside Bar	
			if (
				(engulfBody == false)
					&& ((High[0]-(OutsideBarOffset)) >= High[1] 
					&& (Low[0]+(OutsideBarOffset)) <= Low[1])
						&& (Open[0] < Close[0])
							&& (Open[1] > Close[1])
				)
			
			{
				currentOBGreen = true;
			}
				else
			{
				currentOBGreen = false;	
			}
			
			
			//Currently a Green Outside Bar	
			if (
				(engulfBody == false)
					&& ((High[0]-(OutsideBarOffset)) >= High[1] 
					&& (Low[0]+(OutsideBarOffset)) <= Low[1])
						&& (Open[0] > Close[0])
							&& (Open[1] < Close[1])
				)
			
			{
				currentOBRed = true;
			}
				else
			{
				currentOBRed = false;	
			}
			
		///Current Outside Bars with Engulf Body Selected	
			
			//Green Outside Bar w/ Engulf Body
			if (
				(engulfBody)
					&&((High[0]-(OutsideBarOffset)) >= High[1]) 
					&& ((Low[0]+(OutsideBarOffset)) <= Low[1])
						&& (Open[0] < Close[0])
							&& (Open[1] > Close[1])
								&& ((Open[0] <= Close[1]) && Close[0] >= Open[1])
				)
			
			{
				currentBodyEngulfGreen = true;
			}
				else
			{
				currentBodyEngulfGreen = false;
			}
			
			//Red Outside Bar w/ Engulf Body
			if (
				(engulfBody)
					&&((High[0]-(OutsideBarOffset)) >= High[1]) 
					&& ((Low[0]+(OutsideBarOffset)) <= Low[1])
						&& (Open[0] > Close[0])
							&& (Open[1] < Close[1])
								&& ((Open[0] >= Close[1]) && Close[0] <= Open[1])
				)
			
			{
				currentBodyEngulfRed = true;
			}
				else
			{
				currentBodyEngulfRed = false;
			}
			
			
			
			
		
	#endregion
		
	#region Previous Outside Bar Logic(Alerts)
			
		///Previous Bar was an Outside Bar (Once bar Closes it determines if it was an OB)
		
			//Green Alert 'Without' Engulf Candle
			if (
			(engulfBody == false)
				&& ((High[1]-(OutsideBarOffsetAlert)) >= High[2] 
				&& (Low[1]+(OutsideBarOffsetAlert)) <= Low[2])
					&& (Open[1] < Close[1])
					&& (Open[2] > Close[2])
			)
			
			{
				alertOBGreen = true;	
			}
			
				else 
			{
				alertOBGreen = false;
			}
			
			//Red Alert 'Without' Engulf Candle
			if (
			(engulfBody == false)
				&& ((High[1]-(OutsideBarOffsetAlert)) >= High[2] 
				&& (Low[1]+(OutsideBarOffsetAlert)) <= Low[2])
					&& (Open[1] > Close[1])
					&& (Open[2] < Close[2])
			)
			
			{
				alertOBRed = true;	
			}
			
				else 
			{
				alertOBRed = false;
			}
			
			
			
			//Green Alert w/ Engulf Candle
			if (
			(engulfBody)
				&& ((High[1]-(OutsideBarOffsetAlert)) >= High[2] 
				&& (Low[1]+(OutsideBarOffsetAlert)) <= Low[2])
					&& (Open[1] < Close[1])
					&& (Open[2] > Close[2])
						&& ((Open[1] <= Close[2]) && Close[1] >= Open[2])
			)
			
			{
				alertBodyEngulfGreen = true;	
			}
			
				else 
			{
				alertBodyEngulfGreen = false;
			}
			
			//Red Alert w/ Engulf Candle
			if (
			(engulfBody)
				&& ((High[1]-(OutsideBarOffsetAlert)) >= High[2] 
				&& (Low[1]+(OutsideBarOffsetAlert)) <= Low[2])
					&& (Open[1] > Close[1])
					&& (Open[2] < Close[2])
						&& ((Open[1] >= Close[2]) && Close[1] <= Open[2])
			)
			
			{
				alertBodyEngulfRed = true;	
			}
			
				else 
			{
				alertBodyEngulfRed = false;
			}
			
		#endregion
			
	
	if (setChartOB)
		{
			
	#region Color Outside Bars
			
	///Outside Bar Color
		if (colorOutsideBar)
		{
			//Green Outside Bar Logic - Current Bar is Green, engulfing a previous Red candle
			if (currentOBGreen) 
				
				{
					BarBrush = GreenOutsideBar;
				}
			
			//Red Outside Bar Logic - Current Bar is Red, engulfing a previous Green candle
			else if (currentOBRed) 
					
				{
					BarBrush = RedOutsideBar;
				}
			
		///Needs body of current candle to be greater than body of previous candle		
		
				//Green Outside Bar Logic - Current Bar is Green, engulfing a previous Red candle
			if (currentBodyEngulfGreen)
					
				{
					BarBrush = GreenOutsideBar;
				}	
			
				//Red Outside Bar Logic - Current Bar is Red, engulfing a previous Green candle 
			else if (currentBodyEngulfGreen)
				
				{
					BarBrush = RedOutsideBar;
				}
	
		}
	
	#endregion
	
	
	#region Flash Alert
		
		///Flash Alert (Once Bar Closes)
			
		if (SetOBFlash)	
			{
				if (alertOBGreen || alertOBRed || alertBodyEngulfGreen || alertBodyEngulfGreen)
				{
					BackBrushes[1] = FlashBrush;
				}
			}
			
	#endregion
			
	#region Sound Alert
			
		///Sound Alert (Once Bar Closes)	
			if (SetVolumeAlert) 
			{
				if ((alertOBGreen || alertOBRed || alertBodyEngulfGreen || alertBodyEngulfGreen)	
						&& (IsFirstTickOfBar))		
				{
					PlaySound(CustomSoundFile);	
				}
			}
	#endregion
			
		}	
	
	#region Market Analyzer Alert
			
			if (setMarketAnalyzer)
			{
			 
				if ((currentOBGreen) || (currentOBRed) || (currentBodyEngulfGreen) || (currentBodyEngulfRed))
					
					{
						CurrentOutsideBar[0] = 1;
					}
	
			}
	#endregion
			
			else
			{
				return;
			}
		}
			
	#region Button Click Event
		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			if (showSocials)
			{
				System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
				if (button == youtubeButton && button.Name == "YoutubeButton" && button.Content == "Youtube")
				{
					System.Diagnostics.Process.Start(youtube);
					return;
				}
				
				if (button == discordButton && button.Name == "DiscordButton" && button.Content == "Discord")
				{	
					System.Diagnostics.Process.Start(discord);
					return;
				}
			}
		}
	
	#endregion	
	
		#region Properties
	
		#region 1. Offset Properties
	///Offset for Outside Bars. Allows Price to come outside of the High/Low of the previous candle if the user chooses.
		
		[Display(Name = "Price Offset", GroupName = "1. Outside Bar Offset", Order = 0)]
		public double PriceOffset
		{
			get{return priceOffset;}
			set{priceOffset = (value);}
		}
			
		[Display(Name = "Percentage Offset", GroupName = "1. Outside Bar Offset", Order = 1)]
		public double PercentageOffset
		{
			get{return percentageOffset;}
			set{percentageOffset = (value);}
		}
		
		[Display(Name = "Tick Offset", GroupName = "1. Outside Bar Offset", Order = 2)]
		public int TickOffset
		{
			get{return tickOffset;}
			set{tickOffset = (value);}
		}
		#endregion
		
		#region 2. Color Outside Bars
	///Change the color of the Green and Red Outside Bars.
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Color Outside Bars", Description = "", Order = 0, GroupName = "2. Outside Bar Custom Color")]
		public bool ColorOutsideBar 
		{
		 	get{return colorOutsideBar;} 
			set{colorOutsideBar = (value);} 
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Engulf Body of previous candle", Description = "Body of the current candle must be greater than body of the previous candle", Order = 1, GroupName = "2. Outside Bar Custom Color")]
		public bool EngulfBody 
		{
		 	get{return engulfBody;} 
			set{engulfBody = (value);} 
		}
		
		[XmlIgnore()]
		[Display(Name = "Bullish Outside Bar", GroupName = "2. Outside Bar Custom Color", Order = 2)]
		public Brush GreenOutsideBar
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string GreenOutsideBarSerialize
		{
			get { return Serialize.BrushToString(GreenOutsideBar); }
   			set { GreenOutsideBar = Serialize.StringToBrush(value); }
		}
		
			[XmlIgnore()]
		[Display(Name = "Bearish Outside Bar", GroupName = "2. Outside Bar Custom Color", Order = 3)]
		public Brush RedOutsideBar
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string RedOutsideBarSerialize
		{
			get { return Serialize.BrushToString(RedOutsideBar); }
   			set { RedOutsideBar = Serialize.StringToBrush(value); }
		}
		
		#endregion
		
		#region 3. Flash Alert
	///Enable Flash Alert and Change color 	
		
		[Display(ResourceType = typeof (Custom.Resource), Name = "Enable Outside Bar Flash", Order = 0, GroupName = "3. Flash Alert")]
		public bool SetOBFlash
		{ get; set; }	
		
		[XmlIgnore()]
		[Display(Name = "Flash Brush", GroupName = "3. Flash Alert", Order = 1)]
		public Brush FlashBrush
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string FlashBrushSerialize
		{
			get { return Serialize.BrushToString(FlashBrush); }
   			set { FlashBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		
		#region 4. Sound Alerts
	///Enable Sound Alert	
		
		[Display(ResourceType = typeof (Custom.Resource), Name = "Enable Volume Alert", Order = 0, GroupName = "4. Audio Alert")]
		public bool SetVolumeAlert
		{ get; set; }
		
		
		[Display(Name="Alert sound file", Description="Enter sound file path/name", Order=1, GroupName="4. Audio Alert")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		public string CustomSoundFile
		{ get; set; }
		
		#endregion
		
		#region 5. Market Analyer Mode
		
		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]
		[Display(Name = "Use with Chart Only", Order = 1, GroupName = "5. Outside Bar Indicator")]
		public bool SetChartOB
		{
		    get
		   {
		      return setChartOB;
		   }
		   set
		   {
		      if (value == true)
		      {
		         SetMarketAnalyzer = false;
		      }
		      setChartOB = value;
		    }
		}

		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]
		[Display(Name = "Use with Market Analyzer Only", Order = 2, GroupName = "5. Outside Bar Indicator")]
		public bool SetMarketAnalyzer
		{
		   get
		   {
		      return setMarketAnalyzer;
		   }
		   set
		   {
		      if (value == true)
		      {
		         SetChartOB = false;
		      }
		      setMarketAnalyzer = value;
		   }
		}
				
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CurrentOutsideBar
		{
			get { return Values[0]; }
		}
		
		#endregion
		
		#region 9. Links/Buttons
		
		[NinjaScriptProperty]
		[Display(Name = "Show Social Media Buttons", Description = "", Order = 0, GroupName = "9. TradeSimple Dre's Links")]
		public bool ShowSocials 
		{
		 	get{return showSocials;} 
			set{showSocials = (value);} 
		}
		
		[NinjaScriptProperty]
		[Display(Name="Explanation Video", Order=1, GroupName="9. TradeSimple Dre's Links")]
		public  string Youtube
		{
		 	get{return youtube;} 
			set{youtube = (value);} 
		}
		
		[NinjaScriptProperty]
		[Display(Name="Discord Link", Order=2, GroupName="9. TradeSimple Dre's Links")]
		public  string Discord
		{
		 	get{return discord;} 
			set{discord = (value);} 
		}
		
		
		#endregion
		
		#endregion
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeSimple.EngulfingBar[] cacheEngulfingBar;
		public TradeSimple.EngulfingBar EngulfingBar(bool colorOutsideBar, bool engulfBody, bool setChartOB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			return EngulfingBar(Input, colorOutsideBar, engulfBody, setChartOB, setMarketAnalyzer, showSocials, youtube, discord);
		}

		public TradeSimple.EngulfingBar EngulfingBar(ISeries<double> input, bool colorOutsideBar, bool engulfBody, bool setChartOB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			if (cacheEngulfingBar != null)
				for (int idx = 0; idx < cacheEngulfingBar.Length; idx++)
					if (cacheEngulfingBar[idx] != null && cacheEngulfingBar[idx].ColorOutsideBar == colorOutsideBar && cacheEngulfingBar[idx].EngulfBody == engulfBody && cacheEngulfingBar[idx].SetChartOB == setChartOB && cacheEngulfingBar[idx].SetMarketAnalyzer == setMarketAnalyzer && cacheEngulfingBar[idx].ShowSocials == showSocials && cacheEngulfingBar[idx].Youtube == youtube && cacheEngulfingBar[idx].Discord == discord && cacheEngulfingBar[idx].EqualsInput(input))
						return cacheEngulfingBar[idx];
			return CacheIndicator<TradeSimple.EngulfingBar>(new TradeSimple.EngulfingBar(){ ColorOutsideBar = colorOutsideBar, EngulfBody = engulfBody, SetChartOB = setChartOB, SetMarketAnalyzer = setMarketAnalyzer, ShowSocials = showSocials, Youtube = youtube, Discord = discord }, input, ref cacheEngulfingBar);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSimple.EngulfingBar EngulfingBar(bool colorOutsideBar, bool engulfBody, bool setChartOB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			return indicator.EngulfingBar(Input, colorOutsideBar, engulfBody, setChartOB, setMarketAnalyzer, showSocials, youtube, discord);
		}

		public Indicators.TradeSimple.EngulfingBar EngulfingBar(ISeries<double> input , bool colorOutsideBar, bool engulfBody, bool setChartOB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			return indicator.EngulfingBar(input, colorOutsideBar, engulfBody, setChartOB, setMarketAnalyzer, showSocials, youtube, discord);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSimple.EngulfingBar EngulfingBar(bool colorOutsideBar, bool engulfBody, bool setChartOB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			return indicator.EngulfingBar(Input, colorOutsideBar, engulfBody, setChartOB, setMarketAnalyzer, showSocials, youtube, discord);
		}

		public Indicators.TradeSimple.EngulfingBar EngulfingBar(ISeries<double> input , bool colorOutsideBar, bool engulfBody, bool setChartOB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			return indicator.EngulfingBar(input, colorOutsideBar, engulfBody, setChartOB, setMarketAnalyzer, showSocials, youtube, discord);
		}
	}
}

#endregion
