namespace Sitecore.Foundation.Field.MultiSingleLineListField.Editor
{
    using Sitecore;
    using Sitecore.Diagnostics;
    using Sitecore.Shell.Applications.ContentEditor;
    using Sitecore.Text;
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    public class MultiSingleLineListFieldEditor : NameValue
    {
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");

            if (Sitecore.Context.ClientPage.IsEvent)
            {
                this.LoadValue();
            }
            else
            {
                this.BuildControl();
            }
        }

        protected new void ParameterChange()
        {
            var clientPage = Sitecore.Context.ClientPage;

            bool isFocusIn = clientPage.ClientRequest.EventType == "focus";
            bool isLastBox = clientPage.ClientRequest.Source == StringUtil.GetString(clientPage.ServerProperties[this.ID + "_LastParameterID"]) + "_value";
            //On getting focused on last box or after typing on last box
            if ((isFocusIn && isLastBox) ||
                        (isLastBox && !string.IsNullOrEmpty(clientPage.ClientRequest.Form[clientPage.ClientRequest.Source])))
            {
                var emptyRow = this.BuildParameterKeyValue(string.Empty, string.Empty);
                clientPage.ClientResponse.Insert(this.ID, "beforeEnd", emptyRow);
            }

            clientPage.ClientResponse.SetReturnValue(true);
        }

        private void LoadValue()
        {
            var page = HttpContext.Current.Handler as Page;
            this.LoadValue(page == null ? new NameValueCollection() : page?.Request?.Form ?? new NameValueCollection());
        }

        public void LoadValue(NameValueCollection formCollection)
        {
            if (this.ReadOnly || this.Disabled)
            {
                return;
            }

            var collection = formCollection ?? new NameValueCollection();
            var urlString = new UrlString();
            //To Build Non empty value fields only
            foreach (string key in collection.Keys)
            {
                if (!string.IsNullOrEmpty(key)
                            && key.StartsWith(this.ID + "_Param", StringComparison.InvariantCulture)
                            && !key.EndsWith("_value", StringComparison.InvariantCulture))
                {
                    var resultKey = collection[key];
                    var resultValue = collection[key + "_value"];
                    if (string.IsNullOrEmpty(resultKey) || string.IsNullOrEmpty(resultValue))
                        continue;

                    urlString[HttpUtility.UrlEncode(resultKey)] = HttpUtility.UrlEncode(resultValue) ?? string.Empty;
                }
            }

            this.Value = urlString.ToString();
            this.SetModified();
        }

        public void BuildControl()
        {
            var urlString = new UrlString(this.Value);
            if (urlString?.Parameters?.Keys != null)
            {
                foreach (var key in urlString.Parameters.Keys.Cast<string>().Where(key => key.Length > 0))
                {
                    var input = this.BuildParameterKeyValue(HttpUtility.UrlDecode(key), HttpUtility.UrlDecode(urlString.Parameters[key]));
                    if (!string.IsNullOrEmpty(input))
                        this.Controls.Add(new LiteralControl(input));
                }
            }
            //To Add Empty New Input field
            var _input = this.BuildParameterKeyValue(string.Empty, string.Empty);
            if (!string.IsNullOrEmpty(_input))
                this.Controls.Add(new LiteralControl(_input));
        }

        private string BuildParameterKeyValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = System.DateTime.Now.ToString("yyyyMMddHHmmssfff");
            }
            else if (string.IsNullOrEmpty(value))
                return string.Empty;
           
            var uniqueId = GetUniqueID(this.ID + "_Param");
            Sitecore.Context.ClientPage.ServerProperties[this.ID + "_LastParameterID"] = uniqueId;
            var clientEvent = Sitecore.Context.ClientPage.GetClientEvent(this.ID + ".ParameterChange");
            var isVertical = this.IsVertical ? "</tr><tr>" : string.Empty;

            var keyHtml = string.Format(
                        "<input type=\"hidden\" id=\"{0}\" name=\"{1}\" type=\"text\"{2}{3} style=\"{6}\" value=\"{4}\" onchange=\"{5}\"/>",
                        uniqueId, uniqueId, IsReadOnly(), IsDisabled(), StringUtil.EscapeQuote(key), clientEvent, this.NameStyle);
            
            var valueHtml = GetValueHtmlControl(uniqueId, StringUtil.EscapeQuote(HttpUtility.UrlDecode(value)), clientEvent);

            return
                string.Format(
                    "<table width=\"100%\" class='scAdditionalParameters'><tr><td>{0}</td>{2}<td width=\"100%\">{1}</td></tr></table>",
                    keyHtml, valueHtml, isVertical);
        }

        protected string GetValueHtmlControl(string id, string value,string clientEvent)
        {
            return string.Format("<input id=\"{0}_value\" name=\"{0}_value\" type=\"text\" style=\"width:100%\" value=\"{1}\" onfocus=\"{4}\"  onchange=\"{4}\"{2}{3}/>",
                (object)id, (object)value, IsReadOnly(), IsDisabled(), clientEvent);
        }

        private string IsDisabled()
        {
            return this.Disabled ? " disabled=\"disabled\"" : string.Empty; ;
        }
        private string IsReadOnly()
        {
            return this.ReadOnly ? " readonly=\"readonly\"" : string.Empty;
        }
    }
}