/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
using NamaadDB.Entity;

namespace NamaadDB.entity
{
	public class FieldDescr
	{
		private int cid;
		private string name;
		private int type;
		private bool notNull;
		private string defaultValue;
		private bool pk;

		public string DefaultValue
		{
			get { return defaultValue; }
			set
			{
				if (string.IsNullOrWhiteSpace(value) && type != TableField.TYPE_STRING)
					defaultValue = null;
				else
					defaultValue = value;
			}
		}

		public int getCid()
		{
			return cid;
		}
		public void setCid(int cid)
		{
			this.cid = cid;
		}
		public string getName()
		{
			return name;
		}
		public void setName(string name)
		{
			this.name = name;
		}
		public int getType()
		{
			return type;
		}
		public void setType(int type)
		{
			this.type = type;
		}
		public bool isNotNull()
		{
			return notNull;
		}
		public void setNotNull(bool notNull)
		{
			this.notNull = notNull;
		}
		public string getDefaultValue()
		{
			return DefaultValue;
		}
		public void setDefaultValue(string defaultValue)
		{
			this.DefaultValue = defaultValue;
		}
		public bool isPk()
		{
			return pk;
		}
		public void setPk(bool pk)
		{
			this.pk = pk;
		}
	}
}