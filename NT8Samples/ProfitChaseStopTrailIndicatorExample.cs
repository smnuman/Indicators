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
namespace NinjaTrader.NinjaScript.Indicators.NT8Samples
{
	public class ProfitChaseStopTrailIndicatorExample : Indicator
	{
		private Position					accountPosition;
		private List<Order>					changeOrdersArray, submitOrdersArray;
		private double						currentPtPrice, currentSlPrice;
		private Order						entryBuyMarketOrder, profitTargetOrder, stopLossOrder;
		private Account						submissionAccount;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name										= "ProfitChaseStopTrailIndicatorExample";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= false;

				ChaseProfitTarget							= true;
				PrintDetails								= false;
				ProfitTargetDistance						= 10;
				StopLossDistance							= 10;
				TrailStopLoss								= true;
				UseProfitTarget						 		= true;
				UseStopLoss									= true;
			}
			else if (State == State.DataLoaded)
			{
				submissionAccount = Account.All.FirstOrDefault(a => a.Name == "Sim101");

				if (submissionAccount != null)
					submissionAccount.OrderUpdate	+= Account_OrderUpdate;
			}
			else if (State == State.Terminated)
			{
				if (submissionAccount != null)
					submissionAccount.OrderUpdate	-= Account_OrderUpdate;
			}
		}

		private void Account_OrderUpdate(object sender, OrderEventArgs orderUpdateArgs)
		{
			if (entryBuyMarketOrder != null && entryBuyMarketOrder == orderUpdateArgs.Order && orderUpdateArgs.Order.OrderState == OrderState.Filled)
			{
				string oco			= Guid.NewGuid().ToString("N");
				submitOrdersArray	= new List<Order>();

				if (UseProfitTarget)
				{
					currentPtPrice = orderUpdateArgs.AverageFillPrice + ProfitTargetDistance * TickSize;

					if (PrintDetails)
						Print(string.Format("{0} | Account_OrderUpdate | placing profit target | currentPtPrice: {1}", orderUpdateArgs.Time, currentPtPrice));

					profitTargetOrder = submissionAccount.CreateOrder(orderUpdateArgs.Order.Instrument, OrderAction.Sell, OrderType.Limit, OrderEntry.Automated, TimeInForce.Day, orderUpdateArgs.Quantity, currentPtPrice, 0, oco, "Profit Target", Core.Globals.MaxDate, null);
					submitOrdersArray.Add(profitTargetOrder);
				}

				if (UseStopLoss)
				{
					currentSlPrice = orderUpdateArgs.AverageFillPrice - StopLossDistance * TickSize;

					if (PrintDetails)
						Print(string.Format("{0} | Account_OrderUpdate | placing stop loss | currentSlPrice: {1}", orderUpdateArgs.Time, currentSlPrice));
					
					stopLossOrder = submissionAccount.CreateOrder(orderUpdateArgs.Order.Instrument, OrderAction.Sell, OrderType.StopMarket, OrderEntry.Automated, TimeInForce.Day, orderUpdateArgs.Quantity, 0, currentSlPrice, oco, "Stop Loss", Core.Globals.MaxDate, null);
					submitOrdersArray.Add(stopLossOrder);
				}

				submissionAccount.Submit(submitOrdersArray);
			}

			// once the exit orders are closed, reset for a new entry.
			else if ((profitTargetOrder != null && (profitTargetOrder.OrderState == OrderState.Filled || profitTargetOrder.OrderState == OrderState.Rejected || profitTargetOrder.OrderState == OrderState.Cancelled)) || (stopLossOrder != null && (stopLossOrder.OrderState == OrderState.Filled || stopLossOrder.OrderState == OrderState.Rejected || stopLossOrder.OrderState == OrderState.Cancelled)))
			{
				entryBuyMarketOrder		= null;
				profitTargetOrder		= null;
				stopLossOrder			= null;
			}
		}

		protected override void OnBarUpdate()
		{
			if (State != State.Realtime)
				return;

			// check if the account position for this instrument is flat, if so submit entry

			if (submissionAccount != null && submissionAccount.Positions.Where(o => o.Instrument == Instrument).Count() > 0)
				accountPosition = submissionAccount.Positions.Where(o => o.Instrument == Instrument).Last();
			else
				accountPosition = null;

			if (IsFirstTickOfBar && (accountPosition == null || accountPosition.MarketPosition == MarketPosition.Flat))
			{
				if (PrintDetails)
					Print(string.Format("{0} | OOU | submitting entry", Time[0], currentPtPrice));
				// the name of the order must be Entry or the order will get stuck in the intialize state
				entryBuyMarketOrder = submissionAccount.CreateOrder(Instrument, OrderAction.Buy, OrderType.Market, OrderEntry.Automated, TimeInForce.Day, 1, 0, 0, string.Empty, "Entry", Core.Globals.MaxDate, null);

				submissionAccount.Submit(new[] { entryBuyMarketOrder });
			}
			// else ensure the stop and targets are working, and see if these need to be trailed / chased.
			else
			{
				 changeOrdersArray = new List<Order>();

				if (ChaseProfitTarget && profitTargetOrder != null &&
						(profitTargetOrder.OrderState == OrderState.Accepted || profitTargetOrder.OrderState == OrderState.Working) &&
						Close[0] < currentPtPrice - ProfitTargetDistance * TickSize)
				{
					currentPtPrice						= Close[0] + ProfitTargetDistance * TickSize;
					profitTargetOrder.LimitPriceChanged = currentPtPrice;
					changeOrdersArray.Add(profitTargetOrder);

					if (PrintDetails)
						Print(string.Format("{0} | OOU | chasing target, currentPtPrice: {1}", Time[0], currentPtPrice));
				}

				if (TrailStopLoss && stopLossOrder != null &&
						(stopLossOrder.OrderState == OrderState.Accepted || stopLossOrder.OrderState == OrderState.Working) &&
						Close[0] > currentSlPrice + StopLossDistance * TickSize)
				{
					currentSlPrice						= Close[0] - StopLossDistance * TickSize;
					stopLossOrder.StopPriceChanged		= currentSlPrice;
					changeOrdersArray.Add(stopLossOrder);

					if (PrintDetails)
						Print(string.Format("{0} | OOU | trailing stop, currentPtPrice: {1}", Time[0], currentSlPrice));
				}

				if (changeOrdersArray.Count() > 0)
					submissionAccount.Change(changeOrdersArray);
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "Chase profit target", Order = 2, GroupName = "NinjaScriptStrategyParameters")]
		public bool ChaseProfitTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Profit target distance", Description = "Distance for profit target (in ticks)", Order = 3, GroupName = "NinjaScriptStrategyParameters")]
		public int ProfitTargetDistance
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Print details", Order = 7, GroupName = "NinjaScriptStrategyParameters")]
		public bool PrintDetails
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Stop loss distance", Description = "Distance for stop loss (in ticks)", Order = 6, GroupName = "NinjaScriptStrategyParameters")]
		public int StopLossDistance
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Trail stop loss", Order = 5, GroupName = "NinjaScriptStrategyParameters")]
		public bool TrailStopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Use profit target", Order = 1, GroupName = "NinjaScriptStrategyParameters")]
		public bool UseProfitTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Use stop loss", Order = 4, GroupName = "NinjaScriptStrategyParameters")]
		public bool UseStopLoss
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private NT8Samples.ProfitChaseStopTrailIndicatorExample[] cacheProfitChaseStopTrailIndicatorExample;
		public NT8Samples.ProfitChaseStopTrailIndicatorExample ProfitChaseStopTrailIndicatorExample(bool chaseProfitTarget, int profitTargetDistance, bool printDetails, int stopLossDistance, bool trailStopLoss, bool useProfitTarget, bool useStopLoss)
		{
			return ProfitChaseStopTrailIndicatorExample(Input, chaseProfitTarget, profitTargetDistance, printDetails, stopLossDistance, trailStopLoss, useProfitTarget, useStopLoss);
		}

		public NT8Samples.ProfitChaseStopTrailIndicatorExample ProfitChaseStopTrailIndicatorExample(ISeries<double> input, bool chaseProfitTarget, int profitTargetDistance, bool printDetails, int stopLossDistance, bool trailStopLoss, bool useProfitTarget, bool useStopLoss)
		{
			if (cacheProfitChaseStopTrailIndicatorExample != null)
				for (int idx = 0; idx < cacheProfitChaseStopTrailIndicatorExample.Length; idx++)
					if (cacheProfitChaseStopTrailIndicatorExample[idx] != null && cacheProfitChaseStopTrailIndicatorExample[idx].ChaseProfitTarget == chaseProfitTarget && cacheProfitChaseStopTrailIndicatorExample[idx].ProfitTargetDistance == profitTargetDistance && cacheProfitChaseStopTrailIndicatorExample[idx].PrintDetails == printDetails && cacheProfitChaseStopTrailIndicatorExample[idx].StopLossDistance == stopLossDistance && cacheProfitChaseStopTrailIndicatorExample[idx].TrailStopLoss == trailStopLoss && cacheProfitChaseStopTrailIndicatorExample[idx].UseProfitTarget == useProfitTarget && cacheProfitChaseStopTrailIndicatorExample[idx].UseStopLoss == useStopLoss && cacheProfitChaseStopTrailIndicatorExample[idx].EqualsInput(input))
						return cacheProfitChaseStopTrailIndicatorExample[idx];
			return CacheIndicator<NT8Samples.ProfitChaseStopTrailIndicatorExample>(new NT8Samples.ProfitChaseStopTrailIndicatorExample(){ ChaseProfitTarget = chaseProfitTarget, ProfitTargetDistance = profitTargetDistance, PrintDetails = printDetails, StopLossDistance = stopLossDistance, TrailStopLoss = trailStopLoss, UseProfitTarget = useProfitTarget, UseStopLoss = useStopLoss }, input, ref cacheProfitChaseStopTrailIndicatorExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.NT8Samples.ProfitChaseStopTrailIndicatorExample ProfitChaseStopTrailIndicatorExample(bool chaseProfitTarget, int profitTargetDistance, bool printDetails, int stopLossDistance, bool trailStopLoss, bool useProfitTarget, bool useStopLoss)
		{
			return indicator.ProfitChaseStopTrailIndicatorExample(Input, chaseProfitTarget, profitTargetDistance, printDetails, stopLossDistance, trailStopLoss, useProfitTarget, useStopLoss);
		}

		public Indicators.NT8Samples.ProfitChaseStopTrailIndicatorExample ProfitChaseStopTrailIndicatorExample(ISeries<double> input , bool chaseProfitTarget, int profitTargetDistance, bool printDetails, int stopLossDistance, bool trailStopLoss, bool useProfitTarget, bool useStopLoss)
		{
			return indicator.ProfitChaseStopTrailIndicatorExample(input, chaseProfitTarget, profitTargetDistance, printDetails, stopLossDistance, trailStopLoss, useProfitTarget, useStopLoss);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.NT8Samples.ProfitChaseStopTrailIndicatorExample ProfitChaseStopTrailIndicatorExample(bool chaseProfitTarget, int profitTargetDistance, bool printDetails, int stopLossDistance, bool trailStopLoss, bool useProfitTarget, bool useStopLoss)
		{
			return indicator.ProfitChaseStopTrailIndicatorExample(Input, chaseProfitTarget, profitTargetDistance, printDetails, stopLossDistance, trailStopLoss, useProfitTarget, useStopLoss);
		}

		public Indicators.NT8Samples.ProfitChaseStopTrailIndicatorExample ProfitChaseStopTrailIndicatorExample(ISeries<double> input , bool chaseProfitTarget, int profitTargetDistance, bool printDetails, int stopLossDistance, bool trailStopLoss, bool useProfitTarget, bool useStopLoss)
		{
			return indicator.ProfitChaseStopTrailIndicatorExample(input, chaseProfitTarget, profitTargetDistance, printDetails, stopLossDistance, trailStopLoss, useProfitTarget, useStopLoss);
		}
	}
}

#endregion
