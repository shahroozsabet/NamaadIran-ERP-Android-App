/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */

using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace NamaadDB.Entity
{
	public class WebGoodSalPrice : Object, IParcelable
	{
		public int PriceType { get; set; }
		public string ItemCode { get; set; }
		public string FarsiDesc { get; set; }
		public int PriceID { get; set; }
		public double Price { get; set; }
		public string Unit { get; set; }
		public double Cost1 { get; set; }
		public double Quantity { get; set; }

		public override string ToString()
		{
			base.ToString();
			return PriceType + " " + ItemCode + " " + FarsiDesc + " " + PriceID + " " + Price + " " + Unit + " " + Cost1 + " " + Quantity;
		}

		#region IParcelable implementation
		/// <summary>
		/// //ToDo: unimplemented yet, but is not important.
		/// Describe the kinds of special objects contained in this Parcelable's marshalled representation.
		/// </summary>
		/// <returns>A bitmask indicating the set of special object types marshalled by the Parcelable.</returns>
		public int DescribeContents()
		{
			return 0;
		}
		/// <summary>
		/// Flatten this object in to a Parcel.
		/// </summary>
		/// <param name="dest">The Parcel in which the object should be written.</param>
		/// <param name="flags">Additional flags about how the object should be written. May be 0 or PARCELABLE_WRITE_RETURN_VALUE.</param>
		public void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
		{
			dest.WriteInt(PriceType);
			dest.WriteInt(PriceID);
			dest.WriteString(ItemCode);
			dest.WriteString(FarsiDesc);
			dest.WriteString(Unit);
			dest.WriteDouble(Price);
			dest.WriteDouble(Cost1);
			dest.WriteDouble(Quantity);
		}
		#endregion

		public WebGoodSalPrice()
		{
		}

		private WebGoodSalPrice(Parcel source)
		{
			readFromParcel(source);
		}

		private void readFromParcel(Parcel source)
		{
			PriceType = source.ReadInt();
			PriceID = source.ReadInt();
			ItemCode = source.ReadString();
			FarsiDesc = source.ReadString();
			Unit = source.ReadString();
			Price = source.ReadDouble();
			Cost1 = source.ReadDouble();
			Quantity = source.ReadDouble();
		}

		[ExportField("CREATOR")]
		public static WebGoodSalPriceCreator InititalizeCreator()
		{
			return new WebGoodSalPriceCreator();
		}
		public class WebGoodSalPriceCreator : Object, IParcelableCreator
		{
			public Object CreateFromParcel(Parcel source)
			{
				return (new WebGoodSalPrice(source));
			}

			public Object[] NewArray(int size)
			{
				return new Object[size];
			}

		}
	}
}