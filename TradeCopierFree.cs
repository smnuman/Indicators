#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
//using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{

	[Gui.CategoryOrder("Misc. Parameters",		1)]
	[Gui.CategoryOrder("Master Position", 		2)]
	[Gui.CategoryOrder("Follower Positions",	3)]
	[Gui.CategoryOrder("Debug", 				100)]
	
	public class TradeCopierFree : Indicator
	{

		#region Vars
		private bool DEBUG = true;
		
		private const string	indiName				= "Trade Copier (free)";
		private const string	indiNameFull			= indiName+"v1";
		private const string	buttonContentCopyOn		= "Copier: ON";
		private const string	buttonContentCopyOff	= "Copier: Off";
		private const string	orderPreName			= "TCF-";
		private string			warningSoundFile 		= NinjaTrader.Core.Globals.InstallDir+@"sounds\Alert1.wav";
		
		private bool IsCopyAllowed	= false,		isRealtime		= false,		isTimerEventCreated	= false;
		private bool IsMasterSubscribed = false,	IsAcct1Subscribed = false,		IsAcct2Subscribed	= false,	IsAcct3Subscribed = false,		IsAcct4Subscribed = false,		IsAcct5Subscribed = false;
		private bool IsAcct1Trading = false,		IsAcct2Trading	= false,		IsAcct3Trading = false,			IsAcct4Trading = false,			IsAcct5Trading = false;
		
		private NinjaTrader.Cbi.Instrument	MasterInstr;//,	Acct1Instrument,	Acct2Instrument,	Acct3Instrument,	Acct4Instrument,	Acct5Instrument;
		private Account 					MasterAccount,	Acct1,	Acct2,	Acct3,	Acct4;
//		private Account Acct5,	Acct6,	Acct7,	Acct8,	Acct9;
		private MarketPosition 				masterAcctMarketPosition,	Acct1MarketPosition,	Acct2MarketPosition,	Acct3MarketPosition,	Acct4MarketPosition;
//		Execution masterExecution = null;
		
//		OrderAction orderAction;
//		OrderType orderType = OrderType.Market;
//		private int ordQuantity;
		
		
		private bool copyButtonClicked;
		private System.Windows.Controls.Button copyButton;
		private System.Windows.Controls.Grid TCFGrid;
		private Timer acctFlatTimer = new Timer(1500);
		private Chart chartWindow;
		#endregion


		protected override void OnStateChange()
		{
//			if(DEBUG) Print(orderPreName+"  State."+State);
			if (State == State.SetDefaults)
			{
				#region SetDefaults
				
				Description									= @"Copies orders from the master account & this chart's instrument to other accounts and/or instruments.";
				Name										= indiNameFull;
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				
				ButtonVertPosition	= TCFVerticalPosition.Top;
				ButtonShift 		= 35;
				CheckAllAcctsFlat	= true;
				Acct1InstrName = "MES 03-24";
				Acct2InstrName = "MES 03-24";
				Acct3InstrName = "MES 03-24";
				Acct4InstrName = "MES 03-24";
//				Acct5InstrName = "MES 03-24";
//				Acct6InstrName = "MES 03-24";
//				Acct7InstrName = "MES 03-24";
//				Acct8InstrName = "MES 03-24";
//				Acct9InstrName = "MES 03-24";
				Ratio1 = 1;
				Ratio2 = 1;
				Ratio3 = 1;
				Ratio4 = 1;
//				Ratio5 = 1.0;
//				Ratio6 = 1.0;
//				Ratio7 = 1.0;
//				Ratio8 = 1.0;
//				Ratio9 = 1.0;
//				RatioFormula	= TCFRoundingFormula.Normal_Rounding;
				#endregion
			}
			else if (State == State.Configure)
			{
				Calculate		= Calculate.OnBarClose;
				IsCopyAllowed 	= false;
			}
			else if(State == State.DataLoaded)
			{
				#region DataLoaded
				
				#region Accounts
				// Account select
				lock (Account.All)
				{
					if (MasterAccountName != null)
					{
						MasterAccount = Account.All.FirstOrDefault(a => a.Name == MasterAccountName);
						if (Acct1Name != null  ) 		Acct1 = Account.All.FirstOrDefault(a => a.Name == Acct1Name);
						if (Acct2Name != null  ) 		Acct2 = Account.All.FirstOrDefault(a => a.Name == Acct2Name);
						if (Acct3Name != null  ) 		Acct3 = Account.All.FirstOrDefault(a => a.Name == Acct3Name);
						if (Acct4Name != null  ) 		Acct4 = Account.All.FirstOrDefault(a => a.Name == Acct4Name);
//						if (Acct5Name != null  ) 		Acct5 = Account.All.FirstOrDefault(a => a.Name == Acct5Name);
					}
//						if(DEBUG) Print(orderPreName+" "+State+":  lock (Account.All) - END");
				}
				if(DEBUG) Print(orderPreName+" "+State+":  Acct1: "+(Acct1!=null?Acct1.Name.PadRight(23):"null \t\t")+" Acct2: "+(Acct2!=null?Acct2.Name.PadRight(24):"null \t\t")+" Acct3: "+(Acct3!=null?Acct3.Name.PadRight(24):"null \t\t")+" Acct4: "+(Acct4!=null?Acct4.Name:"null \t\t"));
			
				if (MasterAccount != null)
				{
					MasterAccount.OrderUpdate	+= OnOrderUpdate;	MasterAccount.PositionUpdate	+= OnPositionUpdate;	MasterAccount.ExecutionUpdate 	+= OnExecutionUpdate;	IsMasterSubscribed = true;
					
					if (Acct1 != null && Acct1 != MasterAccount) 	
						{ Acct1.OrderUpdate 	+= OnOrderUpdate;	Acct1.PositionUpdate 	+= OnPositionUpdate;	Acct1.ExecutionUpdate 	+= OnExecutionUpdate;	IsAcct1Subscribed = true; }
					if (Acct2 != null && Acct2 != MasterAccount && Acct2 != Acct1) 	
						{ Acct2.OrderUpdate 	+= OnOrderUpdate; 	Acct2.PositionUpdate 	+= OnPositionUpdate;	Acct2.ExecutionUpdate 	+= OnExecutionUpdate;	IsAcct2Subscribed = true; }
					if (Acct3 != null && Acct3 != MasterAccount && Acct3 != Acct1 && Acct3 != Acct2) 	
						{ Acct3.OrderUpdate 	+= OnOrderUpdate; 	Acct3.PositionUpdate 	+= OnPositionUpdate;	Acct3.ExecutionUpdate 	+= OnExecutionUpdate;	IsAcct3Subscribed = true; }
					if (Acct4 != null && Acct4 != MasterAccount && Acct4 != Acct1 && Acct4 != Acct2 && Acct4 != Acct3) 	
						{ Acct4.OrderUpdate 	+= OnOrderUpdate; 	Acct4.PositionUpdate 	+= OnPositionUpdate;	Acct4.ExecutionUpdate 	+= OnExecutionUpdate;	IsAcct4Subscribed = true; }
//					if (Acct5 != null && Acct5 != MasterAccount) 	{ Acct5.PositionUpdate 	+= OnPositionUpdate; Acct5.OrderUpdate 	+= OnOrderUpdate; Acct5.ExecutionUpdate 	+= OnExecutionUpdate;	IsAcct5Subscribed = true; }
					
					if((Enable_1 && Acct1 != null) || (Enable_2 && Acct2 != null) || (Enable_3 && Acct3 != null) || (Enable_4 && Acct4 != null))
					{
						acctFlatTimer.Elapsed += FlattenFollowerPositions;
						acctFlatTimer.Interval = 1500;
						isTimerEventCreated	= true;
					}
				}
				
//				if (MasterAccount != null ) 
//					foreach (Position pos in MasterAccount.Positions)
//					{
//						if(pos.Instrument == Instrument && pos.Account == MasterAccount )
//							masterPosition = pos;
//					}
				#endregion
				
				if(ChartControl != null)		MasterInstr = ChartControl.Instrument;
				
				if (MasterAccountName == null)
				{
					string msg = String.Format("{0}  is DISABLED!  Master Account is NOT set.", indiName);
					Alert(indiName, Priority.High, msg, warningSoundFile, 2, Brushes.DarkRed, Brushes.White);
					Print("<<<<<<<<->>>>>>>>");
					SendLogAndPrint(msg, LogLevel.Warning);
					Print("<<<<<<<<->>>>>>>>");
					return;
				}
				
				#region Feedback Loop check
				if(Enable_1 && Acct1 == MasterAccount && Acct1Instrument == MasterInstr)
				{
					Enable_1 = false;
					SendDisableMessage(1, Acct1Name, Acct1Instrument.FullName);
				}
				if(Enable_2 && Acct2 == MasterAccount && Acct2Instrument == MasterInstr)
				{
					Enable_2 = false;
					SendDisableMessage(2, Acct2Name, Acct2Instrument.FullName);
				}
				if(Enable_3 && Acct3 == MasterAccount && Acct3Instrument == MasterInstr)
				{
					Enable_3 = false;
					SendDisableMessage(3, Acct3Name, Acct3Instrument.FullName);
				}
				if(Enable_4 && Acct4 == MasterAccount && Acct4Instrument == MasterInstr)
				{
					Enable_4 = false;
					SendDisableMessage(4, Acct4Name, Acct4Instrument.FullName);
				}
				#endregion
				
				#endregion
			}
			else if (State == State.Historical)
			{
				#region Historical
				
				IsAcct1Trading = (Enable_1 && Acct1 != null);
				IsAcct2Trading = (Enable_2 && Acct2 != null);
				IsAcct3Trading = (Enable_3 && Acct3 != null);
				IsAcct4Trading = (Enable_4 && Acct4 != null);
//				IsAcct5Trading = (Enable_5 && Acct5 != null);
				if(DEBUG) Print(orderPreName+" "+State+":  IsAcct1Trading =  "+IsAcct1Trading.ToString().PadRight(12)+" IsAcct2Trading =  "+IsAcct2Trading.ToString().PadRight(13)+" IsAcct3Trading =  "+IsAcct3Trading.ToString().PadRight(13)+" IsAcct4Trading =  "+IsAcct4Trading);
				if(DEBUG) Print(orderPreName+" "+State+":  Acct1Instrument: "+(Acct1Instrument!=null?Acct1Instrument.FullName:"null")+" \t Acct2Instrument: "+(Acct2Instrument!=null?Acct2Instrument.FullName:"null")
									+" \t Acct3Instrument: "+(Acct3Instrument!=null?Acct3Instrument.FullName:"null")+" \t Acct4Instrument: "+(Acct4Instrument!=null?Acct4Instrument.FullName:"null"));
				
				if (UserControlCollection.Contains(TCFGrid))
					return;
				
				#region Button
				Dispatcher.InvokeAsync((() =>
				{
					TCFGrid = new System.Windows.Controls.Grid
					{
						Name = "TCFGrid", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top
					};
					if (ButtonVertPosition == TCFVerticalPosition.Bottom)
						TCFGrid.VerticalAlignment = VerticalAlignment.Bottom;
					
					System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
					column1.MaxWidth = 130 +ButtonShift;
					TCFGrid.ColumnDefinitions.Add(column1);
					
					copyButton = new System.Windows.Controls.Button
					{
						Name = "TradeCopierFree", Content = "No Master Acct", Foreground = Brushes.White, Background = Brushes.Maroon, FontSize = 14
					};
					if(MasterAccount != null)
					{
						copyButton.Content = buttonContentCopyOff;
						copyButton.MaxWidth = 90;
					}
					else
						copyButton.MaxWidth = 120;
					copyButton.HorizontalContentAlignment = HorizontalAlignment.Left;
					copyButton.Margin = new Thickness(0, 0, ButtonShift, 0); // Shift away from right edge.
					copyButton.Padding = new Thickness(8, 4, 6, 4);
					
					copyButton.Click += OnButtonClick;
					System.Windows.Controls.Grid.SetColumn(copyButton, 0);
					TCFGrid.Children.Add(copyButton);
					UserControlCollection.Add(TCFGrid);
				}));
				#endregion
				
				#endregion
			}
			else if (State == State.Realtime)
			{
				#region Realtime
				
				isRealtime		= true;
				
				if(DEBUG) Print(orderPreName+" "+State+": \t  MasterAccount: "+(MasterAccount!=null?MasterAccount.Name.PadRight(15):"> null < \t\t")+" MasterInstr: "+MasterInstr.FullName+" \t CheckAllAcctsFlat = "+CheckAllAcctsFlat); 
//				if(DEBUG) Print(orderPreName+" "+State+": \t  IsAcct1Trading =  "+IsAcct1Trading+" \t IsAcct2Trading =  "+IsAcct2Trading+" \t IsAcct3Trading =  "+IsAcct3Trading+" \t IsAcct4Trading =  "+IsAcct4Trading);
//				if(DEBUG) Print(orderPreName+" "+State+": \t Acct1Instrument: "+(Acct1Instrument!=null?Acct1Instrument.FullName:"null")+" \t Acct2Instrument: "+(Acct2Instrument!=null?Acct2Instrument.FullName:"null")
//									+" \t Acct3Instrument: "+(Acct3Instrument!=null?Acct3Instrument.FullName:"null")+" \t Acct4Instrument: "+(Acct4Instrument!=null?Acct4Instrument.FullName:"null"));
			
				#endregion
			}
			else if (State == State.Terminated)
			{	
				#region Terminated
				
				IsCopyAllowed = false;
				Dispatcher.InvokeAsync((() =>
				{
					if (TCFGrid != null)
					{
						if (copyButton != null)
						{
							TCFGrid.Children.Remove(copyButton);
							copyButton.Click -= OnButtonClick;
							copyButton = null;
						}
						TCFGrid = null;
					}
				}));
				
				if (IsMasterSubscribed) //(MasterAccount != null)
				{
					MasterAccount.OrderUpdate 		-= OnOrderUpdate;	MasterAccount.PositionUpdate 	-= OnPositionUpdate;	MasterAccount.ExecutionUpdate 	-= OnExecutionUpdate;
			
					if (IsAcct1Subscribed) 	{ Acct1.OrderUpdate 	-= OnOrderUpdate;	Acct1.PositionUpdate 	-= OnPositionUpdate;	Acct1.ExecutionUpdate 	-= OnExecutionUpdate; }
					if (IsAcct2Subscribed) 	{ Acct2.OrderUpdate 	-= OnOrderUpdate;	Acct2.PositionUpdate 	-= OnPositionUpdate;	Acct2.ExecutionUpdate 	-= OnExecutionUpdate; }
					if (IsAcct3Subscribed) 	{ Acct3.OrderUpdate 	-= OnOrderUpdate;	Acct3.PositionUpdate 	-= OnPositionUpdate;	Acct3.ExecutionUpdate 	-= OnExecutionUpdate; }
					if (IsAcct4Subscribed) 	{ Acct4.OrderUpdate 	-= OnOrderUpdate;	Acct4.PositionUpdate 	-= OnPositionUpdate;	Acct4.ExecutionUpdate 	-= OnExecutionUpdate; }
//					if (IsAcct5Subscribed) 	{ Acct5.OrderUpdate 	-= OnOrderUpdate;	Acct5.PositionUpdate 	-= OnPositionUpdate; Acct5.ExecutionUpdate 	-= OnExecutionUpdate;}
//					if (Acct6 != null) 	{Acct6.PositionUpdate 	-= OnPositionUpdate; Acct6.OrderUpdate 	-= OnOrderUpdate; Acct6.ExecutionUpdate 	-= OnExecutionUpdate;}
//					if (Acct7 != null) 	{Acct7.PositionUpdate 	-= OnPositionUpdate; Acct7.OrderUpdate 	-= OnOrderUpdate; Acct7.ExecutionUpdate 	-= OnExecutionUpdate;}
//					if (Acct8 != null) 	{Acct8.PositionUpdate 	-= OnPositionUpdate; Acct8.OrderUpdate 	-= OnOrderUpdate; Acct8.ExecutionUpdate 	-= OnExecutionUpdate;}
//					if (Acct9 != null) 	{Acct9.PositionUpdate 	-= OnPositionUpdate; Acct9.OrderUpdate 	-= OnOrderUpdate; Acct9.ExecutionUpdate 	-= OnExecutionUpdate;}
				}
				if(isTimerEventCreated)
				{
					acctFlatTimer.Elapsed -= FlattenFollowerPositions;
					isTimerEventCreated	= false;
				}
				#endregion
			}
		}


		private void OnOrderUpdate(object sender, OrderEventArgs e)
    	{
			bool isDebugOk					= DEBUG && (e.OrderState == OrderState.Submitted || e.OrderState == OrderState.Filled || e.OrderState == OrderState.PartFilled);
			if(isDebugOk && IsCopyAllowed)	Print(GetPreFix+" OnOrderUpdate:  Acct: "+e.Order.Account+" \t Inst: "+e.Order.Instrument.FullName+"  \t Qty: "+e.Quantity+" \t Action: "+e.Order.OrderAction+" \t Type: "+e.Order.OrderTypeString+"  \t State: "+e.Order.OrderState);//+"  \t Name: "+e.Order.Name);
//			if(DEBUG) Print(orderPreName+" OnOrderUpdate:  MasterAccount: "+MasterAccount+" \t MasterInstr: "+MasterInstr+" \t Test #!: "+(e.Order.Account != MasterAccount)+" \t Test #2 = "+(e.Order.Instrument != MasterInstr) );
			
			if (IsNotMasterTest(e.Order.Account, e.Order.Instrument) )
			{
//				if(isDebugOk && IsCopyAllowed) Print(GetPreFix+"     - RETURN -   Acct: "+e.Order.Account+" \t  Inst: "+e.Order.Instrument.FullName+"  \t Qty: "+e.Quantity+" \t Action: "+e.Order.OrderAction+" \t Type: "+e.Order.OrderTypeString+"  \t State: "+e.Order.OrderState+"  \t  Name: "+e.Order.Name);
				return;
			}
			
			bool isSubmittedNotCancelled	= e.OrderState == OrderState.Submitted && e.OrderState != OrderState.CancelSubmitted;
			bool isLimitOrStop				= e.Order.IsLimit || e.Order.IsStopMarket || e.Order.IsStopLimit;
			bool isFilled					= e.OrderState == OrderState.Filled || e.OrderState == OrderState.PartFilled;

			if((e.Order.IsMarket && isSubmittedNotCancelled) || (isLimitOrStop && isFilled) )
			{
				if(isDebugOk)
				{
//					if(e.Order.IsMarket)
//						Print(GetPreFix+" \t\t\t\t\t Copying Master Position.  "+e.Quantity+" "+e.Order.OrderAction+" Market order  "+e.OrderState+".");
//					else
						Print(GetPreFix+" \t\t\t\t\t Copying Master Position.  "+e.Quantity+" "+e.Order.OrderAction+" "+e.Order.OrderTypeString+" order "+e.OrderState+".");
				}
				
				if (IsAcct1Trading)		sendOrder(Acct1Instrument, Acct1, e.Order.OrderAction, e.Order.OrderEntry, GetRatioQuantity(e.Quantity, Ratio1), (orderPreName+e.Order.Name));
				if (IsAcct2Trading)		sendOrder(Acct2Instrument, Acct2, e.Order.OrderAction, e.Order.OrderEntry, GetRatioQuantity(e.Quantity, Ratio2), (orderPreName+e.Order.Name));
				if (IsAcct3Trading)		sendOrder(Acct3Instrument, Acct3, e.Order.OrderAction, e.Order.OrderEntry, GetRatioQuantity(e.Quantity, Ratio3), (orderPreName+e.Order.Name));
				if (IsAcct4Trading)		sendOrder(Acct4Instrument, Acct4, e.Order.OrderAction, e.Order.OrderEntry, GetRatioQuantity(e.Quantity, Ratio4), (orderPreName+e.Order.Name));
//				if (IsAcct5Trading)		sendOrder(Acct5Instrument, Acct5, e.Order.OrderAction, OrderType.Market, e.Order.OrderEntry, GetRatioQuantity(e.Order.Quantity, Ratio5), (orderPreName+e.Order.Name));
			}
			
			#region Not Used
//			if (!IsCopyAllowed) return;
			
//			// ----------   ENRTY LONG/SHORT   -----------------------
//			if (e.Order.Account == MasterAccount && e.Order.Instrument == Instrument)
//			{
//				if (e.OrderState == OrderState.Submitted && e.Order.IsMarket && e.OrderState != OrderState.CancelSubmitted )
//				{
//					if (Acct1 != null ) sendOrder(Acct1, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
//					if (Acct2 != null ) sendOrder(Acct2, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
//					if (Acct3 != null ) sendOrder(Acct3, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
//					if (Acct4 != null ) sendOrder(Acct4, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
//					if (Acct5 != null ) sendOrder(Acct5, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
//					if (Acct6 != null ) sendOrder(Acct6, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
//					if (Acct7 != null ) sendOrder(Acct7, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
//					if (Acct8 != null ) sendOrder(Acct8, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
//					if (Acct9 != null ) sendOrder(Acct9, e.Order.OrderAction, e.Order.OrderType, e.Order.Quantity, e.Order.OrderId );
//				}
//			}
//				//---
//			if (e.Order.Account == MasterAccount && e.Order.Instrument == Instrument)
//			{
//				if( (e.Order.IsLimit || e.Order.IsStopMarket) && e.OrderState == OrderState.Filled )
//				{
//					if (Acct1 != null ) sendOrder(Acct1, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
//					if (Acct2 != null ) sendOrder(Acct2, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
//					if (Acct3 != null ) sendOrder(Acct3, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
//					if (Acct4 != null ) sendOrder(Acct4, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
//					if (Acct5 != null ) sendOrder(Acct5, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
//					if (Acct6 != null ) sendOrder(Acct6, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
//					if (Acct7 != null ) sendOrder(Acct7, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
//					if (Acct8 != null ) sendOrder(Acct8, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
//					if (Acct9 != null ) sendOrder(Acct9, e.Order.OrderAction, OrderType.Market, e.Order.Quantity, e.Order.OrderId );
//				}
//			}
			#endregion
	    }
		
		
	    private void OnPositionUpdate(object sender, PositionEventArgs e)
	    {
			#region
			if(!IsCopyAllowed || !isRealtime)	return;
			
			if(DEBUG)	Print(GetPreFix+" OnPositionUpdate:  Acct: "+e.Position.Account+" \t Inst: "+e.Position.Instrument.FullName+" \t MarketPos: "+e.MarketPosition+" \t Qty: "+e.Quantity+" \t Op: "+e.Operation); //+" \t Pos: "+e.Position);
			
			if(e.Position.Account == MasterAccount && e.Position.Instrument == MasterInstr)
			{
				masterAcctMarketPosition = e.MarketPosition;
				if(DEBUG && e.MarketPosition==MarketPosition.Flat)	Print(GetPreFix+"\t\t\t\t\t  Master Position is "+masterAcctMarketPosition.ToString().ToUpper()+"  <<<<<");
				else if(DEBUG)										Print(GetPreFix+"\t\t\t\t\t  Master Position is "+masterAcctMarketPosition+" "+e.Position.Quantity+"  <<<<<");
			}
			else if(e.Position.Account == Acct1 && e.Position.Instrument == Acct1Instrument)
			{
				Acct1MarketPosition = e.MarketPosition;
				if(DEBUG && e.MarketPosition==MarketPosition.Flat)	Print(GetPreFix+"\t\t\t\t\t  Position #1 is "+masterAcctMarketPosition.ToString().ToUpper()+"  -----");
			}
			else if(e.Position.Account == Acct2 && e.Position.Instrument == Acct2Instrument)
			{
				Acct2MarketPosition = e.MarketPosition;
				if(DEBUG && e.MarketPosition==MarketPosition.Flat)	Print(GetPreFix+"\t\t\t\t\t  Position #2 is "+masterAcctMarketPosition.ToString().ToUpper()+"  -----");
			}
			else if(e.Position.Account == Acct3 && e.Position.Instrument == Acct3Instrument)
			{
				Acct3MarketPosition = e.MarketPosition;
				if(DEBUG && e.MarketPosition==MarketPosition.Flat)	Print(GetPreFix+"\t\t\t\t\t  Position #3 is "+masterAcctMarketPosition.ToString().ToUpper()+"  -----");
			}
			else if(e.Position.Account == Acct4 && e.Position.Instrument == Acct4Instrument)
			{
				Acct4MarketPosition = e.MarketPosition;
				if(DEBUG && e.MarketPosition==MarketPosition.Flat)	Print(GetPreFix+"\t\t\t\t\t  Position #4 is "+masterAcctMarketPosition.ToString().ToUpper()+"  -----");
			}
			
			if(CheckAllAcctsFlat && e.Position.Account == MasterAccount && e.Position.Instrument == MasterInstr)
			{
				if(e.MarketPosition == MarketPosition.Flat)
				{
					if(DEBUG)	Print(GetPreFix+"\t\t\t\t\t  Starting timer to flatten Follower Positions.");
					acctFlatTimer.Start();
				}
				else
					acctFlatTimer.Stop();	// Better safe than sorry.
			}
			
			#endregion
		}

//		private void OnAccountItemUpdate(object sender, AccountItemEventArgs e){}	
		private void OnExecutionUpdate(object sender, ExecutionEventArgs e){}
		
		
		private void FlattenFollowerPositions(Object source, ElapsedEventArgs e)
		{
			if(DEBUG)	Print(GetPreFix+" FlattenFollowerPositions:  Acct1MarketPosition: "+Acct1MarketPosition+" \t Acct2MarketPosition: "+Acct2MarketPosition+" \t Acct3MarketPosition: "+Acct3MarketPosition+" \t Acct4MarketPosition: "+Acct4MarketPosition);
			if(!IsCopyAllowed)	return;
			
			if(Acct1MarketPosition != MarketPosition.Flat)
			{
				Acct1.Flatten(new [] { Acct1Instrument });
//				Acct1.Flatten(new [] { Instrument.GetInstrument(Acct1Instrument, false) }); 
				SendAccountFlattenMessage(1, Acct1Name, Acct1Instrument.FullName);
			}
			if(Acct2MarketPosition != MarketPosition.Flat)
			{
				Acct2.Flatten(new [] { Acct2Instrument });
//				Acct2.Flatten(new [] { Instrument.GetInstrument(Acct2Instrument, false) });
				SendAccountFlattenMessage(2, Acct2Name, Acct2Instrument.FullName);
			}
			if(Acct3MarketPosition != MarketPosition.Flat)
			{
				Acct3.Flatten(new [] { Acct3Instrument });
//				Acct3.Flatten(new [] { Instrument.GetInstrument(Acct3Instrument, false) });
				SendAccountFlattenMessage(3, Acct3Name, Acct3Instrument.FullName);
			}
			if(Acct4MarketPosition != MarketPosition.Flat)
			{
				Acct4.Flatten(new [] { Acct4Instrument });
//				Acct4.Flatten(new [] { Instrument.GetInstrument(Acct4Instrument, false) });
				SendAccountFlattenMessage(4, Acct4Name, Acct4Instrument.FullName);
			}
			
			acctFlatTimer.Stop();	// Better safe than sorry.
		}

		
		private void sendOrder(Instrument _inst, Account _acct, OrderAction ordAction, OrderEntry ordEntry, int ordQuantity, string ordName)
		{
			sendOrder(_inst, _acct, ordAction, OrderType.Market, ordEntry, ordQuantity, 0.0, ordName);
		}

		private void sendOrder(Instrument _inst, Account _acct, OrderAction ordAction, OrderType ordType, OrderEntry ordEntry, int ordQuantity, double ordPrice, string ordName)
		{
			Order order = _acct.CreateOrder(_inst, ordAction, ordType, ordEntry, TimeInForce.Day , ordQuantity, ordPrice, ordPrice, "", ordName, Core.Globals.MaxDate, null);
			_acct.Submit(new[] { order });
		}

		
		private int GetRatioQuantity(int _quant, int _ratio)
		{
			return _quant * _ratio;
		}

		
		private bool IsNotMasterTest(Account _mastacct, NinjaTrader.Cbi.Instrument _mastInst)
		{
			return (!IsCopyAllowed || !isRealtime || _mastacct != MasterAccount || _mastInst != MasterInstr);
		}

		
		private string GetPreFix
		{
			get { return String.Format("{0} {1}", orderPreName, DateTime.Now.ToString("HH:mm:ss.FFF").PadRight(12) ); }
		}
		
		
		private void SendDisableMessage(int _x, string _acct, string _instname)
		{
			string msg = String.Format("{0}  is DISABLED!  Can NOT copying orders to Acct #{1}: {2}, Instr: {3}, because it matches the Master Position and would create an infinite feedback loop of order submissions."
										, indiName, _x, _acct, _instname);
			Alert(indiName, Priority.High, msg, warningSoundFile, 2, Brushes.DarkRed, Brushes.White);
			Print("<<<<<<<<->>>>>>>>");
			SendLogAndPrint(msg, LogLevel.Warning);
			Print("<<<<<<<<->>>>>>>>");
		}
		
		
		private void SendAccountFlattenMessage(int _x, string _acct, string _instname)
		{
			string msg = String.Format("{0}:  WARNING Following Position #{1} out of sync!  Acct #{1}: {2}, Instr: {3}  is NOT FLAT yet after 1.5 seconds of Master Position being flat.{4}"+
										"\t\t\t\t\t\t\t\t Issueing the Account.Flatten() command.", indiName, _x, _acct, _instname, Environment.NewLine);
			Alert(indiName, Priority.High, msg, warningSoundFile, 5, Brushes.DarkRed, Brushes.White);
			Print("<<<<<<<<->>>>>>>>");
			SendLogAndPrint(msg, LogLevel.Warning);
			Print("<<<<<<<<->>>>>>>>");
		}
		
		
		private void SendLogAndPrint(string _msg, LogLevel _level)
		{
			Log(_msg, _level);		Print(_msg);
		}

		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			
			if (button == copyButton && button.Name == "TradeCopierFree" && button.Content == buttonContentCopyOff)
			{
				button.Content 		= buttonContentCopyOn;
				button.FontWeight 	= FontWeights.Bold;
				button.Background 	= Brushes.ForestGreen;
				copyButtonClicked 	= true;
				IsCopyAllowed 		= true;
				return;
			}
			else if (button == copyButton && button.Name == "TradeCopierFree" && button.Content == buttonContentCopyOn)
			{
				button.Content 		= buttonContentCopyOff;
				button.FontWeight 	= FontWeights.Regular;
				button.Background 	= Brushes.Maroon;
				copyButtonClicked 	= false;
				IsCopyAllowed 		= false;
				return;
			}			
		}

		
		public override string DisplayName
		{
		  get { return indiNameFull;}
		}
		
		
		protected override void OnBarUpdate()	{ }
		
		

		#region Properties
		
