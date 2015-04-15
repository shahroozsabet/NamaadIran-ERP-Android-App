/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
namespace NamaadMobile.entity
{
	public class ForeignKeyHolder
	{
		private string[] ids;
		private string[] texts;

		/**
		 * Retrieve the foreign key values
		 * @return a String [] representation of the foreign keys
		 */
		public string[] getIds()
		{
			return ids;
		}
		/**
		 * Set the foreign key values
		 * @param ids - the values represented as a String []
		 */
		public void setId(string[] ids)
		{
			this.ids = ids;
		}

		/**
		 * Get the text describing the values associated to the foreign key 
		 * @return A String describing the record
		 */
		public string[] getText()
		{
			return texts;
		}

		/**
		 * Set the text that describes the the values associated to the foreign key
		 * @param text A String describing the record
		 */
		public void setText(string[] texts)
		{
			this.texts = texts;
		}
	}
}