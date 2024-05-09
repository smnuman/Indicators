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
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.DirectWrite;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class RKRiskRuler : Indicator
	{
		private Point			clickPoint	= new Point();
		private bool			clickSet	= false;
		
		private bool 			showTicks	= true;		/// Mod - Add : Numan
		private bool			showDetails	= false;	/// Mod - Add : Numan

		private double			myStop 		= 0;
		private double			myEntry 	= 0;
		private double			myR1 		= 0;
		private double			myR1_5 		= 0;	/// Mod - Add : Numan
		private double			myR2 		= 0;
		private double			myR2_5 		= 0;	/// Mod - Add : Numan
		private double			myR3 		= 0;
		private double			myR4 		= 0;	/// Mod - Add : Numan
		private double			myR5 		= 0;	/// Mod - Add : Numan
		private double			myRisk		= 0;
		private double			myTarget1	= 0;
		private double			myTarget2	= 0;
		private double			myTarget1_5	= 0;	/// Mod - Add : Numan
		private double			myTarget2_5	= 0;	/// Mod - Add : Numan
		private double			myTarget3	= 0;
		private double			myTarget4	= 0;	/// Mod - Add : Numan
		private double			myTarget5	= 0;	/// Mod - Add : Numan
		private int				barIdx 		= 0;
		private int				barPrev 	= 0;
		
		private System.Windows.Media.Brush	stopBrush;
		private System.Windows.Media.Brush	entryBrush;
		private System.Windows.Media.Brush	r1Brush;
		private System.Windows.Media.Brush	r2Brush;
		private System.Windows.Media.Brush	r3Brush;
		private System.Windows.Media.Brush	r4Brush;	/// Mod - Add : Numan
		private System.Windows.Media.Brush	r5Brush;	/// Mod - Add : Numan
		private System.Windows.Media.Brush	rMidBrush;	/// Mod - Add : Numan
		
		private string 			myCurrency	= "$";		/// Option: Need to set the account currency -- Mod by Numan
		
		/// <summary>
        /// Return the number of decimal places in the argument.
        /// </summary>
        /// <param name="dblNumber">Value to process.</param>
        /// <returns>Number of decimal places in dblNumber.</returns>
        protected int GetNumberDecimalPlaces(double dblNumber)
        {
            string[] parts = dblNumber.ToString().Split(new Char[] { '.', ',' });
            return parts[0] != dblNumber.ToString() ? parts[1].Length : 0;
        }
		
		/// <summary>
        /// Return the decimal part of the number in the argument.
        /// </summary>
        /// <param name="dblNumber">Value to process.</param>
        /// <returns>Decimal part of the dblNumber.</returns>
        protected int GetNumberDecimal(double dblNumber)
        {
            string[] parts 	 = dblNumber.ToString().Split(new Char[] { '.', ',' });
            return parts[0] != dblNumber.ToString() ? Convert.ToInt32(parts[1]) : 0;
        }
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Draws lines for Stop, Entry, R1, R2 and R3 on middle Mouse-Click. The extended version includes R4, R5 and the mids between R1 & R2 and R2 & R3. Some other tweaks are added too.";
				Name										= "RKRiskRuler";
				#region * Indicator defaults *
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				#endregion
				
				#region Brush
				StopBrush 									= System.Windows.Media.Brushes.Red;
				EntryBrush									= System.Windows.Media.Brushes.Cyan;
				R1Brush										= System.Windows.Media.Brushes.PaleGreen;
				R2Brush										= System.Windows.Media.Brushes.LimeGreen;
				RMidBrush									= System.Windows.Media.Brushes.Gray;	/// Mod - Add : Numan
				RMidBrush									= System.Windows.Media.Brushes.Gray;	/// Mod - Add : Numan
				R3Brush										= System.Windows.Media.Brushes.OliveDrab;
				R4Brush										= System.Windows.Media.Brushes.DarkOliveGreen;	/// Mod - Add : Numan
				R5Brush										= System.Windows.Media.Brushes.ForestGreen;	/// Mod - Add : Numan
				#endregion
				
				#region Line Width
				StopWidth									= 2;
				EntryWidth									= 2;
				R1Width										= 2;
				R2Width										= 2;
				RMidWidth									= 1;	/// Mod - Add : Numan
				R3Width										= 2;
				R4Width										= 2;	/// Mod - Add : Numan
				R5Width										= 2;	/// Mod - Add : Numan
				
				RulerWidth									= 8;
				#endregion
				
				#region ** Line Adjustments to price in ticks **
				StopAdjustTicks								= 0;
				EntryAdjustTicks							= 0;
				MaxRiskTicks								= 0;
				#endregion
				
				#region * Show On/Offs *
				ShowR2										= false;
				ShowR3										= false;
				ShowR4										= false;	/// Mod - Add : Numan
				ShowR5										= false;	/// Mod - Add : Numan
				ShowMids									= true;		/// Mod - Add : Numan
				ShowTicks									= true;
				ShowDetails									= false;
				
				debug										= false;
				#endregion
				
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.Historical)
			{
				//SetZOrder(-1); // default here is go below the bars and called in State.Historical
				SetZOrder(int.MaxValue); //move to front
			}
			else if (State == State.DataLoaded)
			{
					ChartControl.ChartPanels[0].MouseDown += MiddleMouseButtonExample_MouseDown;
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
					ChartControl.ChartPanels[0].MouseDown -= MiddleMouseButtonExample_MouseDown;
			}
		}

		protected override void OnBarUpdate() {}

		public void MiddleMouseButtonExample_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.MiddleButton == MouseButtonState.Pressed)
			{
				// convert e.GetPosition for different dpi settings
				clickPoint.X = ChartingExtensions.ConvertToHorizontalPixels(e.GetPosition(ChartPanel as IInputElement).X, ChartControl.PresentationSource);
				clickPoint.Y = ChartingExtensions.ConvertToVerticalPixels(e.GetPosition(ChartPanel as IInputElement).Y, ChartControl.PresentationSource);

				if (clickPoint.Y > 0)
				{
					clickSet = true;
					barIdx = -1;
				}
				// trigger the chart invalidate so that the render loop starts even if there is no data being received
				ChartControl.InvalidateVisual();
				e.Handled = true;
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			float	myRegionY	= -1;	/// Mod - Add : Numan
			float	startY		= -1;
			float	startX		= -1;
			float	endX		= -1;
			string	myText		= "";

			// Set text to chart label color and font
			TextFormat	textFormat		= chartControl.Properties.LabelFont.ToDirectWriteTextFormat();

			// if the click is set we have a dpi converted x and y position.
			// create the rendering object in OnRender so that any brushes used are created for the renderTarget
			if (clickSet)
			{
				/// Mod by Numan
				double tickValue = Instrument.MasterInstrument.TickSize * Instrument.MasterInstrument.PointValue;

				if (barIdx < 0)
				{
					int clickPixelX 	= (int)clickPoint.X;
					int clickPixelY 	= (int)clickPoint.Y;
					double valueClick	= chartScale.GetValueByY(clickPixelY);
					barIdx 				= ChartBars.GetBarIdxByX(chartControl, clickPixelX);
					
					if (barIdx == barPrev) {
						#region * Initialise Parameters *
						clickSet 	= false;
						myStop 		= 0;
						myEntry 	= 0;
						myR1 		= 0;
						myR1_5 		= 0;	/// Mod - Add : Numan
						myR2 		= 0;
						myR2_5 		= 0;	/// Mod - Add : Numan
						myR3 		= 0;
						myR4 		= 0;	/// Mod - Add : Numan
						myR5 		= 0;	/// Mod - Add : Numan
						barPrev 	= 0;
						#endregion
					} else {
						barPrev = barIdx;
				
						if (valueClick>=(High.GetValueAt(barIdx)+Low.GetValueAt(barIdx))/2)
						{
							
							#region * Long parameter setups *
							myStop 	= Low.GetValueAt(barIdx) - (StopAdjustTicks*Instrument.MasterInstrument.TickSize);
							myEntry = High.GetValueAt(barIdx) + (EntryAdjustTicks*Instrument.MasterInstrument.TickSize);
							myRisk 	=  (myEntry - myStop) / Instrument.MasterInstrument.TickSize;
							
							
							myR1 	= myEntry + (1 * (myEntry - myStop));
							myR1_5 	= myEntry + (1.5 * (myEntry - myStop));	/// Mod - Add : Numan
							myR2 	= myEntry + (2 * (myEntry - myStop));
							myR2_5 	= myEntry + (2.5 * (myEntry - myStop));	/// Mod - Add : Numan
							myR3 	= myEntry + (3 * (myEntry - myStop));
							myR4 	= myEntry + (4 * (myEntry - myStop));	/// Mod - Add : Numan
							myR5 	= myEntry + (5 * (myEntry - myStop));	/// Mod - Add : Numan
							
							myTarget1 	=  (myR1 - myEntry) / Instrument.MasterInstrument.TickSize;
							myTarget2 	=  (myR2 - myEntry) / Instrument.MasterInstrument.TickSize;
							myTarget1_5 =  (myR1_5 - myEntry) / Instrument.MasterInstrument.TickSize;	/// Mod - Add : Numan
							myTarget2_5 =  (myR2_5 - myEntry) / Instrument.MasterInstrument.TickSize;	/// Mod - Add : Numan
							myTarget3 	=  (myR3 - myEntry) / Instrument.MasterInstrument.TickSize;
							myTarget4 	=  (myR4 - myEntry) / Instrument.MasterInstrument.TickSize;	/// Mod - Add : Numan
							myTarget5 	=  (myR5 - myEntry) / Instrument.MasterInstrument.TickSize;	/// Mod - Add : Numan
							#endregion
							
						} else {
							
							#region *Short parameter setups *
							myEntry = Low.GetValueAt(barIdx) - (EntryAdjustTicks*Instrument.MasterInstrument.TickSize);
							myStop 	= High.GetValueAt(barIdx) + (StopAdjustTicks*Instrument.MasterInstrument.TickSize);
							myRisk 	=  (myStop - myEntry) / Instrument.MasterInstrument.TickSize;
							
							myR1 	= myEntry - (1 * (myStop - myEntry));
							myR1_5 	= myEntry - (1.5 * (myStop - myEntry));	/// Mod - Add : Numan
							myR2 	= myEntry - (2 * (myStop - myEntry));
							myR2_5 	= myEntry - (2.5 * (myStop - myEntry));	/// Mod - Add : Numan
							myR3 	= myEntry - (3 * (myStop - myEntry));
							myR4 	= myEntry - (4 * (myStop - myEntry));	/// Mod - Add : Numan
							myR5 	= myEntry - (5 * (myStop - myEntry));	/// Mod - Add : Numan
							
							myTarget1 	=  (myEntry - myR1) / Instrument.MasterInstrument.TickSize;
							myTarget2 	=  (myEntry - myR2) / Instrument.MasterInstrument.TickSize;
							myTarget1_5 =  (myEntry - myR1_5) / Instrument.MasterInstrument.TickSize;	/// Mod - Add : Numan
							myTarget2_5 =  (myEntry - myR2_5) / Instrument.MasterInstrument.TickSize;	/// Mod - Add : Numan
							myTarget3 	=  (myEntry - myR3) / Instrument.MasterInstrument.TickSize;
							myTarget4 	=  (myEntry - myR4) / Instrument.MasterInstrument.TickSize;	/// Mod - Add : Numan
							myTarget5 	=  (myEntry - myR5) / Instrument.MasterInstrument.TickSize;	/// Mod - Add : Numan
							#endregion
							
						}
						
						#region * Rounding off *
						/// Rounding off >> important for NOT showing the decimals occured in the calculations of ticks. -- Mod by Numan
						myStop		= Instrument.MasterInstrument.RoundToTickSize(myStop);
						myEntry		= Instrument.MasterInstrument.RoundToTickSize(myEntry);
						myRisk		= Instrument.MasterInstrument.RoundToTickSize(myRisk);
						
						myTarget1 	= Instrument.MasterInstrument.RoundToTickSize(myTarget1);
						myTarget2 	= Instrument.MasterInstrument.RoundToTickSize(myTarget2);
						myTarget1_5 = Instrument.MasterInstrument.RoundToTickSize(myTarget1_5);
						myTarget2_5 = Instrument.MasterInstrument.RoundToTickSize(myTarget2_5);
						myTarget3 	= Instrument.MasterInstrument.RoundToTickSize(myTarget3);
						myTarget4 	= Instrument.MasterInstrument.RoundToTickSize(myTarget4);
						myTarget5 	= Instrument.MasterInstrument.RoundToTickSize(myTarget5);
						#endregion
						
						#region ***** Print - Debug *****
						if (debug)
						{
							Print("TickSize of the instrument is :" + Instrument.MasterInstrument.TickSize + ".");
							Print("There are " + GetNumberDecimalPlaces(myR1) + " decimal places in " + GetNumberDecimal(myR1));
							Print("The decimal part is " + GetNumberDecimal(myR1));
							Print("The value of the decimal part is " + GetNumberDecimal(myR1) * TickSize);
							Print("The value of the decimal part is " + GetNumberDecimal(myR1) * TickSize);
							Print("Ticks per point is " + 1/TickSize);							
						}
						#endregion
						
//						Print(GetNumberDecimal(myR1)/(exponent(10^GetNumberDecimalPlaces(myR1))));
//						myR1 	= Instrument.MasterInstrument.RoundToTickSize(myR1);
//						myR1_5 	= Instrument.MasterInstrument.RoundToTickSize(myR1_5);
//						myR2 	= Instrument.MasterInstrument.RoundToTickSize(myR2);
//						myR2_5 	= Instrument.MasterInstrument.RoundToTickSize(myR2_5);
//						myR3 	= Instrument.MasterInstrument.RoundToTickSize(myR3);
//						myR4 	= Instrument.MasterInstrument.RoundToTickSize(myR4);
//						myR5 	= Instrument.MasterInstrument.RoundToTickSize(myR5);
						
					}
				}
				
				if (myStop>0) {
					startX		= chartControl.GetXByBarIndex(ChartBars, barIdx);
					endX		= chartControl.GetXByBarIndex(ChartBars, barIdx + RulerWidth);
					
					#region * draw stop line *
					startY		= chartScale.GetYByValue(myStop);
					SharpDX.Vector2 stopStart 	= new SharpDX.Vector2(startX, startY);
					SharpDX.Vector2 stopEnd 	= new SharpDX.Vector2(endX, startY);
					SharpDX.Direct2D1.Brush stopBrushDx;
					stopBrushDx	= stopBrush.ToDxBrush(RenderTarget);
					RenderTarget.DrawLine(stopStart, stopEnd, stopBrushDx, StopWidth);  
					
					#region * Draw Risk-Text *
					myText = "-" + myRisk + ((myRisk == 1) ? " tick"	: " ticks");	/// Mod - Restructure : Numan
					if (MaxRiskTicks > 0)
					{
						myText = (myRisk > MaxRiskTicks) ? "*** No !!! " + myText + " ***" : myText;
					}

					if (ShowTicks || (myRisk > MaxRiskTicks && MaxRiskTicks > 0))
					{
						myText = myText + (ShowDetails?" [SL @" + myStop + "]":"");
						TextLayout stopLayout = new TextLayout(Globals.DirectWriteFactory, myText, textFormat, 200, textFormat.FontSize);
						RenderTarget.DrawTextLayout(stopStart, stopLayout, stopBrushDx);
						stopLayout.Dispose();
					}
					stopBrushDx.Dispose();
					#endregion

					#endregion
					
					#region * draw entry line *
					startY		= chartScale.GetYByValue(myEntry);
					SharpDX.Vector2 entryStart 	= new SharpDX.Vector2(startX, startY);
					SharpDX.Vector2 entryEnd 	= new SharpDX.Vector2(endX, startY);
					SharpDX.Direct2D1.Brush entryBrushDx;
					entryBrushDx	= entryBrush.ToDxBrush(RenderTarget);
					RenderTarget.DrawLine(entryStart, entryEnd, entryBrushDx, EntryWidth);  
					myRegionY 	= startY;
					
					// Draw area
//					Draw.Region(this, "SL_area", (int)startX, (int)endX, startY, myRegionY, null, stopBrushDx, 50);
//					Draw.Rectangle(this, "SL_rect", false, barIdx, Low[barIdx], barIdx-RulerWidth, High[barIdx], null, Brushes.Blue, 2);
					
					#region * draw Entry Text *
					if (ShowDetails)
					{
						myText = "Entry @" + myEntry + " = Risk " + myCurrency + tickValue * myRisk;
						TextLayout entryLayout = new TextLayout(Globals.DirectWriteFactory, myText, textFormat, 200, textFormat.FontSize);
						RenderTarget.DrawTextLayout(entryStart, entryLayout, entryBrushDx);
						entryLayout.Dispose();	
					}
					entryBrushDx.Dispose();
					#endregion
					
					#endregion

					#region * draw R1 line *
					startY		= chartScale.GetYByValue(myR1);
					SharpDX.Vector2 r1Start = new SharpDX.Vector2(startX, startY);
					SharpDX.Vector2 r1End 	= new SharpDX.Vector2(endX, startY);
					SharpDX.Direct2D1.Brush r1BrushDx;
					r1BrushDx	= r1Brush.ToDxBrush(RenderTarget);
					RenderTarget.DrawLine(r1Start, r1End, r1BrushDx, R1Width);  
					
					#region * Draw target-text for R1 *
					if (ShowTicks)
					{
						//myText = (myTarget1 == 1) ? "+" + myTarget1 + " tick"	: "+" + myTarget1 + " ticks";
						myText = "+" + myTarget1 + (ShowDetails? ("  :: R1 @" + myR1):" @ R1");
						TextLayout r1Layout = new TextLayout(Globals.DirectWriteFactory, myText, textFormat, 200, textFormat.FontSize);
						RenderTarget.DrawTextLayout(r1Start, r1Layout, r1BrushDx);
						r1Layout.Dispose();
					}
					r1BrushDx.Dispose();
					#endregion
					
					#endregion
					
					#region * draw R2 line *
					if (ShowR2) 
					{
						#region * Draw R1.5 Line *
						if (ShowMids) 	/// Mod - Add : Numan
						{	// draw R1.5 line
							startY		= chartScale.GetYByValue(myR1_5);
							SharpDX.Vector2 r1_5Start 	= new SharpDX.Vector2(startX, startY);
							SharpDX.Vector2 r1_5End 	= new SharpDX.Vector2(endX, startY);
							SharpDX.Direct2D1.Brush rMidBrushDx;
							rMidBrushDx	= rMidBrush.ToDxBrush(RenderTarget);
							RenderTarget.DrawLine(r1_5Start, r1_5End, rMidBrushDx, RMidWidth);  
							
							// draw R1.5 text
							if (ShowTicks) {
								myText = "          +" + myTarget1_5 + (ShowDetails? ("  :: R1.5 @" + myR1_5):" @ R1.5");
								TextLayout r1_5Layout = new TextLayout(Globals.DirectWriteFactory, myText, textFormat, 200, textFormat.FontSize - 1);
								RenderTarget.DrawTextLayout(r1_5Start, r1_5Layout, rMidBrushDx);
								r1_5Layout.Dispose();
							}
							rMidBrushDx.Dispose();
						}
						#endregion
						
						#region * Draw R2 Line *
						startY		= chartScale.GetYByValue(myR2);
						
						SharpDX.Vector2 r2Start = new SharpDX.Vector2(startX, startY);
						SharpDX.Vector2 r2End 	= new SharpDX.Vector2(endX, startY);
						SharpDX.Direct2D1.Brush r2BrushDx;
						r2BrushDx	= r2Brush.ToDxBrush(RenderTarget);
						RenderTarget.DrawLine(r2Start, r2End, r2BrushDx, R2Width);
						#endregion

						#region * Draw target-text for R2 *
						if (ShowTicks)
						{
							//myText = (myTarget2 == 1) ? "+" + myTarget2 + " tick"	: "+" + myTarget2 + " ticks";
							myText = "+" + myTarget2 + (ShowDetails? ("  :: R2 @" + myR2 + "[ Rew " + myCurrency + tickValue * myTarget2 + " ]"):" @ R2");
							TextLayout r2Layout = new TextLayout(Globals.DirectWriteFactory, myText, textFormat, 200, textFormat.FontSize);
							RenderTarget.DrawTextLayout(r2Start, r2Layout, r2BrushDx);
							r2Layout.Dispose();
						}
						#endregion
						
						r2BrushDx.Dispose();
						
					}
					#endregion

					#region * draw R3 line *
					if (ShowR3) 
					{
						#region * Draw R2.5 Line *
						if (ShowMids) 	/// Mod - Add : Numan
						{	// draw R2.5 line
							startY		= chartScale.GetYByValue(myR2_5);
							SharpDX.Vector2 r2_5Start 	= new SharpDX.Vector2(startX, startY);
							SharpDX.Vector2 r2_5End 	= new SharpDX.Vector2(endX, startY);
							SharpDX.Direct2D1.Brush rMidBrushDx;
							rMidBrushDx	= rMidBrush.ToDxBrush(RenderTarget);
							RenderTarget.DrawLine(r2_5Start, r2_5End, rMidBrushDx, RMidWidth);  
							
							// draw R2.5 text
							if(ShowTicks){
								myText = "          +" + myTarget2_5 + (ShowDetails? ("  :: R2.5 @" + myR2_5):" @ R2.5");
								TextLayout r2_5Layout = new TextLayout(Globals.DirectWriteFactory, myText, textFormat, 200, textFormat.FontSize - 1);
								RenderTarget.DrawTextLayout(r2_5Start, r2_5Layout, rMidBrushDx);
								r2_5Layout.Dispose();
							}
							rMidBrushDx.Dispose();
						}
						#endregion
						
						#region * Draw R3 Line *
						startY		= chartScale.GetYByValue(myR3);
						
						SharpDX.Vector2 r3Start = new SharpDX.Vector2(startX, startY);
						SharpDX.Vector2 r3End 	= new SharpDX.Vector2(endX, startY);
						SharpDX.Direct2D1.Brush r3BrushDx;
						r3BrushDx	= r3Brush.ToDxBrush(RenderTarget);
						RenderTarget.DrawLine(r3Start, r3End, r3BrushDx, R3Width);
						#endregion

						#region * Draw target-text for R3 *
						if (ShowTicks)
						{
							//myText = (myTarget3 == 1) ? "+" + myTarget3 + " tick"	: "+" + myTarget3 + " ticks";
							myText = "+" + myTarget3 + (ShowDetails? ("  :: R3 @" + myR3 + "[ Rew " + myCurrency + tickValue * myTarget3 + " ]"):" @ R3");
							TextLayout r3Layout = new TextLayout(Globals.DirectWriteFactory, myText, textFormat, 200, textFormat.FontSize);
							RenderTarget.DrawTextLayout(r3Start, r3Layout, r3BrushDx);
							r3Layout.Dispose();
						}
						#endregion
						r3BrushDx.Dispose();
					}
					#endregion

					#region * draw R4 line *
					if (ShowR4) 
					{
						startY		= chartScale.GetYByValue(myR4);
						
						#region * Draw R4 Line *
						SharpDX.Vector2 r4Start = new SharpDX.Vector2(startX, startY);
						SharpDX.Vector2 r4End 	= new SharpDX.Vector2(endX, startY);
						SharpDX.Direct2D1.Brush r4BrushDx;
						r4BrushDx	= r4Brush.ToDxBrush(RenderTarget);
						RenderTarget.DrawLine(r4Start, r4End, r4BrushDx, R4Width);
						#endregion

						#region * Draw target-text for R4 *
						if (ShowTicks)
						{
							//myText = (myTarget4 == 1) ? "+" + myTarget4 + " tick"	: "+" + myTarget4 + " ticks";
							myText = "+" + myTarget4 + (ShowDetails? ("  :: R4 @" + myR4 + "[ Rew " + myCurrency + tickValue * myTarget4 + " ]"):" @ R4");
							TextLayout r4Layout = new TextLayout(Globals.DirectWriteFactory, myText, textFormat, 200, textFormat.FontSize);
							RenderTarget.DrawTextLayout(r4Start, r4Layout, r4BrushDx);
							r4Layout.Dispose();
						}
						#endregion
						
						r4BrushDx.Dispose();
					}
					#endregion

					#region * draw R5 line *
					if (ShowR5) 
					{
						startY		= chartScale.GetYByValue(myR5);
						
						#region * Draw R5 Line *
						SharpDX.Vector2 r5Start = new SharpDX.Vector2(startX, startY);
						SharpDX.Vector2 r5End 	= new SharpDX.Vector2(endX, startY);
						SharpDX.Direct2D1.Brush r5BrushDx;
						r5BrushDx	= r5Brush.ToDxBrush(RenderTarget);
						RenderTarget.DrawLine(r5Start, r5End, r5BrushDx, R5Width);
						#endregion

						#region * Draw target-text for R5 *
						if (ShowTicks)
						{
							//myText = (myTarget5 == 1) ? "+" + myTarget5 + " tick"	: "+" + myTarget5 + " ticks";
							myText = "+" + myTarget5 + (ShowDetails? ("  :: R5 @" + myR5 + "[ Rew " + myCurrency + tickValue * myTarget5 + " ]"):" @ R5");
							TextLayout r5Layout = new TextLayout(Globals.DirectWriteFactory, myText, textFormat, 200, textFormat.FontSize);
							RenderTarget.DrawTextLayout(r5Start, r5Layout, r5BrushDx);
							r5Layout.Dispose();
						}
						#endregion
						
						r5BrushDx.Dispose();
					}
					#endregion

				}
			}
			textFormat.Dispose();
		}

		#region Properties

		#region ** 1. Settings **				
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Rular Width #Bars", Description="Width of displayed lines (in # of bars)", Order=1, GroupName="1. Settings")]
		public int RulerWidth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Stop adjust Ticks", Description="Number of extra ticks between Bar and Stop Line", Order=2, GroupName="1. Settings")]
		public int StopAdjustTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Entry adjust Ticks", Description="Number of extra ticks between Bar and Entry-line", Order=3, GroupName="1. Settings")]
		public int EntryAdjustTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Max. Risk Ticks", Description="Max. acceptable risk in ticks (display warning if exceeded)", Order=4, GroupName="1. Settings")]
		public int MaxRiskTicks
		{ get; set; }

		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]
		[Display(Name="Show Ticks", Description="Show Ticks", Order=10, GroupName="1. Settings")]
		public bool ShowTicks
		{
		   get
		   {
		      return showTicks;
		   }
		   set
		   {
		      if (value == false)
		      {
		         showDetails = false;
		      }
		      showTicks = value;
		    }
		}

		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]
		[Display(Name="Show Details", Description="Show Details", Order=11, GroupName="1. Settings")]
		public bool ShowDetails
		{
		   get
		   {
		      return showDetails;
		   }
		   set
		   {
		      if (value == true)
		      {
		         showTicks = true;
		      }
		      showDetails = value;
		   }
		}
		#endregion
		
		#region ** 2. Setting On/Offs **
		[NinjaScriptProperty]
		[Display(Name="Show 2x Rewards", Description="Show R2-line", Order=5, GroupName="2. Setting On/Offs")]
		public bool ShowR2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show 3x Rewards", Description="Show R3-line", Order=6, GroupName="2. Setting On/Offs")]
		public bool ShowR3
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show 4x Rewards", Description="Show R4-line", Order=7, GroupName="2. Setting On/Offs")]
		public bool ShowR4
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show 5x Rewards", Description="Show R5-line", Order=8, GroupName="2. Setting On/Offs")]
		public bool ShowR5
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show Mid Rewards", Description="Show Mid-lines", Order=9, GroupName="2. Setting On/Offs")]
		public bool ShowMids
		{ get; set; }
		#endregion
		
		#region ** 3. Line Depth **
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Width Stop", Description="Width of Stop line", Order=20, GroupName="3. Line Depth")]
		public int StopWidth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Width Entry", Description="Width of Entry-line", Order=21, GroupName="3. Line Depth")]
		public int EntryWidth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Width 1x Rewards", Description="Width of 1x Rewards", Order=22, GroupName="3. Line Depth")]
		public int R1Width
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Width 2x Rewards", Description="Width of 2x Rewards", Order=23, GroupName="3. Line Depth")]
		public int R2Width
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Width 3x Rewards", Description="Width of 3x Rewards", Order=24, GroupName="3. Line Depth")]
		public int R3Width
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Width 4x Rewards", Description="Width of 4x Rewards", Order=25, GroupName="3. Line Depth")]
		public int R4Width
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Width 5x Rewards", Description="Width of 5x Rewards", Order=26, GroupName="3. Line Depth")]
		public int R5Width
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Width X.5x Rewards", Description="Width of Mid Rewards", Order=27, GroupName="3. Line Depth")]
		public int RMidWidth
		{ get; set; }
		
		#endregion		
		
		#region ** 4. Line Colors **
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Stop", Description="Stop level", Order=12, GroupName = "4. Line Colors")]
		public System.Windows.Media.Brush StopBrush
		{
			get { return stopBrush; }
			set { stopBrush = value; }
		}

		[Browsable(false)]
		public string StopBrushSerialize
		{
			get { return Serialize.BrushToString(StopBrush); }
			set { StopBrush = Serialize.StringToBrush(value); }
		}


		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Entry", Description="Entry level", Order=13, GroupName = "4. Line Colors")]
		public System.Windows.Media.Brush EntryBrush
		{
			get { return entryBrush; }
			set { entryBrush = value; }
		}

		[Browsable(false)]
		public string EntryBrushSerialize
		{
			get { return Serialize.BrushToString(EntryBrush); }
			set { EntryBrush = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color 1x Reward", Description="1x Reward", Order=14, GroupName = "4. Line Colors")]
		public System.Windows.Media.Brush R1Brush
		{
			get { return r1Brush; }
			set { r1Brush = value; }
		}

		[Browsable(false)]
		public string r1BrushSerialize
		{
			get { return Serialize.BrushToString(r1Brush); }
			set { r1Brush = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color 2x Rewards", Description="2x Rewards", Order=15, GroupName = "4. Line Colors")]
		public System.Windows.Media.Brush R2Brush
		{
			get { return r2Brush; }
			set { r2Brush = value; }
		}

		[Browsable(false)]
		public string r2BrushSerialize
		{
			get { return Serialize.BrushToString(r2Brush); }
			set { r2Brush = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color 3x Rewards", Description="3x Rewards", Order=16, GroupName = "4. Line Colors")]
		public System.Windows.Media.Brush R3Brush
		{
			get { return r3Brush; }
			set { r3Brush = value; }
		}

		[Browsable(false)]
		public string r3BrushSerialize
		{
			get { return Serialize.BrushToString(r3Brush); }
			set { r3Brush = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color 4x Rewards", Description="4x Rewards", Order=17, GroupName = "4. Line Colors")]
		public System.Windows.Media.Brush R4Brush
		{
			get { return r4Brush; }
			set { r4Brush = value; }
		}

		[Browsable(false)]
		public string r4BrushSerialize
		{
			get { return Serialize.BrushToString(r4Brush); }
			set { r4Brush = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color 5x Rewards", Description="5x Rewards", Order=18, GroupName = "4. Line Colors")]
		public System.Windows.Media.Brush R5Brush
		{
			get { return r5Brush; }
			set { r5Brush = value; }
		}

		[Browsable(false)]
		public string r5BrushSerialize
		{
			get { return Serialize.BrushToString(r5Brush); }
			set { r5Brush = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Color Mid Reward", Description="1.5x or 2.5x Rewards", Order=19, GroupName = "4. Line Colors")]
		public System.Windows.Media.Brush RMidBrush
		{
			get { return rMidBrush; }
			set { rMidBrush = value; }
		}

		[Browsable(false)]
		public string rMidBrushSerialize
		{
			get { return Serialize.BrushToString(rMidBrush); }
			set { rMidBrush = Serialize.StringToBrush(value); }
		}

		#endregion
		
		#region ** D E B U G **
		[NinjaScriptProperty]
		[Display(Name="Show debug reports", Description="Show debug prints", Order=9, GroupName="Debug")]
		public bool debug
		{ get; set; }
		#endregion

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RKRiskRuler[] cacheRKRiskRuler;
		public RKRiskRuler RKRiskRuler(int rulerWidth, int stopAdjustTicks, int entryAdjustTicks, int maxRiskTicks, bool showTicks, bool showDetails, bool showR2, bool showR3, bool showR4, bool showR5, bool showMids, int stopWidth, int entryWidth, int r1Width, int r2Width, int r3Width, int r4Width, int r5Width, int rMidWidth, System.Windows.Media.Brush stopBrush, System.Windows.Media.Brush entryBrush, System.Windows.Media.Brush r1Brush, System.Windows.Media.Brush r2Brush, System.Windows.Media.Brush r3Brush, System.Windows.Media.Brush r4Brush, System.Windows.Media.Brush r5Brush, System.Windows.Media.Brush rMidBrush, bool debug)
		{
			return RKRiskRuler(Input, rulerWidth, stopAdjustTicks, entryAdjustTicks, maxRiskTicks, showTicks, showDetails, showR2, showR3, showR4, showR5, showMids, stopWidth, entryWidth, r1Width, r2Width, r3Width, r4Width, r5Width, rMidWidth, stopBrush, entryBrush, r1Brush, r2Brush, r3Brush, r4Brush, r5Brush, rMidBrush, debug);
		}

		public RKRiskRuler RKRiskRuler(ISeries<double> input, int rulerWidth, int stopAdjustTicks, int entryAdjustTicks, int maxRiskTicks, bool showTicks, bool showDetails, bool showR2, bool showR3, bool showR4, bool showR5, bool showMids, int stopWidth, int entryWidth, int r1Width, int r2Width, int r3Width, int r4Width, int r5Width, int rMidWidth, System.Windows.Media.Brush stopBrush, System.Windows.Media.Brush entryBrush, System.Windows.Media.Brush r1Brush, System.Windows.Media.Brush r2Brush, System.Windows.Media.Brush r3Brush, System.Windows.Media.Brush r4Brush, System.Windows.Media.Brush r5Brush, System.Windows.Media.Brush rMidBrush, bool debug)
		{
			if (cacheRKRiskRuler != null)
				for (int idx = 0; idx < cacheRKRiskRuler.Length; idx++)
					if (cacheRKRiskRuler[idx] != null && cacheRKRiskRuler[idx].RulerWidth == rulerWidth && cacheRKRiskRuler[idx].StopAdjustTicks == stopAdjustTicks && cacheRKRiskRuler[idx].EntryAdjustTicks == entryAdjustTicks && cacheRKRiskRuler[idx].MaxRiskTicks == maxRiskTicks && cacheRKRiskRuler[idx].ShowTicks == showTicks && cacheRKRiskRuler[idx].ShowDetails == showDetails && cacheRKRiskRuler[idx].ShowR2 == showR2 && cacheRKRiskRuler[idx].ShowR3 == showR3 && cacheRKRiskRuler[idx].ShowR4 == showR4 && cacheRKRiskRuler[idx].ShowR5 == showR5 && cacheRKRiskRuler[idx].ShowMids == showMids && cacheRKRiskRuler[idx].StopWidth == stopWidth && cacheRKRiskRuler[idx].EntryWidth == entryWidth && cacheRKRiskRuler[idx].R1Width == r1Width && cacheRKRiskRuler[idx].R2Width == r2Width && cacheRKRiskRuler[idx].R3Width == r3Width && cacheRKRiskRuler[idx].R4Width == r4Width && cacheRKRiskRuler[idx].R5Width == r5Width && cacheRKRiskRuler[idx].RMidWidth == rMidWidth && cacheRKRiskRuler[idx].StopBrush == stopBrush && cacheRKRiskRuler[idx].EntryBrush == entryBrush && cacheRKRiskRuler[idx].R1Brush == r1Brush && cacheRKRiskRuler[idx].R2Brush == r2Brush && cacheRKRiskRuler[idx].R3Brush == r3Brush && cacheRKRiskRuler[idx].R4Brush == r4Brush && cacheRKRiskRuler[idx].R5Brush == r5Brush && cacheRKRiskRuler[idx].RMidBrush == rMidBrush && cacheRKRiskRuler[idx].debug == debug && cacheRKRiskRuler[idx].EqualsInput(input))
						return cacheRKRiskRuler[idx];
			return CacheIndicator<RKRiskRuler>(new RKRiskRuler(){ RulerWidth = rulerWidth, StopAdjustTicks = stopAdjustTicks, EntryAdjustTicks = entryAdjustTicks, MaxRiskTicks = maxRiskTicks, ShowTicks = showTicks, ShowDetails = showDetails, ShowR2 = showR2, ShowR3 = showR3, ShowR4 = showR4, ShowR5 = showR5, ShowMids = showMids, StopWidth = stopWidth, EntryWidth = entryWidth, R1Width = r1Width, R2Width = r2Width, R3Width = r3Width, R4Width = r4Width, R5Width = r5Width, RMidWidth = rMidWidth, StopBrush = stopBrush, EntryBrush = entryBrush, R1Brush = r1Brush, R2Brush = r2Brush, R3Brush = r3Brush, R4Brush = r4Brush, R5Brush = r5Brush, RMidBrush = rMidBrush, debug = debug }, input, ref cacheRKRiskRuler);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RKRiskRuler RKRiskRuler(int rulerWidth, int stopAdjustTicks, int entryAdjustTicks, int maxRiskTicks, bool showTicks, bool showDetails, bool showR2, bool showR3, bool showR4, bool showR5, bool showMids, int stopWidth, int entryWidth, int r1Width, int r2Width, int r3Width, int r4Width, int r5Width, int rMidWidth, System.Windows.Media.Brush stopBrush, System.Windows.Media.Brush entryBrush, System.Windows.Media.Brush r1Brush, System.Windows.Media.Brush r2Brush, System.Windows.Media.Brush r3Brush, System.Windows.Media.Brush r4Brush, System.Windows.Media.Brush r5Brush, System.Windows.Media.Brush rMidBrush, bool debug)
		{
			return indicator.RKRiskRuler(Input, rulerWidth, stopAdjustTicks, entryAdjustTicks, maxRiskTicks, showTicks, showDetails, showR2, showR3, showR4, showR5, showMids, stopWidth, entryWidth, r1Width, r2Width, r3Width, r4Width, r5Width, rMidWidth, stopBrush, entryBrush, r1Brush, r2Brush, r3Brush, r4Brush, r5Brush, rMidBrush, debug);
		}

		public Indicators.RKRiskRuler RKRiskRuler(ISeries<double> input , int rulerWidth, int stopAdjustTicks, int entryAdjustTicks, int maxRiskTicks, bool showTicks, bool showDetails, bool showR2, bool showR3, bool showR4, bool showR5, bool showMids, int stopWidth, int entryWidth, int r1Width, int r2Width, int r3Width, int r4Width, int r5Width, int rMidWidth, System.Windows.Media.Brush stopBrush, System.Windows.Media.Brush entryBrush, System.Windows.Media.Brush r1Brush, System.Windows.Media.Brush r2Brush, System.Windows.Media.Brush r3Brush, System.Windows.Media.Brush r4Brush, System.Windows.Media.Brush r5Brush, System.Windows.Media.Brush rMidBrush, bool debug)
		{
			return indicator.RKRiskRuler(input, rulerWidth, stopAdjustTicks, entryAdjustTicks, maxRiskTicks, showTicks, showDetails, showR2, showR3, showR4, showR5, showMids, stopWidth, entryWidth, r1Width, r2Width, r3Width, r4Width, r5Width, rMidWidth, stopBrush, entryBrush, r1Brush, r2Brush, r3Brush, r4Brush, r5Brush, rMidBrush, debug);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RKRiskRuler RKRiskRuler(int rulerWidth, int stopAdjustTicks, int entryAdjustTicks, int maxRiskTicks, bool showTicks, bool showDetails, bool showR2, bool showR3, bool showR4, bool showR5, bool showMids, int stopWidth, int entryWidth, int r1Width, int r2Width, int r3Width, int r4Width, int r5Width, int rMidWidth, System.Windows.Media.Brush stopBrush, System.Windows.Media.Brush entryBrush, System.Windows.Media.Brush r1Brush, System.Windows.Media.Brush r2Brush, System.Windows.Media.Brush r3Brush, System.Windows.Media.Brush r4Brush, System.Windows.Media.Brush r5Brush, System.Windows.Media.Brush rMidBrush, bool debug)
		{
			return indicator.RKRiskRuler(Input, rulerWidth, stopAdjustTicks, entryAdjustTicks, maxRiskTicks, showTicks, showDetails, showR2, showR3, showR4, showR5, showMids, stopWidth, entryWidth, r1Width, r2Width, r3Width, r4Width, r5Width, rMidWidth, stopBrush, entryBrush, r1Brush, r2Brush, r3Brush, r4Brush, r5Brush, rMidBrush, debug);
		}

		public Indicators.RKRiskRuler RKRiskRuler(ISeries<double> input , int rulerWidth, int stopAdjustTicks, int entryAdjustTicks, int maxRiskTicks, bool showTicks, bool showDetails, bool showR2, bool showR3, bool showR4, bool showR5, bool showMids, int stopWidth, int entryWidth, int r1Width, int r2Width, int r3Width, int r4Width, int r5Width, int rMidWidth, System.Windows.Media.Brush stopBrush, System.Windows.Media.Brush entryBrush, System.Windows.Media.Brush r1Brush, System.Windows.Media.Brush r2Brush, System.Windows.Media.Brush r3Brush, System.Windows.Media.Brush r4Brush, System.Windows.Media.Brush r5Brush, System.Windows.Media.Brush rMidBrush, bool debug)
		{
			return indicator.RKRiskRuler(input, rulerWidth, stopAdjustTicks, entryAdjustTicks, maxRiskTicks, showTicks, showDetails, showR2, showR3, showR4, showR5, showMids, stopWidth, entryWidth, r1Width, r2Width, r3Width, r4Width, r5Width, rMidWidth, stopBrush, entryBrush, r1Brush, r2Brush, r3Brush, r4Brush, r5Brush, rMidBrush, debug);
		}
	}
}

#endregion