//		[NinjaScriptProperty]
//		[Display(Name = "Quantity Ratio Rounding Formula",			Order = 2, GroupName = "Misc. Parameters")]
//		public TCFRoundingFormula RatioFormula { get; set; }
		
//		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Button Vertical Location", 				Order = 4, GroupName = "Misc. Parameters")]
		public TCFVerticalPosition ButtonVertPosition { get; set; }
		
		[Range(0, 700), NinjaScriptProperty]
		[Display(Name = "Offset Button from Right Edge", 			Order = 6, GroupName = "Misc. Parameters")]
		public int ButtonShift { get; set; }
		
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Copy from this Account:", 					Order = 2, GroupName = "Master Position")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string MasterAccountName { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Check Follower Positions are Flat",			Order = 4, GroupName = "Master Position")]
		public bool CheckAllAcctsFlat { get; set; }
		

//		[NinjaScriptProperty]
//		[Display(Name = "Select Instrument source:",							Order = 4, GroupName = "Master Position")]
//		public TCFInstrumentSelector InstrumentSource { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Instrument : ", 										Order = 6, GroupName = "Master Position")]
//		public string MasterInstr { get; set; }

		
//		[NinjaScriptProperty]
//		[Display(Name = "======================", Order = #3, GroupName = "Follower Positions")]
//		public bool sep_1 { get; set; }
		
