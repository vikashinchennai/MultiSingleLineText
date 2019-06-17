namespace Sitecore.Foundation.Field.MultiSingleLineListField
{
    internal class Helper
    {
        public static string GetUniqueId
        {
            get
            {
                return System.DateTime.Now.ToString("yyMMddHHmmssfff");
            }
        }
    }
}
