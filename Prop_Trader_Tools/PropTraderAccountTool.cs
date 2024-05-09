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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Prop_Trader_Tools
{
	
	
	
	public class PropTraderAccountTool : Indicator
	{
		
		private NinjaTrader.Gui.Tools.QuantityUpDown quantitySelector;
		private NinjaTrader.Gui.Tools.AccountSelector accountSelector;
		private Account lastAccount = null;
		private int qSelected;
		private Account selectedAccount = null;
		
		private Order current_stopOrder= null;
		private Order current_profitOrder= null;
		private Order _beStop = null;
		private Order _trStop = null;
		
		private string ocoString;
		private bool LongsOnly = false;
		private bool ShortsOnly = false;
		private Position position;
		private Position _pos;
		private Order _WorkingOrder = null;
		private int lastMarketPositionQuantity = 1;
		private double lastProfitOrderPrice;
		
		NinjaTrader.Gui.Tools.SimpleFont _txtFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial Black", 10) { Size = 10, Bold = false };
		private CultureInfo culture = new CultureInfo("en-US");
		
		private System.Windows.Controls.Button		_autotradingButton;
		private System.Windows.Controls.Button		_tressholdButton;
		private System.Windows.Controls.Button		_accountButton;
		private System.Windows.Controls.Button		_trailingButton;
		private System.Windows.Controls.Button		_dayPnLButton;
		private System.Windows.Controls.Button		_exitButton;
		private System.Windows.Controls.Button		_bracketButton;
		private System.Windows.Controls.Button		_positionStopButton;
		private System.Windows.Controls.Button		_beButton;
		private System.Windows.Controls.Button		_trButton;

		private System.Windows.Controls.Grid		buttonGrid;
		private bool								buttonsHidden;
		private Brush								originalPlotColor;
		
		private bool buttonClicked = false;
		private bool _chase_buttonClicked = false;
		private bool _tr_buttonClicked = false;
		private bool _be_buttonClicked = false;
		private bool _bracketButtonClicked = false;
		
		private double realizedPnL;
		private double unRealizedPnL;
		private double cashValue;
		private double netValue;
		private double netLiq;
		private double totalPnL;
		private double grossRealizedPnL;
		private double dayStart;
		private double high_mark;
		private double low_mark;
		private double AutoTressLiq;
		private double AvailableLoss;
		private double AccountLiquidationTresholdValue;
		private string	path;
		private string lastUpdate;
		private double readedTreshold;
		private static void AddText(FileStream fs, string value)
	    {
	        byte[] info = new UTF8Encoding(true).GetBytes(value);
	        fs.Write(info, 0, info.Length);
	    }
		private double fastInfo_unRealizedPnL;
		private double ddMax;
		private bool Auto_Trading = false;
		private double totalDayPnL;
		private bool RunOnce = true;
		double tOtal;
		Account accCheck;
		double TRD_price_1;
		double TRD_price_2;
		double TRD_price_3;
		
		double curr_TRD_price_1;
		double curr_TRD_price_2;
		double curr_TRD_price_3;
		double lastKnownDD = 0;
		double _lastLowMark;
		private bool BE_Stop_Function = false;
		private bool TR_Stop_Function = false;
		private bool Chase_Function = false;
		private Order bracketStop = null;
		private Order bracketProfit = null;
		private SimpleFont ddFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 10) { Size = 10, Bold = true };
	
		protected override void OnStateChange()
		{
			
			
			if (State == State.SetDefaults)
			{
				Description									= @"Join for more tools -> https://discord.gg/gB75nGrzZx";
				Name										= "Prop Trader Account Tool";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				
				ThisTool = "https://discord.gg/gB75nGrzZx";
				//---
				AccountStartValue 							= 50000;	
				Prop_DD 									= 2500;	
				Prop_AutoLiquidationValue 					= 47500;
				ResetAccountInfo 							= false;			
				stoplossExposureWarning 					= 0.6;
				
				usePredSL								=64;
				usePredTP								=64;
				
				 _be_ticks_offset	=	10;
				 _tr_start          =   16;
				 _tr_offset		=   10;
		
				Show_Trailing_DD_ChartLines = true;
				Show_Trailing_DD_flat = true;
				Line_1 = 10;
				Line_2 = 30;
				Line_3 = 80;
				
	
			}
			else if (State == State.Configure)
			{
				RunOnce = true;
				
			}
			else if (State == State.DataLoaded)
			{	
				
				RunOnce = true;
				lock (Account.All)
				if (selectedAccountName != null  )selectedAccount = Account.All.FirstOrDefault(a => a.Name == selectedAccountName);
				if (selectedAccount != null)
				{
					selectedAccount.AccountItemUpdate += OnAccountItemUpdate;
					selectedAccount.PositionUpdate 	+= OnPositionUpdate;
					selectedAccount.OrderUpdate 	+= OnOrderUpdate;
					selectedAccount.ExecutionUpdate 	+= OnExecutionUpdate;
				
					realizedPnL = selectedAccount.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
					cashValue = selectedAccount.Get(AccountItem.CashValue, Currency.UsDollar);
					netLiq = selectedAccount.Get(AccountItem.NetLiquidation, Currency.UsDollar);
					unRealizedPnL = selectedAccount.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
					grossRealizedPnL =  selectedAccount.Get(AccountItem.GrossRealizedProfitLoss, Currency.UsDollar);
					totalDayPnL = realizedPnL + unRealizedPnL;
					
					path = NinjaTrader.Core.Globals.UserDataDir + selectedAccount.Name+".txt";
					lastUpdate = DateTime.Now.ToLongDateString() + " "+DateTime.Now.ToLongTimeString();

					if (ResetAccountInfo == true)
					{
						if (File.Exists(path) == true) File.Delete(path);
						ResetAccountInfo = false;
					}
					if (File.Exists(path) == false)
						using (FileStream fs = File.Create(path))
	        					{
									AddText(fs,Prop_AutoLiquidationValue.ToString("0.00"));
								}

					if (File.Exists(path) == true)
						using (FileStream fs = File.OpenRead(path))
				        { 
				            byte[] b = new byte[1024];
				            UTF8Encoding temp = new UTF8Encoding(true);
				            while (fs.Read(b,0,b.Length) > 0)
				            {
				            	string accTreshold = temp.GetString(b);	
								readedTreshold = Convert.ToDouble(accTreshold);
								Prop_AutoLiquidationValue = readedTreshold;
								
				            }
					
						}
						high_mark = 	Prop_AutoLiquidationValue + Prop_DD;				
						dayStart = cashValue - realizedPnL;
						low_mark = Prop_AutoLiquidationValue;
						totalPnL = netLiq - dayStart;
				}
				if (selectedAccount != null)
				{
					realizedPnL = selectedAccount.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
					cashValue = selectedAccount.Get(AccountItem.CashValue, Currency.UsDollar);
					netLiq = selectedAccount.Get(AccountItem.NetLiquidation, Currency.UsDollar);
					unRealizedPnL = selectedAccount.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
					grossRealizedPnL =  selectedAccount.Get(AccountItem.GrossRealizedProfitLoss, Currency.UsDollar);
					totalDayPnL = realizedPnL + unRealizedPnL;
					
					path = NinjaTrader.Core.Globals.UserDataDir + selectedAccount.Name+".txt";
					lastUpdate = DateTime.Now.ToLongDateString() + " "+DateTime.Now.ToLongTimeString();
				
					if (ResetAccountInfo == true)
					{
						if (File.Exists(path) == true) File.Delete(path);
						ResetAccountInfo = false;
					}
					if (File.Exists(path) == false)
						using (FileStream fs = File.Create(path))
	        					{
									AddText(fs,Prop_AutoLiquidationValue.ToString("0.00"));
								}

					if (File.Exists(path) == true)
						using (FileStream fs = File.OpenRead(path))
				        { 
				            byte[] b = new byte[1024];
				            UTF8Encoding temp = new UTF8Encoding(true);
				            while (fs.Read(b,0,b.Length) > 0)
				            {
				            	string accTreshold = temp.GetString(b);	
								readedTreshold = Convert.ToDouble(accTreshold);
								Prop_AutoLiquidationValue = readedTreshold;
				            }
					
						}
							high_mark = 	Prop_AutoLiquidationValue + Prop_DD;				
							dayStart = cashValue - realizedPnL;
							low_mark = Prop_AutoLiquidationValue;
							totalPnL = netLiq - dayStart;

					  
					foreach (Position _pos in selectedAccount.Positions)
					  {
					      position = null;
						  if (_pos.Instrument == Instrument) position = _pos;
					  }	 
				}
				if (ChartControl != null && !UserControlCollection.Contains(buttonGrid))
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						CreatInsertWPFControls();
					}));
				}
			}
			else if (State == State.Historical)
			{
				 SetZOrder(50);

			}
			else if (State == State.Terminated)
			{
        		if (selectedAccount != null)
				{
            		selectedAccount.AccountItemUpdate -= OnAccountItemUpdate;
					selectedAccount.PositionUpdate 	-= OnPositionUpdate;
					selectedAccount.OrderUpdate 	-= OnOrderUpdate;
					selectedAccount.ExecutionUpdate 	-= OnExecutionUpdate;
				}
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						RemoveWPFControls();
					}));
				}
				
			}

		}
	
		public override string DisplayName
		{
		  get { return "Prop Trader Account Management Tool";}

		}

		private void CreatInsertWPFControls()
		{

			buttonGrid = new System.Windows.Controls.Grid
			{
				Name					= "MyCustomGrid",
				HorizontalAlignment		= HorizontalAlignment.Right,
				VerticalAlignment		= VerticalAlignment.Top,
				Margin					= new Thickness(0, 0, 0, 0)
			};

			buttonGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = GridLength.Auto });
		
			_tressholdButton = new System.Windows.Controls.Button
			{
				Name				= "_tressholdButton",
				Content				= Prop_AutoLiquidationValue.ToString("C2",culture),
				Background			= Brushes.Black,
				Foreground			= Brushes.White,
				Height				= 25,
				MinHeight			= 0,
				Width				= 120,
				MinWidth			= 0,
				Margin					= new Thickness(0, 0, 400, 55)
			};	
			_tressholdButton.FontSize = 12;
			_tressholdButton.ToolTip = "Account Liquidation Tresshold" ;
			_tressholdButton.FontFamily =  new FontFamily("Arial Bold");
			buttonGrid.Children.Add(_tressholdButton);	
		
			_autotradingButton = new System.Windows.Controls.Button
			{
				Name				= "_autotradingButton",
				Content				= " Net Liquidation ",
				Background			= Brushes.DarkGreen,
				Foreground			= Brushes.White,
				Height				= 25,
				MinHeight			= 0,
				Width				= 120,
				MinWidth			= 0,
				Margin					= new Thickness(0, 0, 160,55)
			};
			_autotradingButton.FontSize = 12;
			_autotradingButton.ToolTip = "  Net Liquidation  ";
			_autotradingButton.FontFamily =  new FontFamily("Arial Bold");
			buttonGrid.Children.Add(_autotradingButton);
		
			_accountButton = new System.Windows.Controls.Button
			{
				Name				= "_accountButton",
				Content				= selectedAccount != null ? "Acc. Selected" : " No Acc. Selected",
				Background			= selectedAccount != null ? Brushes.Green : Brushes.Black,
				Foreground			= Brushes.White,
				Height				= 25,
				MinHeight			= 0,
				Width				= 120,
				MinWidth			= 0,
				Margin					= new Thickness(0, 0, -80, 55)
			};
			_accountButton.FontSize = 13;
			_accountButton.ToolTip = "Selected Account: " + selectedAccount.Name;
			_accountButton.FontFamily =  new FontFamily("Georgia");
			buttonGrid.Children.Add(_accountButton);
		
			_trailingButton = new System.Windows.Controls.Button
			{
				Name				= "_trailingButton",
				Content				= "MaxDD: $" + Prop_DD.ToString("C2",culture),
				Background			= Brushes.Black,
				Foreground			= Brushes.White,
				Height				= 25,
				MinHeight			= 0,
				Width				= 120,
				MinWidth			= 0,
				Margin					= new Thickness(0, 0, 400,5)
			};
			_trailingButton.FontSize = 12;
			_trailingButton.ToolTip = "Trailing Drawdown";
			_trailingButton.FontFamily =  new FontFamily("Arial Bold");
			_trailingButton.Click += Button_Click;
			buttonGrid.Children.Add(_trailingButton);	
			
			_dayPnLButton = new System.Windows.Controls.Button
			{
				Name				= "_dayPnLButton",
				Content				= "PnL: $",
				Background			= Brushes.Black,
				Foreground			= Brushes.White,
				Height				= 25,
				MinHeight			= 0,
				Width				= 120,
				MinWidth			= 0,
				Margin					= new Thickness(0, 0, 160,5)
			};
			_dayPnLButton.FontSize = 12;
			_dayPnLButton.ToolTip = "Current Day PnL";
			_dayPnLButton.FontFamily =  new FontFamily("Arial Bold");
			buttonGrid.Children.Add(_dayPnLButton);	
			
			_exitButton = new System.Windows.Controls.Button
			{
				Name				= "_exitButton",
				Content				= "No Position",
				Background			= Brushes.Black,
				Foreground			= Brushes.White,
				Height				= 25,
				MinHeight			= 0,
				Width				= 120,
				MinWidth			= 0,
				Margin					= new Thickness(0, 0, -80, 5)
			};
			_exitButton.FontSize = 12;
			_exitButton.ToolTip = "Open PnL / EXIT (Flatten)";
			_exitButton.FontFamily =  new FontFamily("Arial Bold");
			_exitButton.Click += Button_Click;
			buttonGrid.Children.Add(_exitButton);

			_bracketButton = new System.Windows.Controls.Button
			{
				Name				= "_bracketButton",
				Content				= "[" + usePredSL +"/"+ usePredTP + "]",
				Background			= Brushes.Black,
				Foreground			= Brushes.White,
				Height				= 25,
				MinHeight			= 0,
				Width				= 60,
				MinWidth			= 0,
				Margin					= new Thickness(0, 0, -360, 5)
			};
			_bracketButton.FontSize = 10;
			_bracketButton.ToolTip = "Use Brackets SL/TP";
			_bracketButton.FontFamily =  new FontFamily("Arial Bold");
			_bracketButton.Click += Button_Click;
			buttonGrid.Children.Add(_bracketButton);
			
			_positionStopButton = new System.Windows.Controls.Button
			{
				Name				= "_positionStopButton",
				Content				= "DD Lines",
				Background			= Brushes.Black,
				Foreground			= Brushes.White,
				Height				= 25,
				MinHeight			= 0,
				Width				= 60,
				MinWidth			= 0,
				Margin					= new Thickness(0, 0, -250, 5)
			};
			_positionStopButton.FontSize = 10;
			_positionStopButton.ToolTip = "Your DD Limit Lines Up/Down";
			_positionStopButton.FontFamily =  new FontFamily("Arial Bold");
			_positionStopButton.Click += Button_Click;
			buttonGrid.Children.Add(_positionStopButton);
			
			_trButton = new System.Windows.Controls.Button
			{
				Name				= "_trButton",
				Content				= "["+_tr_start+"/"+_tr_offset+"]",
				Background			= Brushes.Black,
				Foreground			= Brushes.White,
				Height				= 25,
				MinHeight			= 0,
				Width				= 60,
				MinWidth			= 0,
				Margin					= new Thickness(0, 0, -360, 55)
			};
			_trButton.FontSize = 10;
			_trButton.ToolTip = "Start Trailing Stop";
			_trButton.FontFamily =  new FontFamily("Arial Bold");
			_trButton.Click += Button_Click;
			buttonGrid.Children.Add(_trButton);

			_beButton = new System.Windows.Controls.Button
			{
				Name				= "_beButton",
				Content				= "[BE " + _be_ticks_offset +"]",
				Background			= Brushes.Black,
				Foreground			= Brushes.White,
				Height				= 25,
				MinHeight			= 0,
				Width				= 60,
				MinWidth			= 0,
				Margin					= new Thickness(0, 0, -250, 55)
			};
			_beButton.FontSize = 10;
			_beButton.ToolTip = "Set SL at BE";
			_beButton.FontFamily =  new FontFamily("Arial Bold");
			_beButton.Click += Button_Click;
			buttonGrid.Children.Add(_beButton);
			
			
			UserControlCollection.Add(buttonGrid);
			
		}
		

		private void Button_Click(object sender, RoutedEventArgs e)
		{	
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			
			if (button == _exitButton )
			{
				if(selectedAccount != null) Dispatcher.InvokeAsync((() =>{selectedAccount.Flatten(new [] { Bars.Instrument });}));
				return;
			}

			if (button == _bracketButton )
			{
				if( _bracketButtonClicked == false)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
						{
							SetBracketsToOpenPosition();
							_bracketButtonClicked = true;
						}));	
					
					_bracketButtonClicked = true;
					ForceRefresh();
					return;
				}
				if( _bracketButtonClicked == true)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
						{
							SetBracketsToOpenPosition();
							//ChartControl.InvalidateVisual();
							_bracketButtonClicked = false;
						}));
					button.Background = Brushes.Black;
					_bracketButtonClicked = false;
					ForceRefresh();
					return;
				}					
				
			}
			
			if (button == _beButton )
			{
				if( _be_buttonClicked == false)
				{
					BE_Stop_Function = true;
					button.Background = Brushes.DarkGreen;
					_be_buttonClicked = true;
					ForceRefresh();
					return;
				}
				if( _be_buttonClicked == true)
				{
					BE_Stop_Function = false;
					button.Background = Brushes.Black;
					_be_buttonClicked = false;
					ForceRefresh();
					return;
				}
			}

			if (button == _trButton )
			{
				if( _tr_buttonClicked == false)
				{
					TR_Stop_Function = true;
					button.Background = Brushes.LimeGreen;
					_tr_buttonClicked = true;
					ForceRefresh();
					return;
				}
				if( _tr_buttonClicked == true)
				{
					TR_Stop_Function = false;
					button.Background = Brushes.Black;
					_tr_buttonClicked = false;
					ForceRefresh();
					return;
				}
			}
			
			if (button == _positionStopButton && position == null )
			{
				
				if( _chase_buttonClicked == false)
				{
					Chase_Function = true;
					button.Background = Brushes.Green;
					_chase_buttonClicked = true;
					ForceRefresh();
					return;
				}
				if( _chase_buttonClicked == true)
				{
					Chase_Function = false;
					button.Background = Brushes.Black;
					_chase_buttonClicked = false;
					ForceRefresh();
					return;
				}
			}
			
		}

		private void RemoveWPFControls()
		{
			if (_autotradingButton != null){_autotradingButton.Click -= Button_Click;_autotradingButton = null;}
			if (_tressholdButton!= null){_tressholdButton.Click -= Button_Click;_tressholdButton = null;}
			if (_accountButton != null){_accountButton.Click -= Button_Click;_accountButton = null;}
			if (_trailingButton != null){_trailingButton.Click -= Button_Click;_trailingButton = null;}
			if (_dayPnLButton != null){_dayPnLButton.Click -= Button_Click;_dayPnLButton = null;}
			if (_exitButton != null){_exitButton.Click -= Button_Click;_exitButton = null;}
			
			if (_bracketButton != null){_bracketButton.Click -= Button_Click;_bracketButton = null;}
			if (_positionStopButton != null){_positionStopButton.Click -= Button_Click;_positionStopButton = null;}
			if (_beButton != null){_beButton.Click -= Button_Click;_beButton = null;}
			if (_trButton != null){_trButton.Click -= Button_Click;_trButton = null;}

		}
				
		private void OnExecutionUpdate(object sender, ExecutionEventArgs e)
	    {
			if (e.Execution.Account == selectedAccount && e.Execution.Instrument == Instrument && e.Execution.IsLastExit == true )
			{		
				foreach ( Order ord in selectedAccount.Orders)
					if(ord.Instrument == Bars.Instrument ) if( selectedAccount!=null ) selectedAccount.Cancel(new [] { ord }); 
			}
			
	    }
		
		private void ResetAfterFlattering()
		{
						position = null;
						_pos = null;
						lastMarketPositionQuantity = 1;
						_trStop = null;
						_beStop = null;
						lastKnownDD = 0;
						ForceRefresh();
		}
		
		private void OnAccountItemUpdate(object sender, AccountItemEventArgs e)
		{
			if (e.AccountItem == AccountItem.RealizedProfitLoss)realizedPnL = e.Value;
			if (e.AccountItem == AccountItem.CashValue)cashValue = e.Value;
			if (e.AccountItem == AccountItem.NetLiquidation)netLiq = e.Value;
			if (e.AccountItem == AccountItem.UnrealizedProfitLoss)unRealizedPnL = e.Value;
			if (e.AccountItem == AccountItem.GrossRealizedProfitLoss)grossRealizedPnL = e.Value;
			totalPnL = netLiq - AccountStartValue;
			totalDayPnL = realizedPnL + unRealizedPnL;
		}
		
		private void OnOrderUpdate(object sender, OrderEventArgs e)
	    {
			if(bracketStop != null && bracketStop == e.Order )
			{
				if (e.OrderState == OrderState.Cancelled) { bracketStop = null; bracketProfit = null; } 
			}
					
				Dispatcher.InvokeAsync((() =>
					{
						UpdateVisual();
					}));
			
		}
		

		private void BRandTrailingFunction()
		{
				//==============   TRADING   ============
			if (position != null)	
			{
				if (position.MarketPosition == MarketPosition.Long)
				{
					if ( BE_Stop_Function == true  )
					{
						if (GetCurrentBid() >= position.AveragePrice + _be_ticks_offset*TickSize )
						{
							if (_beStop == null)
							{
								if(selectedAccount != null )
								{
									_beStop = selectedAccount.CreateOrder(Instrument, OrderAction.Sell, OrderType.StopMarket, OrderEntry.Manual, TimeInForce.Day, position.Quantity, 0 , position.AveragePrice+1*TickSize,"", "", DateTime.MaxValue, null);
									selectedAccount.Submit(new[] {_beStop});
								}
							}
						}
					}
					if ( TR_Stop_Function == true )
					{	
						if (GetCurrentBid() > position.AveragePrice + _tr_start*TickSize )
						{
							if (_trStop == null)
							{
								if(selectedAccount != null )
								{
									_trStop = selectedAccount.CreateOrder(Instrument, OrderAction.Sell, OrderType.StopMarket, OrderEntry.Manual, TimeInForce.Day, position.Quantity, 0 , GetCurrentBid()-_tr_offset*TickSize,"", "", DateTime.MaxValue, null);
									selectedAccount.Submit(new[] {_trStop});
								}
							}
							if (_trStop != null)
							{
								double curr_tr_stop = GetCurrentBid()-_tr_offset*TickSize;
								if (curr_tr_stop > _trStop.StopPrice ) 
								{
									_trStop.StopPriceChanged = GetCurrentBid()-_tr_offset*TickSize;
									selectedAccount.Change(new[] {_trStop});
								}
							}
						}
					}
				}
				//----
				if (position.MarketPosition == MarketPosition.Short)
				{
					if ( BE_Stop_Function  == true )
					{
						if (GetCurrentAsk() <= position.AveragePrice - _be_ticks_offset*TickSize )
						{
							if (_beStop == null)
							{
								if(selectedAccount != null )
								{
									_beStop = selectedAccount.CreateOrder(Instrument, OrderAction.Buy, OrderType.StopMarket, OrderEntry.Manual, TimeInForce.Day, position.Quantity, 0, position.AveragePrice-1*TickSize,"", "", DateTime.MaxValue, null);
									selectedAccount.Submit(new[] {_beStop});
								}
							}
						}
					}
					if ( TR_Stop_Function == true  )
					{
						if (GetCurrentAsk() < position.AveragePrice - _tr_start*TickSize )
						{
							if (_trStop == null)
							{
								_trStop = selectedAccount.CreateOrder(Instrument, OrderAction.Buy, OrderType.StopMarket, OrderEntry.Manual, TimeInForce.Day, position.Quantity, 0 , GetCurrentAsk()+_tr_offset*TickSize,"", "", DateTime.MaxValue, null);
								selectedAccount.Submit(new[] {_trStop});
							}
							if (_trStop != null)
							{
								double curr_tr_stop = GetCurrentAsk()+_tr_offset*TickSize;
								if (curr_tr_stop < _trStop.StopPrice ) 
								{
									if(selectedAccount != null )
								{
									_trStop.StopPriceChanged = GetCurrentAsk()+_tr_offset*TickSize;
									selectedAccount.Change(new[] {_trStop});
								}
									
							}
								
						}
							
					}
						
						
				}
			}
			
		}
		}

		private void SetBracketsToOpenPosition()
		{
			if ( position != null )
				{
					if (position.MarketPosition == MarketPosition.Long)
					{	
						string ocoString = Guid.NewGuid().ToString("N");
						double posStopPrice = GetCurrentBid() - usePredSL*TickSize;
						double posProfitPrice = GetCurrentAsk() + usePredTP*TickSize;
						
						if (selectedAccount != null)
						{
							if(bracketStop == null) bracketStop = selectedAccount.CreateOrder(Instrument, OrderAction.SellShort, OrderType.StopMarket, OrderEntry.Manual, TimeInForce.Day, position.Quantity, 0 ,posStopPrice,ocoString, "", DateTime.MaxValue, null);
							if(bracketProfit == null) bracketProfit = selectedAccount.CreateOrder(Instrument, OrderAction.Sell, OrderType.Limit, OrderEntry.Manual, TimeInForce.Day, position.Quantity,posProfitPrice ,0 ,ocoString, "", DateTime.MaxValue, null);
							selectedAccount.Submit(new[] {bracketStop,bracketProfit});
							
						}
					}
					//---
					if (position.MarketPosition == MarketPosition.Short)
					{
						string ocoString = Guid.NewGuid().ToString("N");
						double posStopPrice = GetCurrentAsk() + usePredSL*TickSize;
						double posProfitPrice = GetCurrentBid() - usePredTP*TickSize;
						
						if (selectedAccount != null)
						{
							if(bracketStop == null) bracketStop = selectedAccount.CreateOrder(Instrument, OrderAction.Buy, OrderType.StopMarket, OrderEntry.Manual, TimeInForce.Day, position.Quantity, 0 ,posStopPrice,ocoString, "", DateTime.MaxValue, null);
							if(bracketProfit == null) bracketProfit = selectedAccount.CreateOrder(Instrument, OrderAction.BuyToCover, OrderType.Limit, OrderEntry.Manual, TimeInForce.Day, position.Quantity,posProfitPrice ,0 ,ocoString, "", DateTime.MaxValue, null);
							selectedAccount.Submit(new[] {bracketStop,bracketProfit});
						}
					}
				}
		}

		private void UpdateVisual()
		{
			if ( position != null )
				{
					double posPrice = position.AveragePrice;
					double posQ = position.Quantity;
					double posStop = current_stopOrder.StopPrice;
					double posProfit = current_profitOrder.StopPrice;
					double posStopValue = posQ * Math.Abs(posPrice - posStop) * Instrument.MasterInstrument.PointValue;
					double posProfitValue = posQ * Math.Abs(posProfit - posPrice) * Instrument.MasterInstrument.PointValue;

					if (position.MarketPosition == MarketPosition.Long)
					{		
						if(posStopValue > stoplossExposureWarning*Math.Abs(ddMax)) 
						{
							Draw.TextFixed(this, "SL_exposure","\n"+"\n"+ "_____WARNING! SL Exposure > 50% of Max Drawdown !_____", TextPosition.TopRight, Brushes.Red, new Gui.Tools.SimpleFont("Arial", 16), Brushes.Red, Brushes.Pink, 20);
							ForceRefresh();
						}
						if(posStopValue < stoplossExposureWarning*Math.Abs(ddMax)) RemoveDrawObject("SL_exposure");
						ForceRefresh();
					}
					
					if (position.MarketPosition == MarketPosition.Short)
					{		
						if(posStopValue > stoplossExposureWarning*Math.Abs(ddMax)) 
						{
							Draw.TextFixed(this, "SL_exposure","\n"+"\n"+ "_____WARNING! SL Exposure > 50% of Max Drawdown !_____", TextPosition.TopRight, Brushes.Red, new Gui.Tools.SimpleFont("Arial", 16), Brushes.Red, Brushes.Pink, 20);
							ForceRefresh();
						}
						if(posStopValue < stoplossExposureWarning*Math.Abs(ddMax)) RemoveDrawObject("SL_exposure");
						ForceRefresh();
					}
				}
				
				if ( position == null )
				{
					if (position.MarketPosition == MarketPosition.Flat)
					{
						RemoveDrawObject("SL_exposure");
						lastKnownDD = 0;
						_lastLowMark = 0;
						ForceRefresh();
					}
				}
		}
		
		private void OnPositionUpdate(object sender, PositionEventArgs e)
		{
		
			if (e.Position.Account == selectedAccount && e.Position.Instrument == Instrument && e.MarketPosition == MarketPosition.Flat )
					{
						position = null;
						_pos = null;
						lastMarketPositionQuantity = 1;		
						_beStop = null;
						_trStop = null;
						if(bracketStop != null) Dispatcher.InvokeAsync((() =>{selectedAccount.Flatten(new [] { Bars.Instrument });}));
						bracketStop = null;
						bracketProfit = null;
						_lastLowMark = 0;
						lastKnownDD = 0;
						ForceRefresh();

					}
			if (e.Position.Account == selectedAccount && e.Position.Instrument == Instrument && e.MarketPosition != MarketPosition.Flat )
					{		
						position = e.Position;
					}
							
			if (position != null )
					{
						if (lastMarketPositionQuantity != position.Quantity)
						{	
							//----------------------------------------------------------------------------
							Dispatcher.InvokeAsync((() =>
								{
									lastMarketPositionQuantity = position.Quantity;	
									bracketProfit.QuantityChanged = lastMarketPositionQuantity; selectedAccount.Change(new[] {bracketProfit} );
								}));
							
							Dispatcher.InvokeAsync((() =>
								{
									lastMarketPositionQuantity = position.Quantity;	
									bracketStop.QuantityChanged = lastMarketPositionQuantity; selectedAccount.Change(new[] {bracketStop} );
								}));
						}
					}		
		}

		private double lastProjectedLong;
		private double lastProjectedShort;
		private int lastTradeBar;
		private bool Auto_tradingFirst_Run = true;
		
		protected void CheckAccMatch()
		{
			ChartControl.Dispatcher.InvokeAsync((Action)(() =>
			{
				accountSelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector);
				accCheck = accountSelector.SelectedAccount;
					
				if (selectedAccount != null)
					{
						if (selectedAccount.Name != accCheck.Name)
						{
							Draw.TextFixed(this, "accountss","\n"+ "Selected Account does not match ChartTrader Account" , TextPosition.TopRight, Brushes.Red, new Gui.Tools.SimpleFont("Arial", 16), Brushes.Transparent, Brushes.Transparent, 0);
							ForceRefresh();
						}
						if (selectedAccount.Name == accCheck.Name)
						{
							RemoveDrawObject("accountss");
							ForceRefresh();
						}
					}
					_autotradingButton.Content = netLiq.ToString("C2",culture);
					
					if (netLiq < AccountStartValue)
					{
						_autotradingButton.Background = Brushes.DarkRed;
					}
					else
						_autotradingButton.Background = Brushes.DarkGreen;
					
			
			}));
		}
		
		protected override void OnBarUpdate()
		{
			if ( AccountStartValue == 1 ){
					Draw.TextFixed(this, "info", "> Please specify account initial balance < ) " +  "\n"+ "\n", TextPosition.BottomRight,ChartControl.Properties.ChartText,ChartControl.Properties.LabelFont,Brushes.Red, Brushes.Pink, 50);
					return;
				}	
			if ( Prop_DD == 1 ){
					Draw.TextFixed(this, "info1", "> Please specify account max Drawdown value < ) " +  "\n"+ "\n", TextPosition.BottomRight,ChartControl.Properties.ChartText,ChartControl.Properties.LabelFont,Brushes.Red, Brushes.Pink, 50);	
					return;
				}

			if ( State != State.Realtime) return;
			CheckAccMatch();

			if (position != null)	
			{
				fastInfo_unRealizedPnL = position.GetUnrealizedProfitLoss(PerformanceUnit.Currency);
				tOtal = 	netLiq  - AccountStartValue  ;
				if  ( (netLiq - Prop_DD) > Prop_AutoLiquidationValue )
				{
					low_mark = netLiq - Prop_DD;
						if (File.Exists(path) == true)
								using (FileStream fs = File.OpenWrite(path))
								{
									Prop_AutoLiquidationValue = low_mark;
									AddText(fs,Prop_AutoLiquidationValue.ToString("0.00"));
								}
				}
				totalDayPnL = realizedPnL + unRealizedPnL;		
				Dispatcher.InvokeAsync((() =>{
					_dayPnLButton.Content = totalDayPnL.ToString("C2",culture);
					_dayPnLButton.Background = totalDayPnL > 0 ? Brushes.Green : Brushes.Red;
				
				}));
				
				Dispatcher.InvokeAsync((() =>{_exitButton.Content = fastInfo_unRealizedPnL.ToString("C2",culture);}));
				Dispatcher.InvokeAsync((() =>{_exitButton.Background = fastInfo_unRealizedPnL > 0 ? Brushes.Green : Brushes.Red;}));			
				high_mark = Prop_AutoLiquidationValue + Prop_DD;
				ddMax = netLiq - low_mark;
				if (position != null) Dispatcher.InvokeAsync((() =>
				{ 
					_trailingButton.Content = "MaxDD:  " + ddMax.ToString("C2",culture);	
					if (ddMax >= Prop_DD) 		_trailingButton.Background = Brushes.Green;
					if (ddMax < 0.5*Prop_DD) 	_trailingButton.Background = Brushes.Magenta;
					if (ddMax < 0) 				_trailingButton.Background = Brushes.Red;
				}));						
			}														

			if (position == null)	
			{
				Dispatcher.InvokeAsync((() =>{
					_exitButton.Content = " No Open Position";
					_exitButton.Background = Brushes.DarkGoldenrod;
				}));	
				
				Dispatcher.InvokeAsync((() =>{
					ddMax = netLiq - low_mark;
					_trailingButton.Content = "MaxDD:  " + ddMax.ToString("C2",culture);
				}));
				Dispatcher.InvokeAsync((() =>{
					totalDayPnL = realizedPnL + unRealizedPnL;
					_dayPnLButton.Content = totalDayPnL.ToString("C2",culture);
					_dayPnLButton.Background = totalDayPnL > 0 ? Brushes.Green : Brushes.Red;

				}));
				
			}
				
			
			
			
    	
			
		} // end OnBarUpdate
			
			
		
		private void TrailingDDFunction()
		{
			double line_1 = Line_1/100;
			double line_2 = Line_2/100;
			double line_3 = Line_3/100;
			if (Show_Trailing_DD_ChartLines )
			{
			if (position != null  )	
			{
					RemoveDrawObject("longChartTRDD1");RemoveDrawObject("longChartTRDD1_text");RemoveDrawObject("longChartTRDD2");RemoveDrawObject("longChartTRDD2_text");RemoveDrawObject("longChartTRDD3");RemoveDrawObject("longChartTRDD3_text");
					RemoveDrawObject("shortChartTRDD1");RemoveDrawObject("shortChartTRDD1_text");	RemoveDrawObject("shortChartTRDD2");RemoveDrawObject("shortChartTRDD2_text");RemoveDrawObject("shortChartTRDD3");RemoveDrawObject("shortChartTRDD3_text");
					Chase_Function = false;
					_chase_buttonClicked = false;
				
				if (lastKnownDD == 0){_lastLowMark = Prop_AutoLiquidationValue;lastKnownDD = netLiq - _lastLowMark;}	
				if (low_mark > _lastLowMark){lastKnownDD = netLiq - Prop_AutoLiquidationValue;_lastLowMark = Prop_AutoLiquidationValue;}
				int textOffset = 0;
				
				if (position.MarketPosition == MarketPosition.Long)
				{
					double open_Position_price = position.AveragePrice;
					if( GetCurrentBid() > open_Position_price) open_Position_price = GetCurrentBid();textOffset = -10;
					curr_TRD_price_1 = Instrument.MasterInstrument.RoundToTickSize(open_Position_price - line_1*lastKnownDD/position.Quantity/Instrument.MasterInstrument.PointValue);
					if(curr_TRD_price_1 > TRD_price_1 || TRD_price_1 == 0){TRD_price_1 = curr_TRD_price_1;}
					curr_TRD_price_2 = Instrument.MasterInstrument.RoundToTickSize(open_Position_price - line_2*lastKnownDD/position.Quantity/Instrument.MasterInstrument.PointValue);
					if(curr_TRD_price_2 > TRD_price_2 || TRD_price_2 == 0){TRD_price_2 = curr_TRD_price_2;}
					curr_TRD_price_3 = Instrument.MasterInstrument.RoundToTickSize(open_Position_price - line_3*lastKnownDD/position.Quantity/Instrument.MasterInstrument.PointValue);
					if(curr_TRD_price_3 > TRD_price_3 || TRD_price_3 == 0){TRD_price_3 = curr_TRD_price_3;}
				}
				if (position.MarketPosition == MarketPosition.Short)
				{
					double open_Position_price = position.AveragePrice;
					if( GetCurrentAsk() < open_Position_price) open_Position_price = GetCurrentAsk();textOffset = 10;
					curr_TRD_price_1 = Instrument.MasterInstrument.RoundToTickSize( open_Position_price + line_1*lastKnownDD/position.Quantity/Instrument.MasterInstrument.PointValue);
					if(curr_TRD_price_1 < TRD_price_1 || TRD_price_1 == 0){TRD_price_1 = curr_TRD_price_1;}
					curr_TRD_price_2 = Instrument.MasterInstrument.RoundToTickSize(open_Position_price + line_2*lastKnownDD/position.Quantity/Instrument.MasterInstrument.PointValue);
					if(curr_TRD_price_2 < TRD_price_2 || TRD_price_2 == 0){TRD_price_2 = curr_TRD_price_2;}
					curr_TRD_price_3 = Instrument.MasterInstrument.RoundToTickSize(open_Position_price + line_3*lastKnownDD/position.Quantity/Instrument.MasterInstrument.PointValue);
					if(curr_TRD_price_3 < TRD_price_3 || TRD_price_3== 0){TRD_price_3 = curr_TRD_price_3;}
				}
				if ( TRD_price_1 > 0 ) {
					Draw.Ray(this,"ChartTRDD1",false,-1, TRD_price_1,-3, TRD_price_1, Brushes.Yellow,DashStyleHelper.Dash, 1);
					Draw.Text(this, "ChartTRDD1_text", false, Line_1.ToString() + "% Draw Down ($" + (line_1*lastKnownDD).ToString("0") + ")", -1, TRD_price_1, textOffset, Brushes.Yellow, ddFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
				}
				if ( TRD_price_2 > 0 ){
					Draw.Ray(this,"ChartTRDD2",false,-1, TRD_price_2,-3, TRD_price_2, Brushes.Pink, DashStyleHelper.Dash, 2);
					Draw.Text(this, "ChartTRDD2_text", false, Line_2.ToString()+"% Draw Down ($" + (line_2*lastKnownDD).ToString("0") + ")", -1, TRD_price_2, textOffset, Brushes.Pink, ddFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
				}
				if ( TRD_price_3 > 0 ){
					Draw.Ray(this,"ChartTRDD3",false,-1, TRD_price_3,-3, TRD_price_3, Brushes.Red, DashStyleHelper.Dash, 3);
					Draw.Text(this, "ChartTRDD3_text", false, Line_3.ToString() + "% Draw Down ($" + (line_3*lastKnownDD).ToString("0") + ")", -1, TRD_price_3, textOffset, Brushes.Red, ddFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);		
				}
				ForceRefresh();
			}
			}
		if (!Show_Trailing_DD_ChartLines )
		{
			RemoveDrawObject("ChartTRDD1");RemoveDrawObject("ChartTRDD1_text");RemoveDrawObject("ChartTRDD2");RemoveDrawObject("ChartTRDD2_text");RemoveDrawObject("ChartTRDD3");RemoveDrawObject("ChartTRDD3_text");
			ForceRefresh();
		}

		if (position == null)	
			{
				RemoveDrawObject("SL_exposure");RemoveDrawObject("ChartTRDD1");RemoveDrawObject("ChartTRDD1_text");RemoveDrawObject("ChartTRDD2");RemoveDrawObject("ChartTRDD2_text");RemoveDrawObject("ChartTRDD3");RemoveDrawObject("ChartTRDD3_text");
				TRD_price_1 = 0;TRD_price_2 = 0;TRD_price_3 = 0;
				if(Chase_Function && Show_Trailing_DD_flat )
				{
				ChartControl.Dispatcher.InvokeAsync((Action)(() =>
				{
					quantitySelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlQuantitySelector") as NinjaTrader.Gui.Tools.QuantityUpDown);
					qSelected = quantitySelector.Value;
				}));
					
				double currentClose = GetCurrentBid();
				lastKnownDD = netLiq - Prop_AutoLiquidationValue; 

				double long_TRD_price_1 = Instrument.MasterInstrument.RoundToTickSize(currentClose - line_1*lastKnownDD/qSelected/Instrument.MasterInstrument.PointValue);
				double long_TRD_price_2 = Instrument.MasterInstrument.RoundToTickSize(currentClose - line_2*lastKnownDD/qSelected/Instrument.MasterInstrument.PointValue);
				double long_TRD_price_3 = Instrument.MasterInstrument.RoundToTickSize(currentClose - line_3*lastKnownDD/qSelected/Instrument.MasterInstrument.PointValue);
					
				double short_TRD_price_1 = Instrument.MasterInstrument.RoundToTickSize(currentClose + line_1*lastKnownDD/qSelected/Instrument.MasterInstrument.PointValue);
				double short_TRD_price_2 = Instrument.MasterInstrument.RoundToTickSize(currentClose + line_2*lastKnownDD/qSelected/Instrument.MasterInstrument.PointValue);
				double short_TRD_price_3 = Instrument.MasterInstrument.RoundToTickSize(currentClose + line_3*lastKnownDD/qSelected/Instrument.MasterInstrument.PointValue);
						
					if ( long_TRD_price_1 > 0 ){
						Draw.Ray(this,"longChartTRDD1",false,-1,long_TRD_price_1,-3,long_TRD_price_1,Brushes.Yellow,DashStyleHelper.Dash, 1);
						Draw.Text(this, "longChartTRDD1_text", false, Line_1.ToString() + "% Draw Down ($" + (line_1*lastKnownDD).ToString("0") + ")", -1, long_TRD_price_1, -10, Brushes.Yellow, ddFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
					}
					if ( long_TRD_price_2 > 0 ){
						Draw.Ray(this, "longChartTRDD2", false,-1,long_TRD_price_2,-3,long_TRD_price_2, Brushes.Pink, DashStyleHelper.Dash, 2);
						Draw.Text(this, "longChartTRDD2_text", false, Line_2.ToString() + "% Draw Down ($" + (line_2*lastKnownDD).ToString("0") + ")", -1, long_TRD_price_2, -10, Brushes.Pink, ddFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
					}
					if ( long_TRD_price_3 > 0 ){
						Draw.Ray(this, "longChartTRDD3", false, -1,long_TRD_price_3,-3,long_TRD_price_3, Brushes.Red, DashStyleHelper.Dash, 3);
						Draw.Text(this, "longChartTRDD3_text", false, Line_3.ToString() + "% Draw Down ($" + (line_3*lastKnownDD).ToString("0") + ")", -1, long_TRD_price_3, -10, Brushes.Red, ddFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);		
					}
	
					if ( short_TRD_price_1 > 0 ){
						Draw.Ray(this, "shortChartTRDD1", false, -1, short_TRD_price_1,-3, short_TRD_price_1, Brushes.Yellow, DashStyleHelper.Dash, 1);
						Draw.Text(this, "shortChartTRDD1_text", false, Line_1.ToString() + "% Draw Down ($" + (line_1*lastKnownDD).ToString("0") + ")", -1, short_TRD_price_1, 10, Brushes.Yellow, ddFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
					}
					if ( short_TRD_price_2 > 0 ){
						Draw.Ray(this, "shortChartTRDD2", false,  -1, short_TRD_price_2,-3, short_TRD_price_2, Brushes.Pink, DashStyleHelper.Dash, 2);
						Draw.Text(this, "shortChartTRDD2_text", false, Line_2.ToString() + "% Draw Down ($" + (line_2*lastKnownDD).ToString("0") + ")", -1, short_TRD_price_2, 10, Brushes.Pink, ddFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
					}
					if ( short_TRD_price_3 > 0 ){
						Draw.Ray(this, "shortChartTRDD3", false,  -1, short_TRD_price_3,-3, short_TRD_price_3, Brushes.Red, DashStyleHelper.Dash, 3);
						Draw.Text(this, "shortChartTRDD3_text", false, Line_3.ToString() + "% Draw Down ($" + (line_3*lastKnownDD).ToString("0") + ")", -1, short_TRD_price_3, 10, Brushes.Red, ddFont, TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);	
					}
					ForceRefresh();
				}
				
				if(!Chase_Function)
				{
					RemoveDrawObject("longChartTRDD1");RemoveDrawObject("longChartTRDD1_text");RemoveDrawObject("longChartTRDD2");RemoveDrawObject("longChartTRDD2_text");RemoveDrawObject("longChartTRDD3");RemoveDrawObject("longChartTRDD3_text");
					RemoveDrawObject("shortChartTRDD1");RemoveDrawObject("shortChartTRDD1_text");RemoveDrawObject("shortChartTRDD2");RemoveDrawObject("shortChartTRDD2_text");RemoveDrawObject("shortChartTRDD3");RemoveDrawObject("shortChartTRDD3_text");
					ForceRefresh();
				}
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
			base.OnRender(chartControl, chartScale);
			TrailingDDFunction();
			BRandTrailingFunction();
        }
		
		
		#region Properties
		
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Support/HowToUse/SetUp -> :",  		       					Order=0, GroupName="0. Support")]
		public string ThisTool
		{ get; set; }
		
		
		[Display(Name="SELECT TRADE ACCOUNT", 									Order=1, GroupName="1. Trading Accounts ")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string selectedAccountName { get; set; }


		[NinjaScriptProperty]
		[Display(Name="1 Prop Account initial balance", 						Order=2, GroupName="1. Trading Accounts ")]
		public double AccountStartValue
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="2 Prop Account Drawdown", 								Order=3, GroupName="1. Trading Accounts ")]
		public double Prop_DD
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="3 Prop Account Auto Liduidation Value", 					Order=4, GroupName="1. Trading Accounts ")]
		public double Prop_AutoLiquidationValue
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="4 Set/Reset Account Info", 	Description="Do it Once when the tool is used for the first time for specific account. Then the checkbox will automatically uncheck itself. If the account is not set, you will get a warning.",								Order=5, GroupName="1. Trading Accounts ")]
		public bool ResetAccountInfo
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="5 StopLoss Exposure Warning (% of MaxDD)", 				Order=6, GroupName="1. Trading Accounts ")]
		public double stoplossExposureWarning
		{ get; set; }
		
		
		
		
		
		[NinjaScriptProperty]
		[Display(Name="1. Bracket Stop Loss in ticks", 				   Order=1, GroupName="2. Function settings")]
		public int usePredSL
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="2. Bracket Take Profit in ticks", 			   Order=2, GroupName="2. Function settings")]
		public int usePredTP
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="3. Move StopLoss at BE", 					   Order=3, GroupName="2. Function settings")]
		public bool _set_BE
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="4. BE after #ticks ", 			               Order=4, GroupName="2. Function settings")]
		public int _be_ticks_offset
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="5. Set Trailing Stop", 						   Order=5, GroupName="2. Function settings")]
		public bool _set_Trailing_Stop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="6. Trailing Stop start after #ticks",			Order=6, GroupName="2. Function settings")]
		public int _tr_start
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="7. Trailing Stop offset", 			            Order=7, GroupName="2. Function settings")]
		public int _tr_offset
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="8. Show Trailing DD Lines", 						Order=8, GroupName="2. Function settings")]
		public bool Show_Trailing_DD_ChartLines
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="9. Show Trailing DD when FLAT", 					Order=9, GroupName="2. Function settings")]
		public bool Show_Trailing_DD_flat
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="10. Line 1 level DD%", 							Order=10, GroupName="2. Function settings")]
		public double Line_1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="11. Line 2 level DD%", 							Order=11, GroupName="2. Function settings")]
		public double Line_2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="12. Line 3 level DD%", 							Order=12, GroupName="2. Function settings")]
		public double Line_3
		{ get; set; }
		
		
		
	
		

        #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Prop_Trader_Tools.PropTraderAccountTool[] cachePropTraderAccountTool;
		public Prop_Trader_Tools.PropTraderAccountTool PropTraderAccountTool(string thisTool, double accountStartValue, double prop_DD, double prop_AutoLiquidationValue, bool resetAccountInfo, double stoplossExposureWarning, int usePredSL, int usePredTP, bool _set_BE, int _be_ticks_offset, bool _set_Trailing_Stop, int _tr_start, int _tr_offset, bool show_Trailing_DD_ChartLines, bool show_Trailing_DD_flat, double line_1, double line_2, double line_3)
		{
			return PropTraderAccountTool(Input, thisTool, accountStartValue, prop_DD, prop_AutoLiquidationValue, resetAccountInfo, stoplossExposureWarning, usePredSL, usePredTP, _set_BE, _be_ticks_offset, _set_Trailing_Stop, _tr_start, _tr_offset, show_Trailing_DD_ChartLines, show_Trailing_DD_flat, line_1, line_2, line_3);
		}

		public Prop_Trader_Tools.PropTraderAccountTool PropTraderAccountTool(ISeries<double> input, string thisTool, double accountStartValue, double prop_DD, double prop_AutoLiquidationValue, bool resetAccountInfo, double stoplossExposureWarning, int usePredSL, int usePredTP, bool _set_BE, int _be_ticks_offset, bool _set_Trailing_Stop, int _tr_start, int _tr_offset, bool show_Trailing_DD_ChartLines, bool show_Trailing_DD_flat, double line_1, double line_2, double line_3)
		{
			if (cachePropTraderAccountTool != null)
				for (int idx = 0; idx < cachePropTraderAccountTool.Length; idx++)
					if (cachePropTraderAccountTool[idx] != null && cachePropTraderAccountTool[idx].ThisTool == thisTool && cachePropTraderAccountTool[idx].AccountStartValue == accountStartValue && cachePropTraderAccountTool[idx].Prop_DD == prop_DD && cachePropTraderAccountTool[idx].Prop_AutoLiquidationValue == prop_AutoLiquidationValue && cachePropTraderAccountTool[idx].ResetAccountInfo == resetAccountInfo && cachePropTraderAccountTool[idx].stoplossExposureWarning == stoplossExposureWarning && cachePropTraderAccountTool[idx].usePredSL == usePredSL && cachePropTraderAccountTool[idx].usePredTP == usePredTP && cachePropTraderAccountTool[idx]._set_BE == _set_BE && cachePropTraderAccountTool[idx]._be_ticks_offset == _be_ticks_offset && cachePropTraderAccountTool[idx]._set_Trailing_Stop == _set_Trailing_Stop && cachePropTraderAccountTool[idx]._tr_start == _tr_start && cachePropTraderAccountTool[idx]._tr_offset == _tr_offset && cachePropTraderAccountTool[idx].Show_Trailing_DD_ChartLines == show_Trailing_DD_ChartLines && cachePropTraderAccountTool[idx].Show_Trailing_DD_flat == show_Trailing_DD_flat && cachePropTraderAccountTool[idx].Line_1 == line_1 && cachePropTraderAccountTool[idx].Line_2 == line_2 && cachePropTraderAccountTool[idx].Line_3 == line_3 && cachePropTraderAccountTool[idx].EqualsInput(input))
						return cachePropTraderAccountTool[idx];
			return CacheIndicator<Prop_Trader_Tools.PropTraderAccountTool>(new Prop_Trader_Tools.PropTraderAccountTool(){ ThisTool = thisTool, AccountStartValue = accountStartValue, Prop_DD = prop_DD, Prop_AutoLiquidationValue = prop_AutoLiquidationValue, ResetAccountInfo = resetAccountInfo, stoplossExposureWarning = stoplossExposureWarning, usePredSL = usePredSL, usePredTP = usePredTP, _set_BE = _set_BE, _be_ticks_offset = _be_ticks_offset, _set_Trailing_Stop = _set_Trailing_Stop, _tr_start = _tr_start, _tr_offset = _tr_offset, Show_Trailing_DD_ChartLines = show_Trailing_DD_ChartLines, Show_Trailing_DD_flat = show_Trailing_DD_flat, Line_1 = line_1, Line_2 = line_2, Line_3 = line_3 }, input, ref cachePropTraderAccountTool);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Prop_Trader_Tools.PropTraderAccountTool PropTraderAccountTool(string thisTool, double accountStartValue, double prop_DD, double prop_AutoLiquidationValue, bool resetAccountInfo, double stoplossExposureWarning, int usePredSL, int usePredTP, bool _set_BE, int _be_ticks_offset, bool _set_Trailing_Stop, int _tr_start, int _tr_offset, bool show_Trailing_DD_ChartLines, bool show_Trailing_DD_flat, double line_1, double line_2, double line_3)
		{
			return indicator.PropTraderAccountTool(Input, thisTool, accountStartValue, prop_DD, prop_AutoLiquidationValue, resetAccountInfo, stoplossExposureWarning, usePredSL, usePredTP, _set_BE, _be_ticks_offset, _set_Trailing_Stop, _tr_start, _tr_offset, show_Trailing_DD_ChartLines, show_Trailing_DD_flat, line_1, line_2, line_3);
		}

		public Indicators.Prop_Trader_Tools.PropTraderAccountTool PropTraderAccountTool(ISeries<double> input , string thisTool, double accountStartValue, double prop_DD, double prop_AutoLiquidationValue, bool resetAccountInfo, double stoplossExposureWarning, int usePredSL, int usePredTP, bool _set_BE, int _be_ticks_offset, bool _set_Trailing_Stop, int _tr_start, int _tr_offset, bool show_Trailing_DD_ChartLines, bool show_Trailing_DD_flat, double line_1, double line_2, double line_3)
		{
			return indicator.PropTraderAccountTool(input, thisTool, accountStartValue, prop_DD, prop_AutoLiquidationValue, resetAccountInfo, stoplossExposureWarning, usePredSL, usePredTP, _set_BE, _be_ticks_offset, _set_Trailing_Stop, _tr_start, _tr_offset, show_Trailing_DD_ChartLines, show_Trailing_DD_flat, line_1, line_2, line_3);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Prop_Trader_Tools.PropTraderAccountTool PropTraderAccountTool(string thisTool, double accountStartValue, double prop_DD, double prop_AutoLiquidationValue, bool resetAccountInfo, double stoplossExposureWarning, int usePredSL, int usePredTP, bool _set_BE, int _be_ticks_offset, bool _set_Trailing_Stop, int _tr_start, int _tr_offset, bool show_Trailing_DD_ChartLines, bool show_Trailing_DD_flat, double line_1, double line_2, double line_3)
		{
			return indicator.PropTraderAccountTool(Input, thisTool, accountStartValue, prop_DD, prop_AutoLiquidationValue, resetAccountInfo, stoplossExposureWarning, usePredSL, usePredTP, _set_BE, _be_ticks_offset, _set_Trailing_Stop, _tr_start, _tr_offset, show_Trailing_DD_ChartLines, show_Trailing_DD_flat, line_1, line_2, line_3);
		}

		public Indicators.Prop_Trader_Tools.PropTraderAccountTool PropTraderAccountTool(ISeries<double> input , string thisTool, double accountStartValue, double prop_DD, double prop_AutoLiquidationValue, bool resetAccountInfo, double stoplossExposureWarning, int usePredSL, int usePredTP, bool _set_BE, int _be_ticks_offset, bool _set_Trailing_Stop, int _tr_start, int _tr_offset, bool show_Trailing_DD_ChartLines, bool show_Trailing_DD_flat, double line_1, double line_2, double line_3)
		{
			return indicator.PropTraderAccountTool(input, thisTool, accountStartValue, prop_DD, prop_AutoLiquidationValue, resetAccountInfo, stoplossExposureWarning, usePredSL, usePredTP, _set_BE, _be_ticks_offset, _set_Trailing_Stop, _tr_start, _tr_offset, show_Trailing_DD_ChartLines, show_Trailing_DD_flat, line_1, line_2, line_3);
		}
	}
}

#endregion
