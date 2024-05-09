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
namespace NinjaTrader.NinjaScript.Indicators.TradeSaber
{
	public class InsideBarTS : Indicator
	{
			
		private double percentageCalc;
		private double priceCalc;
		private double tickCalc;
		
		//Picks Highest Offset
		private double insideBarOffset;							
		
		private bool colorInsideBar;
		private bool setIBFlash;
		private bool setVolumeAlert;
		
		private bool currentIB;
		
		private bool setChartIB;

		
		private int CalcInt;
		
		
		#region TradeSaber Social
		
		private string author 								= "TradeSaber(Dre)";
		private string version 								= "Version 2.0 // June 2023";
		
		private string youtube								= "https://youtu.be/IqMJCp8N0-4"; 
		private string discord								= "https://discord.gg/2YU9GDme8j";
		private string tradeSaber							= "https://tradesaber.com/";
		
		private bool showSocials;
		
		private bool youtubeButtonClicked;
		private bool discordButtonClicked;
		private bool tradeSaberButtonClicked;
		
		private System.Windows.Controls.Button youtubeButton;
		private System.Windows.Controls.Button discordButton;
		private System.Windows.Controls.Button tradeSaberButton;
		
		
		private System.Windows.Controls.Grid myGrid29;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "InsideBarTS";
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
				
				priceOffset 								= 0.00;
				percentageOffset 							= 0.00;
				tickOffset									= 0;
				
				///Inside Bar Alerts
				CustomSoundFile								= @"C:\Program Files\NinjaTrader 8\sounds\Alert2.wav";
				
				SetVolumeAlert								= false;
				SetIBFlash									= false;
				FlashBrush 									= Brushes.Green;
				
				///Current or Previous Inside Bars condition is true
				currentIB									= false;
				
				setChartIB									= true;
			
				
				///Plot for Market Analyzer settings
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Dot, "CurrentInsideBar");
				
				///Will show buttons with links to Youtube/Discord.
				showSocials									= true;
				
				
				
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				if (Calculate == Calculate.OnBarClose)
				{
					CalcInt = 0;
				}
				
