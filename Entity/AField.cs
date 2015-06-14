/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.OS;
using Java.Interop;
using System;
using Object = Java.Lang.Object;
#endregion
namespace NamaadMobile.Entity
{
    public class AField : Object, IParcelable
    {
        private string fieldData;
        private FieldType fieldType;

        public AField()
        {

        }
        public AField(Parcel source)
        {
            readFromParcel(source);
        }
        private void readFromParcel(Parcel source)
        {
            fieldData = source.ReadString();
            try
            {
                fieldType = (FieldType)Enum.Parse(typeof(FieldType), source.ReadString(), true);
            }
            catch (Exception)
            {
                fieldType = FieldType.NULL;
            }
        }

        public enum FieldType
        {
            NULL,
            INTEGER,
            REAL,
            TEXT,
            BLOB,
            UNRESOLVED
        }

        public override string ToString()
        {
            base.ToString();
            return getFieldData();//"Type = " + fieldType + " Data = " + fieldData;
        }

        public string getFieldData()
        {
            return fieldData;
        }
        public void setFieldData(string fieldData)
        {
            this.fieldData = fieldData;
        }
        public FieldType getFieldType()
        {
            return fieldType;
        }
        public void setFieldType(FieldType fieldType)
        {
            this.fieldType = fieldType;
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
            dest.WriteString(fieldData);
            dest.WriteString(fieldType.ToString());
        }
        #endregion

        [ExportField("CREATOR")]
        public static AFieldCreator InititalizeCreator()
        {
            return new AFieldCreator();
        }
        public class AFieldCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return (new AField(source));
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }

        }
    }
}