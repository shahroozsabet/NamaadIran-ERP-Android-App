/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
namespace NamaadMobile.Entity
{
    public class RefreshDataEntity
    {
        public int SystemCode { get; set; }
        public int TableCode { get; set; }
        public string TableName { get; set; }
        public int TableDataVersion { get; set; }
        public string TableDesc { get; set; }
        public override string ToString()
        {
            base.ToString();
            return TableDesc;
        }
    }
}