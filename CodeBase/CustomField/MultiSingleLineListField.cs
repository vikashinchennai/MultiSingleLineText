namespace Sitecore.Foundation.Field.MultiSingleLineListField.CustomField
{
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Links;
    using Sitecore.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class MultiSingleLineListField : CustomField
    {
        public List<string> SingleLineMultiListValues
        {
            get
            {
                var _valueList = new List<string>();
                var val = StringUtil.GetNameValues(this.Value, '=', '&');
                if (val != null)
                {
                    foreach (var item in val.AllKeys)
                    {
                        _valueList.Add(val[item]);
                    }
                }
                return _valueList;
            }
            set
            {
                Assert.ArgumentNotNull((object)value, nameof(value));
                var urlString = new UrlString();
                if (value.Count > 0)
                {
                    var val = StringUtil.GetNameValues(this.Value, '=', '&');
                    List<long> originalKeys = new List<long>();
                    if (val != null && val.AllKeys.Any())
                    {
                        originalKeys = val.AllKeys.Select(ss => long.Parse(ss)).ToList();
                    }
                    var pendingCounts = value.Count - originalKeys.Count;

                    if (pendingCounts > 0)
                    {
                        var minValue = originalKeys.Any() ? originalKeys.Min() : long.Parse(Helper.GetUniqueId) - 1;

                        for (int i = 0; i < pendingCounts; i++)
                        {
                            originalKeys.Insert(0, minValue - i);
                        }
                    }
                    int _cnt = 0;
                    foreach (var eachValue in value)
                    {
                        if (!string.IsNullOrEmpty(eachValue))
                            urlString[HttpUtility.UrlEncode(originalKeys[_cnt++].ToString())] = HttpUtility.UrlEncode(eachValue) ?? string.Empty;
                    }
                }
                this.Value = urlString.ToString();
            }
        }

        public MultiSingleLineListField(Field innerField)
          : base(innerField)
        {
        }

        public static implicit operator MultiSingleLineListField(Field field)
        {
            if (field != null)
                return new MultiSingleLineListField(field);
            return (MultiSingleLineListField)null;
        }

        public override void ValidateLinks(LinksValidationResult result)
        {
            Assert.ArgumentNotNull((object)result, nameof(result));
            foreach (var values in this.SingleLineMultiListValues)
            {
                if (values != null && Guid.TryParse(values, out Guid id))
                {
                    Item targetItem = this.InnerField.Database.GetItem(HttpUtility.UrlDecode(values));
                    if (targetItem != null)
                        result.AddValidLink(targetItem, targetItem.Paths.FullPath);
                }
            }
        }
    }
}