				if (Calculate == Calculate.OnPriceChange || Calculate == Calculate.OnEachTick)
				{
					CalcInt = 1;
				}
			}
			
		#region Add Buttons with Links
			
		else if (State == State.Historical)
		{
			#region TradeSaber Socials
			
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
					System.Windows.Controls.ColumnDefinition column3 = new System.Windows.Controls.ColumnDefinition();
					
					myGrid29.ColumnDefinitions.Add(column1);
					myGrid29.ColumnDefinitions.Add(column2);
					myGrid29.ColumnDefinitions.Add(column3);
					
					youtubeButton = new System.Windows.Controls.Button
					{
						Name = "YoutubeButton", Content = "Youtube", Foreground = Brushes.White, Background = Brushes.Red
					};
					
					discordButton = new System.Windows.Controls.Button
					{
						Name = "DiscordButton", Content = "Discord", Foreground = Brushes.White, Background = Brushes.RoyalBlue
					};
					
					tradeSaberButton = new System.Windows.Controls.Button
					{
						Name = "TradeSaberButton", Content = "TradeSaber", Foreground = Brushes.White, Background = Brushes.DarkOrange
					};
					
					youtubeButton.Click += OnButtonClick;
					discordButton.Click += OnButtonClick;
					tradeSaberButton.Click += OnButtonClick;
					
					System.Windows.Controls.Grid.SetColumn(youtubeButton, 0);
					System.Windows.Controls.Grid.SetColumn(discordButton, 1);
					System.Windows.Controls.Grid.SetColumn(tradeSaberButton, 2);
					
					myGrid29.Children.Add(youtubeButton);
					myGrid29.Children.Add(discordButton);
					myGrid29.Children.Add(tradeSaberButton);
					
					UserControlCollection.Add(myGrid29);
				}));
			}
		#endregion
			
			else if (State == State.Terminated)
			{
				#region Terminate TradeSaber Socials
			
			if (showSocials)
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
						
						if (tradeSaberButton != null)
						{
							myGrid29.Children.Remove(tradeSaberButton);
							tradeSaberButton.Click -= OnButtonClick;
							tradeSaberButton = null;
						}		
					}
				}));
			}
		#endregion
			}
		}
		#endregion	
			
		}
		
		
		public override string DisplayName
		{
        	get 
			{ 
				if  (State == State.SetDefaults) 
				return Name; 
		
				else  return "TradeSaber Inside Bar"; 
			}

		}
		
		protected override void OnBarUpdate()
		{
			
			if(CurrentBars[0] < 2) 
				return;
			
			BarBrush	= null;
			
			
		#region offset Calculations
			
			//Percent for the current IB forming
			percentageCalc = (High[1] - Low[1]) * percentageOffset;
			
			//These are the same for both because they do not rely on another candle
			priceCalc = 	priceOffset;
			tickCalc = TickSize * tickOffset;
			
			//Picks Highest Offset for the Current IB
			insideBarOffset = Math.Max(percentageCalc, Math.Max(priceCalc, tickCalc));

		#endregion		
		
		#region Current Inside Bar Logic
			
			if (
			((High[0]-(insideBarOffset)) <= High[1]) 
				&& ((Low[0]+(insideBarOffset)) >= Low[1])
					&& (Open[0] < Close[0])
			)
			
			{
				CurrentInsideBar[0] = 1;
			}
		
			//Currently an Inside Bar	
			else if (
			((High[0]-(insideBarOffset)) <= High[1]) 
				&& ((Low[0]+(insideBarOffset)) >= Low[1])
					&& (Open[0] > Close[0])
			)
			
			{
				CurrentInsideBar[0] = -1;
			}
				else
			{
				CurrentInsideBar[0] = 0;
			}
		
	#endregion
		
			
		if (setChartIB)
		{
			#region Color Inside Bars
			
	///Inside Bar Color
		if (colorInsideBar)
		{
			//Red Inside Bar Logic
			if (CurrentInsideBar[0] == 1) 
			{
				BarBrush = GreenInsideBar;
			}
			
			//Green Inside Bar Logic
			else if (CurrentInsideBar[0] == -1) 
			{
				BarBrush = RedInsideBar;
			}
				
			else if (CurrentInsideBar[0] == 0)
			{
				BarBrush = null;
			}
		}
	
	#endregion
		}
		
		
			#region Flash Alert
		
		///Flash Alert (Once Bar Closes)
		if ((CurrentInsideBar[CalcInt] == 1 || CurrentInsideBar[CalcInt] == -1) && IsFirstTickOfBar && SetIBFlash)
		{
			BackBrushes[CalcInt] = FlashBrush;
		}
		
		else if (CurrentInsideBar[CalcInt] == 0 && IsFirstTickOfBar && SetIBFlash)
		{
			BackBrushes = null;
		}
		
		
			
	#endregion 
			
			#region Sound Alert
			
		///Sound Alert (Once Bar Closes)	
		if ((CurrentInsideBar[CalcInt] == 1 || CurrentInsideBar[CalcInt] == -1) && IsFirstTickOfBar && SetVolumeAlert)
		{
			PlaySound(CustomSoundFile);	
		}

			
	#endregion
			
		}
			
		#region Button Click Event
		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			
			#region TradeSaber Socials
			
			if (showSocials)
			{
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
				
				if (button == tradeSaberButton && button.Name == "TradeSaberButton" && button.Content == "TradeSaber")
				{	
					System.Diagnostics.Process.Start(tradeSaber);
					return;
				}
			}
			
			#endregion
		}
	
	#endregion	
	
		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CurrentInsideBar
		{ get { return Values[0]; } }
	
		#region 01. Offset Properties
	///Offset for Inside Bars. Allows Price to come outside of the High/Low of the previous candle if the user chooses.
		
		[NinjaScriptProperty]
		[Display(Name = "Price Offset", GroupName = "01. Inside Bar Offset", Order = 0)]
		public double priceOffset
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Percentage Offset", GroupName = "01. Inside Bar Offset", Order = 1)]
		public double percentageOffset
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Tick Offset", Description = "Sensetive Value", Order = 2, GroupName = "01. Inside Bar Offset")]
        public int tickOffset 
		{ get; set; }
		
		
		
		#endregion
		
		#region 02. Color Inside Bars
	///Change the color of the Green and Red Inside Bars.
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Color Inside Bars", Description = "", Order = 0, GroupName = "02. Inside Bar Custom Color")]
		public bool ColorInsideBar 
		{
		 	get{return colorInsideBar;} 
			set{colorInsideBar = (value);} 
		}
		
		[NinjaScriptProperty]
		[XmlIgnore()]
		[Display(Name = "Green Inside Bar", GroupName = "02. Inside Bar Custom Color", Order = 1)]
		public Brush GreenInsideBar
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string GreenInsideBarSerialize
		{
			get { return Serialize.BrushToString(GreenInsideBar); }
   			set { GreenInsideBar = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore()]
		[Display(Name = "Red Inside Bar", GroupName = "02. Inside Bar Custom Color", Order = 2)]
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
		
		#region 03. Flash Alert
	///Enable Flash Alert and Change color 	
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof (Custom.Resource), Name = "Enable Inside Bar Flash", Order = 0, GroupName = "03. Flash Alert")]
		public bool SetIBFlash
		{ get; set; }	
		
		[NinjaScriptProperty]
		[XmlIgnore()]
		[Display(Name = "Flash Brush", GroupName = "03. Flash Alert", Order = 1)]
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
		
		#region 04. Sound Alerts
	///Enable Sound Alert	
		
		[Display(ResourceType = typeof (Custom.Resource), Name = "Enable Volume Alert", Order = 0, GroupName = "04. Audio Alert")]
		public bool SetVolumeAlert
		{ get; set; }
		
		[Display(Name="Alert sound file", Description="Enter sound file path/name", Order=1, GroupName="04. Audio Alert")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		public string CustomSoundFile
		{ get; set; }
		
		#endregion
		
		#region 29. TradeSaber Socials
		
		[NinjaScriptProperty]
		[Display(Name = "Show Social Media Buttons", Description = "", Order = 0, GroupName = "29. TradeSaber Socials")]
		public bool ShowSocials 
		{
		 	get{return showSocials;} 
			set{showSocials = (value);} 
		}
		
		
		[Display(Name="Explanation Video", Order=1, GroupName="29. TradeSaber Socials")]
		public  string Youtube
		{
		 	get{return youtube;} 
			set{youtube = (value);} 
		}
		
		
		[Display(Name="Discord Link", Order=2, GroupName="29. TradeSaber Socials")]
		public  string Discord
		{
		 	get{return discord;} 
			set{discord = (value);} 
		}
		
		
		[Display(Name="TradeSaber Link", Order=3, GroupName="29. TradeSaber Socials")]
		public  string TradeSaber
		{
		 	get{return tradeSaber;} 
			set{tradeSaber = (value);} 
		}
		
		
		[ReadOnly(true)]
		[Display(Name = "Author", GroupName = "29. TradeSaber Socials", Order = 4)]
		public string Author
		{
		 	get{return author;} 
			set{author = (value);} 
		}
		
		
		[ReadOnly(true)]
		[Display(Name = "Version", GroupName = "29. TradeSaber Socials", Order = 5)]
		public string Version
		{
		 	get{return version;} 
			set{version = (value);} 
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
		private TradeSaber.InsideBarTS[] cacheInsideBarTS;
		public TradeSaber.InsideBarTS InsideBarTS(double priceOffset, double percentageOffset, int tickOffset, bool colorInsideBar, Brush greenInsideBar, Brush redInsideBar, bool setIBFlash, Brush flashBrush, bool showSocials)
		{
			return InsideBarTS(Input, priceOffset, percentageOffset, tickOffset, colorInsideBar, greenInsideBar, redInsideBar, setIBFlash, flashBrush, showSocials);
		}

		public TradeSaber.InsideBarTS InsideBarTS(ISeries<double> input, double priceOffset, double percentageOffset, int tickOffset, bool colorInsideBar, Brush greenInsideBar, Brush redInsideBar, bool setIBFlash, Brush flashBrush, bool showSocials)
		{
			if (cacheInsideBarTS != null)
				for (int idx = 0; idx < cacheInsideBarTS.Length; idx++)
					if (cacheInsideBarTS[idx] != null && cacheInsideBarTS[idx].priceOffset == priceOffset && cacheInsideBarTS[idx].percentageOffset == percentageOffset && cacheInsideBarTS[idx].tickOffset == tickOffset && cacheInsideBarTS[idx].ColorInsideBar == colorInsideBar && cacheInsideBarTS[idx].GreenInsideBar == greenInsideBar && cacheInsideBarTS[idx].RedInsideBar == redInsideBar && cacheInsideBarTS[idx].SetIBFlash == setIBFlash && cacheInsideBarTS[idx].FlashBrush == flashBrush && cacheInsideBarTS[idx].ShowSocials == showSocials && cacheInsideBarTS[idx].EqualsInput(input))
						return cacheInsideBarTS[idx];
			return CacheIndicator<TradeSaber.InsideBarTS>(new TradeSaber.InsideBarTS(){ priceOffset = priceOffset, percentageOffset = percentageOffset, tickOffset = tickOffset, ColorInsideBar = colorInsideBar, GreenInsideBar = greenInsideBar, RedInsideBar = redInsideBar, SetIBFlash = setIBFlash, FlashBrush = flashBrush, ShowSocials = showSocials }, input, ref cacheInsideBarTS);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber.InsideBarTS InsideBarTS(double priceOffset, double percentageOffset, int tickOffset, bool colorInsideBar, Brush greenInsideBar, Brush redInsideBar, bool setIBFlash, Brush flashBrush, bool showSocials)
		{
			return indicator.InsideBarTS(Input, priceOffset, percentageOffset, tickOffset, colorInsideBar, greenInsideBar, redInsideBar, setIBFlash, flashBrush, showSocials);
		}

		public Indicators.TradeSaber.InsideBarTS InsideBarTS(ISeries<double> input , double priceOffset, double percentageOffset, int tickOffset, bool colorInsideBar, Brush greenInsideBar, Brush redInsideBar, bool setIBFlash, Brush flashBrush, bool showSocials)
		{
			return indicator.InsideBarTS(input, priceOffset, percentageOffset, tickOffset, colorInsideBar, greenInsideBar, redInsideBar, setIBFlash, flashBrush, showSocials);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber.InsideBarTS InsideBarTS(double priceOffset, double percentageOffset, int tickOffset, bool colorInsideBar, Brush greenInsideBar, Brush redInsideBar, bool setIBFlash, Brush flashBrush, bool showSocials)
		{
			return indicator.InsideBarTS(Input, priceOffset, percentageOffset, tickOffset, colorInsideBar, greenInsideBar, redInsideBar, setIBFlash, flashBrush, showSocials);
		}

		public Indicators.TradeSaber.InsideBarTS InsideBarTS(ISeries<double> input , double priceOffset, double percentageOffset, int tickOffset, bool colorInsideBar, Brush greenInsideBar, Brush redInsideBar, bool setIBFlash, Brush flashBrush, bool showSocials)
		{
			return indicator.InsideBarTS(input, priceOffset, percentageOffset, tickOffset, colorInsideBar, greenInsideBar, redInsideBar, setIBFlash, flashBrush, showSocials);
		}
	}
}

#endregion
