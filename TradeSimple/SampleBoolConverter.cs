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
	public class SampleBoolConverter : Indicator, ICustomTypeDescriptor//<<<<<<<<<<<<<<<<<<<<ADD THIS , ICustomTypeDescriptor
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SampleBoolConverter";
				Calculate									= Calculate.OnBarClose;
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
				
				SwitchBool									= true;
				
				Bool1										= false;
				Bool2										= false;
				Bool3										= false;
				
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}
		
		
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)] ///<<<<<<<<<<<<<<<<<<Add RefreshProperties to your main one.
		[Display(Name="SwitchBool", Order=1, GroupName="Parameters")]
		public bool SwitchBool
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="Bool1", Order=2, GroupName="Parameters")]
		public bool Bool1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Bool2", Order=3, GroupName="Parameters")]
		public bool Bool2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Bool3", Order=4, GroupName="Parameters")]
		public bool Bool3
		{ get; set; }
		
		
		
		
		private void ModifyProperties(PropertyDescriptorCollection col)
        {
			if (SwitchBool)
			{
				col.Remove(col.Find("Bool1", true));
			}
			
			if (!SwitchBool)
			{
				col.Remove(col.Find("Bool2", true));
				col.Remove(col.Find("Bool3", true));
			}
				
		}
		
		///Copy and Paste this
		#region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(GetType());
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(GetType());
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(GetType());
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(GetType());
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(GetType());
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(GetType());
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(GetType(), editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(GetType(), attributes);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(GetType());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection orig = TypeDescriptor.GetProperties(GetType(), attributes);
            PropertyDescriptor[] arr = new PropertyDescriptor[orig.Count];
            orig.CopyTo(arr, 0);
            PropertyDescriptorCollection col = new PropertyDescriptorCollection(arr);

            ModifyProperties(col);
            return col;

        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(GetType());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeSimple.SampleBoolConverter[] cacheSampleBoolConverter;
		public TradeSimple.SampleBoolConverter SampleBoolConverter(bool switchBool, bool bool1, bool bool2, bool bool3)
		{
			return SampleBoolConverter(Input, switchBool, bool1, bool2, bool3);
		}

		public TradeSimple.SampleBoolConverter SampleBoolConverter(ISeries<double> input, bool switchBool, bool bool1, bool bool2, bool bool3)
		{
			if (cacheSampleBoolConverter != null)
				for (int idx = 0; idx < cacheSampleBoolConverter.Length; idx++)
					if (cacheSampleBoolConverter[idx] != null && cacheSampleBoolConverter[idx].SwitchBool == switchBool && cacheSampleBoolConverter[idx].Bool1 == bool1 && cacheSampleBoolConverter[idx].Bool2 == bool2 && cacheSampleBoolConverter[idx].Bool3 == bool3 && cacheSampleBoolConverter[idx].EqualsInput(input))
						return cacheSampleBoolConverter[idx];
			return CacheIndicator<TradeSimple.SampleBoolConverter>(new TradeSimple.SampleBoolConverter(){ SwitchBool = switchBool, Bool1 = bool1, Bool2 = bool2, Bool3 = bool3 }, input, ref cacheSampleBoolConverter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSimple.SampleBoolConverter SampleBoolConverter(bool switchBool, bool bool1, bool bool2, bool bool3)
		{
			return indicator.SampleBoolConverter(Input, switchBool, bool1, bool2, bool3);
		}

		public Indicators.TradeSimple.SampleBoolConverter SampleBoolConverter(ISeries<double> input , bool switchBool, bool bool1, bool bool2, bool bool3)
		{
			return indicator.SampleBoolConverter(input, switchBool, bool1, bool2, bool3);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSimple.SampleBoolConverter SampleBoolConverter(bool switchBool, bool bool1, bool bool2, bool bool3)
		{
			return indicator.SampleBoolConverter(Input, switchBool, bool1, bool2, bool3);
		}

		public Indicators.TradeSimple.SampleBoolConverter SampleBoolConverter(ISeries<double> input , bool switchBool, bool bool1, bool bool2, bool bool3)
		{
			return indicator.SampleBoolConverter(input, switchBool, bool1, bool2, bool3);
		}
	}
}

#endregion
