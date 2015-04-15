/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.OS;
using Java.Interop;
using NamaadMobile.Util;
using System;
using Object = Java.Lang.Object;
#endregion
namespace NamaadMobile.Entity
{
	/// <summary>
	///  Holds informations about a single field of a table
	/// </summary>
	public class TableField : Object, IParcelable
	{
		#region Defins
		public const int TYPE_STRING = 0;
		public const int TYPE_INTEGER = 1;
		public const int TYPE_FLOAT = 2;
		public const int TYPE_DATE = 3;
		public const int TYPE_TIME = 4;
		public const int TYPE_DATETIME = 5;
		public const int TYPE_BOOLEAN = 6;
		public const int TYPE_PHONENO = 7;
		private string name;
		private int type;
		private string value;
		private bool notNull;
		private bool primaryKey;
		private string defaultValue;
		private string foreignKey;   // on the form tableName(fieldInTable)
		private bool updateable = true;
		private string hint = null;
		private string displayName = null;
		#endregion
		/// <summary>
		/// Determines whether this instance is updateable.
		/// </summary>
		/// <returns>If this return true the field is updateable in the update view </returns>
		public bool isUpdateable()
		{
			return updateable;
		}
		/// <summary>
		/// Set this to false if you want to exclude the field from the update view
		/// Default value = true		
		/// </summary>
		/// <param name="updateable">if set to <c>true</c> [updateable].</param>
		public void setUpdateable(bool updateable)
		{
			this.updateable = updateable;
		}
		/// <summary>
		/// A hint / description of the field shown Edit fields
		/// </summary>
		/// <returns>the hint</returns>
		public string getHint()
		{
			return hint;
		}
		/// <summary>
		/// Add a Hint / description to a field. Un edit it is used as a hint
		/// </summary>
		/// <param name="hint">The hint.</param>
		public void setHint(string hint)
		{
			this.hint = hint;
		}
		/// <summary>
		/// Get the display name of a field. If not set by setDisplayName field name
		/// will be returned		
		/// </summary>
		public string getDisplayName()
		{
			if (displayName == null)
				return name;
			else
				return displayName;
		}
		/// <summary>
		/// Use this to change the display name in the update view
		/// </summary>
		/// <param name="displayName">The display name.</param>
		public void setDisplayName(string displayName)
		{
			this.displayName = displayName;
		}
		/// <summary>
		/// Retrieves the name of the field
		/// </summary>
		/// <returns></returns>
		public string getName()
		{
			return name;
		}
		/// <summary>
		/// Set the name of the field
		/// </summary>
		/// <param name="name">The name.</param>
		public void setName(string name)
		{
			this.name = name;
		}
		/// <summary>
		/// Get the data type
		/// </summary>
		/// <returns></returns>
		public int getType()
		{
			return type;
		}
		/// <summary>
		/// Set data type, possible data types: TYPE_STRING, TYPE_INTEGER, TYPE_FLOAT,
		/// TYPE_DATE, TYPE_TIME, TYPE_DATETIME, TYPE_BOOLEAN 		/// </summary>
		/// <param name="type">The type.</param>
		public void setType(int type)
		{
			this.type = type;
		}
		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <returns>a String with the value</returns>
		public string getValue()
		{
			if (value == null)
				return "";
			else
				return value;
		}
		/// <summary>
		/// Sets the value (as a String).
		/// </summary>
		/// <param name="value">The value.</param>
		public void setValue(string value)
		{
			this.value = value;
		}
		/// <summary>
		/// Gets the not null.
		/// </summary>
		/// <returns>true if the field is not null</returns>
		public bool getNotNull()
		{
			return notNull;
		}
		/// <summary>
		/// Set the field as not null
		/// </summary>
		/// <param name="notNull">if set to <c>true</c> [not null].</param>
		public void setNotNull(bool notNull)
		{
			this.notNull = notNull;
		}
		/// <summary>
		/// Gets the primary key.
		/// </summary>
		/// <returns>true if the field is the (part of) the primary key</returns>
		public bool getPrimaryKey()
		{
			return primaryKey;
		}
		/// <summary>
		/// Set the field as the (part of) the primary key
		/// </summary>
		/// <param name="primaryKey">if set to <c>true</c> [primary key].</param>
		public void setPrimaryKey(bool primaryKey)
		{
			this.primaryKey = primaryKey;
		}
		/// <summary>
		/// Get the fields default value
		/// </summary>
		/// <returns></returns>
		public string getDefaultValue()
		{
			return defaultValue;
		}
		/// <summary>
		/// Set the fields default value
		/// </summary>
		/// <param name="defaultValue">The default value.</param>
		public void setDefaultValue(string defaultValue)
		{
			this.defaultValue = defaultValue;
		}
		/// <summary>
		/// Gets the foreign key.
		/// </summary>
		/// <returns>a String that defines the fields foreign key</returns>
		public string getForeignKey()
		{
			return foreignKey;
		}
		/// <summary>
		/// Define the foreign key for the field
		/// Currently stores the sql needed to select the keys / codes
		/// Not quite sure if this is the right way to do it 	
		/// </summary>
		/// <param name="foreignKey">The foreign key.</param>
		public void setForeignKey(string foreignKey)
		{
			this.foreignKey = foreignKey;
			//Utils.logD("FK set to: " + foreignKey);
		}

		public TableField()
		{
		}
		private TableField(Parcel source)
		{
			readFromParcel(source);
		}
		private void readFromParcel(Parcel source)
		{
			name = source.ReadString();
			type = source.ReadInt();
			value = source.ReadString();
			bool[] boolArr = new bool[3];
			try
			{
				source.ReadBooleanArray(boolArr);
			}
			catch (Exception e)
			{
				ExceptionHandler.printStackTrace(e, true);
			}
			notNull = boolArr[0];
			primaryKey = boolArr[1];
			updateable = boolArr[2];
			defaultValue = source.ReadString();
			foreignKey = source.ReadString();
			hint = source.ReadString();
			displayName = source.ReadString();
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
			dest.WriteString(name);
			dest.WriteInt(type);
			dest.WriteString(value);
			dest.WriteBooleanArray(new bool[3] { notNull, primaryKey, updateable });
			dest.WriteString(defaultValue);
			dest.WriteString(foreignKey);
			dest.WriteString(hint);
			dest.WriteString(displayName);
		}
		#endregion

		[ExportField("CREATOR")]
		public static TableFieldCreator InititalizeCreator()
		{
			return new TableFieldCreator();
		}
		public class TableFieldCreator : Object, IParcelableCreator
		{
			public Object CreateFromParcel(Parcel source)
			{
				return (new TableField(source));
			}

			public Object[] NewArray(int size)
			{
				return new Object[size];
			}

		}

		public override string ToString()
		{
			base.ToString();
			return getValue() + " ";
		}
	}
}