/*		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Account 1", 											Order=2, GroupName = "Follower Positions")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Acct1Name { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Account 2", 											Order=3,  GroupName = "Follower Positions")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Acct2Name { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Account #3", 											Order=4,  GroupName = "Follower Positions")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Acct3Name { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Account #4", 											Order=5,  GroupName = "Follower Positions")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Acct4Name { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Account #5", 											Order=6,  GroupName = "Follower Positions")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Acct5Name { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Account #6", 											Order=7,  GroupName = "Follower Positions")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Acct6Name { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Account 7", 											Order=8,  GroupName = "Follower Positions")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Acct7Name { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Account 8", 											Order=9,  GroupName = "Follower Positions")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Acct8Name { get; set; }
		
		[NinjaScriptProperty]//[XmlIgnore]
		[Display(Name="Account 9", 											Order=10,  GroupName = "Follower Positions")]
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string Acct9Name { get; set; }
*/

		[NinjaScriptProperty]
		[Display(Name = "Enable Position #1................",	Order = 10, GroupName = "Follower Positions")]
		public bool Enable_1 { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Position #1 Account", 					Order = 12, GroupName = "Follower Positions")]
		[TypeConverter(typeof (AccountNameConverter))]
		public string Acct1Name { get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[TypeConverter(typeof(NinjaTrader.Gui.Tools.InstrumentSelector))]
		[Display(Name = "Position #1 Instrument : ", 			Order = 14, GroupName = "Follower Positions")]
		public NinjaTrader.Cbi.Instrument Acct1Instrument	
		{ get; set; }

		[Browsable(false)]
		public string Acct1InstrName
		{
			get { return Acct1Instrument.FullName; }
			set { Acct1Instrument = NinjaTrader.Cbi.Instrument.GetInstrumentFuzzy(value); } //, false); } // Serialize.StringToBrush(value); }
		}
		
//		[NinjaScriptProperty]
//		[Display(Name = "Account #1 Instrument : ", 			Order = 14, GroupName = "Follower Positions")]
//		public string Acct1Instrument	{ get; set; }

		[Range(0.001, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "#1 Quantity Ratio", 					Order = 16, GroupName = "Follower Positions")]
		public int Ratio1 { get; set; }


		[NinjaScriptProperty]
		[Display(Name = "Enable Position #2................",	Order = 20, GroupName = "Follower Positions")]
		public bool Enable_2 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Position #2 Account", 					Order = 22, GroupName = "Follower Positions")]
		[TypeConverter(typeof (AccountNameConverter))]
		public string Acct2Name { get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[TypeConverter(typeof(NinjaTrader.Gui.Tools.InstrumentSelector))]
		[Display(Name = "Position #2 Instrument : ", 			Order = 24, GroupName = "Follower Positions")]
		public NinjaTrader.Cbi.Instrument Acct2Instrument	{ get; set; }

		[Browsable(false)]
		public string Acct2InstrName
		{
			get { return Acct2Instrument.FullName; }
			set { Acct2Instrument = NinjaTrader.Cbi.Instrument.GetInstrumentFuzzy(value); } //, false); } // Serialize.StringToBrush(value); }
		}
		
//		[NinjaScriptProperty]
//		[Display(Name = "Account #2 Instrument : ", 			Order = 24, GroupName = "Follower Positions")]
//		public string Acct2Instrument	{ get; set; }

		[Range(0.001, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "#2 Quantity Ratio", 					Order = 26, GroupName = "Follower Positions")]
		public int Ratio2 { get; set; }


		[NinjaScriptProperty]
		[Display(Name = "Enable Position #3................",	Order = 30, GroupName = "Follower Positions")]
		public bool Enable_3 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Position #3 Account", 					Order = 32, GroupName = "Follower Positions")]
		[TypeConverter(typeof (AccountNameConverter))]
		public string Acct3Name { get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[TypeConverter(typeof(NinjaTrader.Gui.Tools.InstrumentSelector))]
		[Display(Name = "Position #3 Instrument : ", 			Order = 34, GroupName = "Follower Positions")]
		public NinjaTrader.Cbi.Instrument Acct3Instrument	{ get; set; }

		[Browsable(false)]
		public string Acct3InstrName
		{
			get { return Acct3Instrument.FullName; }
			set { Acct3Instrument = NinjaTrader.Cbi.Instrument.GetInstrumentFuzzy(value); } //, false); } // Serialize.StringToBrush(value); }
		}

//		[NinjaScriptProperty]
//		[Display(Name = "Account #3 Instrument : ", 			Order = 34, GroupName = "Follower Positions")]
//		public string Acct3Instrument	{ get; set; }

		[Range(0.001, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "#3 Quantity Ratio", 					Order = 36, GroupName = "Follower Positions")]
		public int Ratio3 { get; set; }


		[NinjaScriptProperty]
		[Display(Name = "Enable Position #4................",	Order = 40, GroupName = "Follower Positions")]
		public bool Enable_4 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Position #4 Account", 					Order = 42, GroupName = "Follower Positions")]
		[TypeConverter(typeof (AccountNameConverter))]
		public string Acct4Name { get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[TypeConverter(typeof(NinjaTrader.Gui.Tools.InstrumentSelector))]
		[Display(Name = "Position #4 Instrument : ", 			Order = 44, GroupName = "Follower Positions")]
		public NinjaTrader.Cbi.Instrument Acct4Instrument	{ get; set; }

		[Browsable(false)]
		public string Acct4InstrName
		{
			get { return Acct4Instrument.FullName; }
			set { Acct4Instrument = NinjaTrader.Cbi.Instrument.GetInstrumentFuzzy(value); } //, false); } // Serialize.StringToBrush(value); }
		}

//		[NinjaScriptProperty]
//		[Display(Name = "Account #4 Instrument : ", 			Order = 44, GroupName = "Follower Positions")]
//		public string Acct4Instrument	{ get; set; }

		[Range(0.001, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "#4 Quantity Ratio", 					Order = 46, GroupName = "Follower Positions")]
		public int Ratio4 { get; set; }


//		[NinjaScriptProperty]
//		[Display(Name = "Enable Account #5", 				Order = 50, GroupName = "Follower Positions")]
//		public bool Enable_5 { get; set; }
		
//		[NinjaScriptProperty]
//		[Display(Name = "Account #5", 					Order = 52, GroupName = "Follower Positions")]
//		[TypeConverter(typeof (AccountNameConverter))]
//		public string Acct5Name { get; set; }
		
//		[NinjaScriptProperty]
//		[Display(Name = "Account #5 Instrument : ", 			Order = 54, GroupName = "Follower Positions")]
//		[TypeConverter(typeof(NinjaTrader.Gui.Tools.InstrumentSelector))]
//		public NinjaTrader.Cbi.Instrument Acct5Instrument	{ get; set; }
//		[NinjaScriptProperty]
//		[Display(Name = "Account #5 Instrument : ", 			Order = 54, GroupName = "Follower Positions")]
//		public string Acct5InstrName { get; set; }

//		[Range(0.001, int.MaxValue), NinjaScriptProperty]
//		[Display(Name = "#5 Quantity Ratio", 					Order = 56, GroupName = "Follower Positions")]
//		public int Ratio5 { get; set; }

		
//		[NinjaScriptProperty]
//		[Display(Name = "Account #6", 					Order = 64, GroupName = "Follower Positions")]
//		[TypeConverter(typeof (AccountNameConverter))]
//		public string Acct6Name { get; set; }
		
//		[NinjaScriptProperty]
//		[Display(Name = "Acct. #6 Instrument : ", 			Order = 65, GroupName = "Follower Positions")]
//		public string Acct6InstrName { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Acct. #6 Quantity Ratio", 					Order = 66, GroupName = "Follower Positions")]
//		public double Ratio6 { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Enable Acct. #6 ", 				Order = 60, GroupName = "Follower Positions")]
//		public bool Enable_6 { get; set; }

		
//		[NinjaScriptProperty]
//		[Display(Name = "Account 7", 					Order = 74, GroupName = "Follower Positions")]
//		[TypeConverter(typeof (AccountNameConverter))]
//		public string Acct7Name { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Acct. 7 Instrument : ", 			Order = 75, GroupName = "Follower Positions")]
//		public string Acct7InstrName { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Acct. 7 Quantity Ratio", 					Order = 76, GroupName = "Follower Positions")]
//		public double Ratio7 { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Enable Acct. #7 ", 				Order = 70, GroupName = "Follower Positions")]
//		public bool Enable_7 { get; set; }

		
//		[NinjaScriptProperty]
//		[Display(Name = "Account 8", 					Order = 84, GroupName = "Follower Positions")]
//		[TypeConverter(typeof (AccountNameConverter))]
//		public string Acct8Name { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Acct. 8 Instrument : ", 			Order = 85, GroupName = "Follower Positions")]
//		public string Acct8InstrName { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Acct. 8 Quantity Ratio", 					Order = 86, GroupName = "Follower Positions")]
//		public double Ratio8 { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Enable Acct. #8 ", 				Order = 80, GroupName = "Follower Positions")]
//		public bool Enable_8 { get; set; }

		
//		[NinjaScriptProperty]
//		[Display(Name = "Account 9", 					Order = 94, GroupName = "Follower Positions")]
//		[TypeConverter(typeof (AccountNameConverter))]
//		public string Acct9Name { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Acct. 9 Instrument : ", 			Order = 95, GroupName = "Follower Positions")]
//		public string Acct9InstrName { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Acct. 9 Quantity Ratio", 		Order = 96, GroupName = "Follower Positions")]
//		public double Ratio9 { get; set; }

//		[NinjaScriptProperty]
//		[Display(Name = "Enable Acct. #9 ", 				Order = 90, GroupName = "Follower Positions")]
//		public bool Enable_9 { get; set; }
		
// ----- Debug ----------------------------------------------------------------------------------------------------------
		[NinjaScriptProperty]
		[Display(Name="Enable Debug messaging", Description="Send Debug info to the Output window.", GroupName="Debug", Order=2)]
		public bool Debug
		{
			get{return DEBUG;}
			set{DEBUG = value;}
		}
		
		#endregion
	}

}


public enum TCFVerticalPosition
{
    Top		= 1,
    Bottom	= 2
}
//
//
//

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeCopierFree[] cacheTradeCopierFree;
		public TradeCopierFree TradeCopierFree(TCFVerticalPosition buttonVertPosition, int buttonShift, string masterAccountName, bool checkAllAcctsFlat, bool enable_1, string acct1Name, NinjaTrader.Cbi.Instrument acct1Instrument, int ratio1, bool enable_2, string acct2Name, NinjaTrader.Cbi.Instrument acct2Instrument, int ratio2, bool enable_3, string acct3Name, NinjaTrader.Cbi.Instrument acct3Instrument, int ratio3, bool enable_4, string acct4Name, NinjaTrader.Cbi.Instrument acct4Instrument, int ratio4, bool debug)
		{
			return TradeCopierFree(Input, buttonVertPosition, buttonShift, masterAccountName, checkAllAcctsFlat, enable_1, acct1Name, acct1Instrument, ratio1, enable_2, acct2Name, acct2Instrument, ratio2, enable_3, acct3Name, acct3Instrument, ratio3, enable_4, acct4Name, acct4Instrument, ratio4, debug);
		}

		public TradeCopierFree TradeCopierFree(ISeries<double> input, TCFVerticalPosition buttonVertPosition, int buttonShift, string masterAccountName, bool checkAllAcctsFlat, bool enable_1, string acct1Name, NinjaTrader.Cbi.Instrument acct1Instrument, int ratio1, bool enable_2, string acct2Name, NinjaTrader.Cbi.Instrument acct2Instrument, int ratio2, bool enable_3, string acct3Name, NinjaTrader.Cbi.Instrument acct3Instrument, int ratio3, bool enable_4, string acct4Name, NinjaTrader.Cbi.Instrument acct4Instrument, int ratio4, bool debug)
		{
			if (cacheTradeCopierFree != null)
				for (int idx = 0; idx < cacheTradeCopierFree.Length; idx++)
					if (cacheTradeCopierFree[idx] != null && cacheTradeCopierFree[idx].ButtonVertPosition == buttonVertPosition && cacheTradeCopierFree[idx].ButtonShift == buttonShift && cacheTradeCopierFree[idx].MasterAccountName == masterAccountName && cacheTradeCopierFree[idx].CheckAllAcctsFlat == checkAllAcctsFlat && cacheTradeCopierFree[idx].Enable_1 == enable_1 && cacheTradeCopierFree[idx].Acct1Name == acct1Name && cacheTradeCopierFree[idx].Acct1Instrument == acct1Instrument && cacheTradeCopierFree[idx].Ratio1 == ratio1 && cacheTradeCopierFree[idx].Enable_2 == enable_2 && cacheTradeCopierFree[idx].Acct2Name == acct2Name && cacheTradeCopierFree[idx].Acct2Instrument == acct2Instrument && cacheTradeCopierFree[idx].Ratio2 == ratio2 && cacheTradeCopierFree[idx].Enable_3 == enable_3 && cacheTradeCopierFree[idx].Acct3Name == acct3Name && cacheTradeCopierFree[idx].Acct3Instrument == acct3Instrument && cacheTradeCopierFree[idx].Ratio3 == ratio3 && cacheTradeCopierFree[idx].Enable_4 == enable_4 && cacheTradeCopierFree[idx].Acct4Name == acct4Name && cacheTradeCopierFree[idx].Acct4Instrument == acct4Instrument && cacheTradeCopierFree[idx].Ratio4 == ratio4 && cacheTradeCopierFree[idx].Debug == debug && cacheTradeCopierFree[idx].EqualsInput(input))
						return cacheTradeCopierFree[idx];
			return CacheIndicator<TradeCopierFree>(new TradeCopierFree(){ ButtonVertPosition = buttonVertPosition, ButtonShift = buttonShift, MasterAccountName = masterAccountName, CheckAllAcctsFlat = checkAllAcctsFlat, Enable_1 = enable_1, Acct1Name = acct1Name, Acct1Instrument = acct1Instrument, Ratio1 = ratio1, Enable_2 = enable_2, Acct2Name = acct2Name, Acct2Instrument = acct2Instrument, Ratio2 = ratio2, Enable_3 = enable_3, Acct3Name = acct3Name, Acct3Instrument = acct3Instrument, Ratio3 = ratio3, Enable_4 = enable_4, Acct4Name = acct4Name, Acct4Instrument = acct4Instrument, Ratio4 = ratio4, Debug = debug }, input, ref cacheTradeCopierFree);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeCopierFree TradeCopierFree(TCFVerticalPosition buttonVertPosition, int buttonShift, string masterAccountName, bool checkAllAcctsFlat, bool enable_1, string acct1Name, NinjaTrader.Cbi.Instrument acct1Instrument, int ratio1, bool enable_2, string acct2Name, NinjaTrader.Cbi.Instrument acct2Instrument, int ratio2, bool enable_3, string acct3Name, NinjaTrader.Cbi.Instrument acct3Instrument, int ratio3, bool enable_4, string acct4Name, NinjaTrader.Cbi.Instrument acct4Instrument, int ratio4, bool debug)
		{
			return indicator.TradeCopierFree(Input, buttonVertPosition, buttonShift, masterAccountName, checkAllAcctsFlat, enable_1, acct1Name, acct1Instrument, ratio1, enable_2, acct2Name, acct2Instrument, ratio2, enable_3, acct3Name, acct3Instrument, ratio3, enable_4, acct4Name, acct4Instrument, ratio4, debug);
		}

		public Indicators.TradeCopierFree TradeCopierFree(ISeries<double> input , TCFVerticalPosition buttonVertPosition, int buttonShift, string masterAccountName, bool checkAllAcctsFlat, bool enable_1, string acct1Name, NinjaTrader.Cbi.Instrument acct1Instrument, int ratio1, bool enable_2, string acct2Name, NinjaTrader.Cbi.Instrument acct2Instrument, int ratio2, bool enable_3, string acct3Name, NinjaTrader.Cbi.Instrument acct3Instrument, int ratio3, bool enable_4, string acct4Name, NinjaTrader.Cbi.Instrument acct4Instrument, int ratio4, bool debug)
		{
			return indicator.TradeCopierFree(input, buttonVertPosition, buttonShift, masterAccountName, checkAllAcctsFlat, enable_1, acct1Name, acct1Instrument, ratio1, enable_2, acct2Name, acct2Instrument, ratio2, enable_3, acct3Name, acct3Instrument, ratio3, enable_4, acct4Name, acct4Instrument, ratio4, debug);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeCopierFree TradeCopierFree(TCFVerticalPosition buttonVertPosition, int buttonShift, string masterAccountName, bool checkAllAcctsFlat, bool enable_1, string acct1Name, NinjaTrader.Cbi.Instrument acct1Instrument, int ratio1, bool enable_2, string acct2Name, NinjaTrader.Cbi.Instrument acct2Instrument, int ratio2, bool enable_3, string acct3Name, NinjaTrader.Cbi.Instrument acct3Instrument, int ratio3, bool enable_4, string acct4Name, NinjaTrader.Cbi.Instrument acct4Instrument, int ratio4, bool debug)
		{
			return indicator.TradeCopierFree(Input, buttonVertPosition, buttonShift, masterAccountName, checkAllAcctsFlat, enable_1, acct1Name, acct1Instrument, ratio1, enable_2, acct2Name, acct2Instrument, ratio2, enable_3, acct3Name, acct3Instrument, ratio3, enable_4, acct4Name, acct4Instrument, ratio4, debug);
		}

		public Indicators.TradeCopierFree TradeCopierFree(ISeries<double> input , TCFVerticalPosition buttonVertPosition, int buttonShift, string masterAccountName, bool checkAllAcctsFlat, bool enable_1, string acct1Name, NinjaTrader.Cbi.Instrument acct1Instrument, int ratio1, bool enable_2, string acct2Name, NinjaTrader.Cbi.Instrument acct2Instrument, int ratio2, bool enable_3, string acct3Name, NinjaTrader.Cbi.Instrument acct3Instrument, int ratio3, bool enable_4, string acct4Name, NinjaTrader.Cbi.Instrument acct4Instrument, int ratio4, bool debug)
		{
			return indicator.TradeCopierFree(input, buttonVertPosition, buttonShift, masterAccountName, checkAllAcctsFlat, enable_1, acct1Name, acct1Instrument, ratio1, enable_2, acct2Name, acct2Instrument, ratio2, enable_3, acct3Name, acct3Instrument, ratio3, enable_4, acct4Name, acct4Instrument, ratio4, debug);
		}
	}
}

#endregion
