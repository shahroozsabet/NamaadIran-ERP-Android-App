/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
using Android.OS;
using Java.Interop;
using System.Text;
using Object = Java.Lang.Object;

namespace NamaadDB.Entity
{
	public class Record : Object, IParcelable
	{
		private AField[] fields;
		private string[] fieldNames;

		public Record()
		{
		}
		public Record(Parcel source)
		{
			readFromParcel(source);
		}
		private void readFromParcel(Parcel source)
		{
			source.ReadParcelableArray(new AField().Class.ClassLoader);
			source.ReadStringArray(fieldNames);
		}
		public string[] getFieldNames()
		{
			return fieldNames;
		}

		public void setFieldNames(string[] fieldNames)
		{
			this.fieldNames = fieldNames;
		}

		public void setFields(AField[] fields)
		{
			this.fields = fields;
		}

		public AField[] getFields()
		{
			return fields;
		}
		public override string ToString()
		{
			base.ToString();
			StringBuilder sb = new StringBuilder();
			foreach (AField aF in fields)
				sb.Append(aF + " ");//TODO: Got a SIGSEGV Runtime error:
			//04-19 18:12:10.821 E/mono-rt ( 1372):   at <unknown> <0xffffffff>
			//04-19 18:12:10.830 E/mono-rt ( 1372):   at (wrapper managed-to-native) object.wrapper_native_0x40731d09 (intptr,intptr,intptr,intptr) <IL 0x00028, 0xffffffff>
			//...
			//at NamaadDB.Entity.AField.ToString () [0x00001] in c:\Users\shsabet\Documents\Visual Studio 2012\Projects\NamaadMobile\Entity\AField.cs:56
			//04-19 18:12:10.840 E/mono-rt ( 1372):   at string.Concat (object,object) <IL 0x00007, 0x0006f>
			//04-19 18:12:10.840 E/mono-rt ( 1372):   at NamaadDB.Entity.Record.ToString () [0x00020] in c:\Users\shsabet\Documents\Visual Studio 2012\Projects\NamaadMobile\Entity\Record.cs:59
			//...
			//04-19 18:12:10.840 E/mono-rt ( 1372):   at (wrapper native-to-managed) object.9304470b-95dc-4dd0-86b5-6bd13fa61e93 (intptr,intptr) <IL 0x00024, 0xffffffff>
			//String sb = "";
			//foreach (AField aF in fields)
			//	sb = aF + " " + sb;
			return sb.ToString();
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
			dest.WriteParcelableArray(fields, 0);
			dest.WriteStringArray(fieldNames);
		}
		#endregion

		[ExportField("CREATOR")]
		public static RecordCreator InititalizeCreator()
		{
			return new RecordCreator();
		}
		public class RecordCreator : Object, IParcelableCreator
		{
			public Object CreateFromParcel(Parcel source)
			{
				return (new Record(source));
			}

			public Object[] NewArray(int size)
			{
				return new Object[size];
			}

		}
	}
}