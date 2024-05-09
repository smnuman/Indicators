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
	
	public class TradeSimpleInsideBarCombo : Indicator
	{
		
		private const string SystemVersion 					= " V1.0";
        private const string SystemName 					= "TradeSimple InsideBar Combo";
		
		private string youtube								= "https://youtu.be/VvRimpQ1meI";
		private string discord								= "https://discord.gg/2YU9GDme8j";

		private double priceOffset 							= 0.00;
		private double percentageOffset 					= 0.00;
		private int tickOffset								= 0;
		
		private bool colorInsideBar;
		private bool setIBFlash;
		private bool setVolumeAlert;
		
		private bool currentIB;
		private bool alertIB;
		
		private bool setChartIB;
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
				Description									= @"Will color Inside Bars based on custom parameters";
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
				
				///Default colors for Inside Bars
				colorInsideBar								= true;
				GreenInsideBar 								= Brushes.DarkCyan;
				RedInsideBar								= Brushes.Indigo;
				
				///Inside Bar Alerts
				CustomSoundFile								= @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert2.Wav";
				SetVolumeAlert								= true;
				SetIBFlash									= true;
				FlashBrush 									= Brushes.Green;
				
				///Current or Previous Inside Bars condition is true
				currentIB									= false;
				alertIB										= false;
				
				///Use a different setting when using Market analyzer to prevent interference. See explanation video linked
				setChartIB									= true;
				setMarketAnalyzer							= false;
				
				///Plot for Market Analyzer settings
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Dot, "CurrentInsideBar");
				
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
			
			//Percent for the previous Inside Bar
			double percentCalcAlert = (High[2] - Low[2]) * percentageOffset;
			 
			//Percent for the current IB forming
			double percentageCalc = (High[1] - Low[1]) * percentageOffset;
			
			//These are the same for both because they do not rely on another candle
			double priceCalc = 	priceOffset;
			double tickCalc = TickSize * tickOffset;
			
			//Picks Highest Offset for the Current IB
			double insideBarOffset = Math.Max(percentageCalc, Math.Max(priceCalc, tickCalc));
			
			//Picks Highest Offset for previous IB (Alert IB's)
			double insideBarOffsetAlert = Math.Max(percentCalcAlert, Math.Max(priceCalc, tickCalc));
	
	#endregion		
		
	#region Current Inside Bar Logic
		
			//Currently an Inside Bar	
		if ((High[0]-(insideBarOffset)) <= High[1] 
				&& (Low[0]+(insideBarOffset)) >= Low[1])
			
			{
				currentIB = true;
			}
				else
			{
				currentIB = false;	
			}
		
	#endregion
		
	#region Previous Inside Bar Logic(Alerts)
			
		//Previous Bar was an Inside Bar (Once bar Closes it determines if it was an IB)
		if ((High[1]-(insideBarOffsetAlert)) <= High[2] 
				&& (Low[1]+(insideBarOffsetAlert)) >= Low[2])
			
			{
				alertIB = true;	
			}
			
				else 
			{
				alertIB = false;
			}
		#endregion
			
	if (setChartIB)
	{
		
	#region Color Inside Bars
			
	///Inside Bar Color
		if (colorInsideBar)
		{
			//Red Inside Bar Logic
			if ((currentIB) 
					&& (Open[0] > Close[0]))
				{
					BarBrush = RedInsideBar;
				}
			
			//Green Inside Bar Logic
			else if ((currentIB) 
						&& (Open[0] < Close[0]))
				{
					BarBrush = GreenInsideBar;
				}
		}
	
	#endregion
		
	#region Flash Alert
		
		///Flash Alert (Once Bar Closes)
			if ((SetIBFlash)
					&& (alertIB))
			{
				BackBrushes[1] = FlashBrush;
			}
			
	#endregion
			
	#region Sound Alert
			
		///Sound Alert (Once Bar Closes)	
			if ((SetVolumeAlert) 
					&& (alertIB)
						&& (IsFirstTickOfBar))
						
			{
				PlaySound(CustomSoundFile);	
			}
			
	#endregion
			
	}	
	
	#region Market Analyzer Alert
			
			if (setMarketAnalyzer)
			{
			 
				if (currentIB)
				{
				CurrentInsideBar[0] = 1;
				}
				
			else
				{
				CurrentInsideBar[0] = 0;
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
	///Offset for Inside Bars. Allows Price to come outside of the High/Low of the previous candle if the user chooses.
		
		[Display(Name = "Price Offset", GroupName = "1. Inside Bar Offset", Order = 0)]
		public double PriceOffset
		{
			get{return priceOffset;}
			set{priceOffset = (value);}
		}
			
		[Display(Name = "Percentage Offset", GroupName = "1. Inside Bar Offset", Order = 1)]
		public double PercentageOffset
		{
			get{return percentageOffset;}
			set{percentageOffset = (value);}
		}
		
		[Display(Name = "Tick Offset", GroupName = "1. Inside Bar Offset", Order = 2)]
		public int TickOffset
		{
			get{return tickOffset;}
			set{tickOffset = (value);}
		}
		#endregion
		
		#region 2. Color Inside Bars
	///Change the color of the Green and Red Inside Bars.
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Color Inside Bars", Description = "", Order = 0, GroupName = "2. Inside Bar Custom Color")]
		public bool ColorInsideBar 
		{
		 	get{return colorInsideBar;} 
			set{colorInsideBar = (value);} 
		}
		
		[XmlIgnore()]
		[Display(Name = "Green Inside Bar", GroupName = "2. Inside Bar Custom Color", Order = 1)]
		public Brush GreenInsideBar
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string GreenInsideBarSerialize
		{
			get { return Serialize.BrushToString(GreenInsideBar); }
   			set { GreenInsideBar = Serialize.StringToBrush(value); }
		}
		
			[XmlIgnore()]
		[Display(Name = "Red Inside Bar", GroupName = "2. Inside Bar Custom Color", Order = 2)]
		public Brush RedInsideBar
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string RedInsideBarSerialize
		{
			get { return Serialize.BrushToString(RedInsideBar); }
   			set { RedInsideBar = Serialize.StringToBrush(value); }
		}
		
		#endregion
		
		#region 3. Flash Alert
	///Enable Flash Alert and Change color 	
		
		[Display(ResourceType = typeof (Custom.Resource), Name = "Enable Inside Bar Flash", Order = 0, GroupName = "3. Flash Alert")]
		public bool SetIBFlash
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
		[Display(Name = "Use with Chart Only", Order = 1, GroupName = "5. Inside Bar Indicator")]
		public bool SetChartIB
		{
		    get
		   {
		      return setChartIB;
		   }
		   set
		   {
		      if (value == true)
		      {
		         SetMarketAnalyzer = false;
		      }
		      setChartIB = value;
		    }
		}

		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]
		[Display(Name = "Use with Market Analyzer Only", Order = 2, GroupName = "5. Inside Bar Indicator")]
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
		         SetChartIB = false;
		      }
		      setMarketAnalyzer = value;
		   }
		}
				
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CurrentInsideBar
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
		private TradeSimple.TradeSimpleInsideBarCombo[] cacheTradeSimpleInsideBarCombo;
		public TradeSimple.TradeSimpleInsideBarCombo TradeSimpleInsideBarCombo(bool colorInsideBar, bool setChartIB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			return TradeSimpleInsideBarCombo(Input, colorInsideBar, setChartIB, setMarketAnalyzer, showSocials, youtube, discord);
		}

		public TradeSimple.TradeSimpleInsideBarCombo TradeSimpleInsideBarCombo(ISeries<double> input, bool colorInsideBar, bool setChartIB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			if (cacheTradeSimpleInsideBarCombo != null)
				for (int idx = 0; idx < cacheTradeSimpleInsideBarCombo.Length; idx++)
					if (cacheTradeSimpleInsideBarCombo[idx] != null && cacheTradeSimpleInsideBarCombo[idx].ColorInsideBar == colorInsideBar && cacheTradeSimpleInsideBarCombo[idx].SetChartIB == setChartIB && cacheTradeSimpleInsideBarCombo[idx].SetMarketAnalyzer == setMarketAnalyzer && cacheTradeSimpleInsideBarCombo[idx].ShowSocials == showSocials && cacheTradeSimpleInsideBarCombo[idx].Youtube == youtube && cacheTradeSimpleInsideBarCombo[idx].Discord == discord && cacheTradeSimpleInsideBarCombo[idx].EqualsInput(input))
						return cacheTradeSimpleInsideBarCombo[idx];
			return CacheIndicator<TradeSimple.TradeSimpleInsideBarCombo>(new TradeSimple.TradeSimpleInsideBarCombo(){ ColorInsideBar = colorInsideBar, SetChartIB = setChartIB, SetMarketAnalyzer = setMarketAnalyzer, ShowSocials = showSocials, Youtube = youtube, Discord = discord }, input, ref cacheTradeSimpleInsideBarCombo);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSimple.TradeSimpleInsideBarCombo TradeSimpleInsideBarCombo(bool colorInsideBar, bool setChartIB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			return indicator.TradeSimpleInsideBarCombo(Input, colorInsideBar, setChartIB, setMarketAnalyzer, showSocials, youtube, discord);
		}

		public Indicators.TradeSimple.TradeSimpleInsideBarCombo TradeSimpleInsideBarCombo(ISeries<double> input , bool colorInsideBar, bool setChartIB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			return indicator.TradeSimpleInsideBarCombo(input, colorInsideBar, setChartIB, setMarketAnalyzer, showSocials, youtube, discord);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSimple.TradeSimpleInsideBarCombo TradeSimpleInsideBarCombo(bool colorInsideBar, bool setChartIB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			return indicator.TradeSimpleInsideBarCombo(Input, colorInsideBar, setChartIB, setMarketAnalyzer, showSocials, youtube, discord);
		}

		public Indicators.TradeSimple.TradeSimpleInsideBarCombo TradeSimpleInsideBarCombo(ISeries<double> input , bool colorInsideBar, bool setChartIB, bool setMarketAnalyzer, bool showSocials, string youtube, string discord)
		{
			return indicator.TradeSimpleInsideBarCombo(input, colorInsideBar, setChartIB, setMarketAnalyzer, showSocials, youtube, discord);
		}
	}
}

#endregion
