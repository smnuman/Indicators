// Coded by Chelsea Bell. chelsea.bell@ninjatrader.com
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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ChartTraderGetSelectedAccountExample : Indicator
	{
		private System.Windows.Controls.Grid	buttonGrid;
		private System.Windows.Controls.Button	changeButton;
		private string							message;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"";
				Name						= "ChartTraderGetSelectedAccountExample";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= false;
				DrawOnPricePanel			= true;
				IsSuspendedWhileInactive	= true;
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						InsertWPFControls();
					}));
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						RemoveWPFControls();
					}));
				}
			}
		}

		protected override void OnBarUpdate() { }

		protected void ButtonClick(object sender, RoutedEventArgs rea)
		{
			NinjaTrader.Gui.Tools.AccountSelector chartTraderAccountSelector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;

			message = "Account selector null or no account selected";

			if (chartTraderAccountSelector != null && chartTraderAccountSelector.SelectedAccount != null)
				message = string.Format("Selected account DisplayName: {1} \r\n (Full) Name: {2}", DateTime.Now, chartTraderAccountSelector.SelectedAccount.DisplayName, chartTraderAccountSelector.SelectedAccount.Name);
			
			Print(string.Format("{0} | {1}", DateTime.Now, message));
			Draw.TextFixed(this, "infobox", message, TextPosition.BottomLeft, Brushes.MediumTurquoise, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, ChartControl.Properties.ChartBackground, 100);
			ForceRefresh();
		}

		protected void InsertWPFControls()
		{
			buttonGrid = new System.Windows.Controls.Grid
			{
				Name				= "MyCustomGrid",
				HorizontalAlignment	= HorizontalAlignment.Left,
				Margin				= new Thickness(10, 28, 0, 0),
				VerticalAlignment	= VerticalAlignment.Top
			};

			System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();

			buttonGrid.ColumnDefinitions.Add(column1);

			changeButton = new System.Windows.Controls.Button
			{
				Name				= "ShowAccount",
				Content				= "Show ChartTrader selected account",
				Foreground			= Brushes.White,
				Background			= Brushes.Green
			};

			changeButton.Click += ButtonClick;

			System.Windows.Controls.Grid.SetColumn(changeButton, 0);

			buttonGrid.Children.Add(changeButton);

			UserControlCollection.Add(buttonGrid);
		}

		protected void RemoveWPFControls()
		{
			if (changeButton != null)
				changeButton.Click -= ButtonClick;
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ChartTraderGetSelectedAccountExample[] cacheChartTraderGetSelectedAccountExample;
		public ChartTraderGetSelectedAccountExample ChartTraderGetSelectedAccountExample()
		{
			return ChartTraderGetSelectedAccountExample(Input);
		}

		public ChartTraderGetSelectedAccountExample ChartTraderGetSelectedAccountExample(ISeries<double> input)
		{
			if (cacheChartTraderGetSelectedAccountExample != null)
				for (int idx = 0; idx < cacheChartTraderGetSelectedAccountExample.Length; idx++)
					if (cacheChartTraderGetSelectedAccountExample[idx] != null &&  cacheChartTraderGetSelectedAccountExample[idx].EqualsInput(input))
						return cacheChartTraderGetSelectedAccountExample[idx];
			return CacheIndicator<ChartTraderGetSelectedAccountExample>(new ChartTraderGetSelectedAccountExample(), input, ref cacheChartTraderGetSelectedAccountExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChartTraderGetSelectedAccountExample ChartTraderGetSelectedAccountExample()
		{
			return indicator.ChartTraderGetSelectedAccountExample(Input);
		}

		public Indicators.ChartTraderGetSelectedAccountExample ChartTraderGetSelectedAccountExample(ISeries<double> input )
		{
			return indicator.ChartTraderGetSelectedAccountExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChartTraderGetSelectedAccountExample ChartTraderGetSelectedAccountExample()
		{
			return indicator.ChartTraderGetSelectedAccountExample(Input);
		}

		public Indicators.ChartTraderGetSelectedAccountExample ChartTraderGetSelectedAccountExample(ISeries<double> input )
		{
			return indicator.ChartTraderGetSelectedAccountExample(input);
		}
	}
}

#endregion
