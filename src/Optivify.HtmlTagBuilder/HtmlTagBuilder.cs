using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

namespace Optivify.HtmlTagBuilder
{
    public class HtmlTagBuilder
    {
        public string TagName { get; private set; }

        public string InnerHtml { get; set; }

        public bool EncodeValue { get; set; } = true;

        public IDictionary<string, string> Attributes { get; private set; }

        public HtmlTagBuilder(string tagName)
        {
            if (tagName is null)
            {
                throw new ArgumentNullException(nameof(tagName));
            }

            this.TagName = tagName;
            this.InnerHtml = string.Empty;
            this.Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        #region Css Class

        public void AddCssClass(string value)
        {
            if (this.Attributes.TryGetValue("class", out string value2))
            {
                this.Attributes["class"] = value + " " + value2;
            }
            else
            {
                this.Attributes["class"] = value;
            }
        }

        #endregion

        #region Attributes

        private void AppendAttributes(StringBuilder sb)
        {
            foreach (KeyValuePair<string, string> attribute in this.Attributes)
            {
                if (!string.Equals(attribute.Key, "id", StringComparison.Ordinal))
                {
                    sb.Append(' ').Append(attribute.Key);

                    if (!string.IsNullOrEmpty(attribute.Value))
                    {
                        sb.Append("=\"");

                        if (this.EncodeValue)
                        {
                            sb.Append(HttpUtility.HtmlAttributeEncode(attribute.Value));
                        }
                        else
                        {
                            sb.Append(attribute.Value);
                        }

                        sb.Append('"');
                    }
                }
            }
        }

        public void MergeAttribute(string key, string value, bool replaceExisting = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }

            if (replaceExisting || !this.Attributes.ContainsKey(key))
            {
                this.Attributes[key] = value;
            }
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes, bool replaceExisting = false)
        {
            if (attributes == null)
            {
                return;
            }

            foreach (KeyValuePair<TKey, TValue> attribute in attributes)
            {
                var key = Convert.ToString(attribute.Key, CultureInfo.InvariantCulture);
                var value = Convert.ToString(attribute.Value, CultureInfo.InvariantCulture);

                this.MergeAttribute(key, value, replaceExisting);
            }
        }

        #endregion

        public void SetInnerText(string innerText)
        {
            this.InnerHtml = this.EncodeValue ? HttpUtility.HtmlEncode(innerText) : innerText;
        }

        public string ToString(HtmlTagRenderMode renderMode)
        {
            var stringBuilder = new StringBuilder();

            switch (renderMode)
            {
                case HtmlTagRenderMode.StartTag:
                    stringBuilder
                        .Append('<')
                        .Append(this.TagName);
                    this.AppendAttributes(stringBuilder);
                    stringBuilder.Append('>');
                    break;

                case HtmlTagRenderMode.EndTag:
                    stringBuilder
                        .Append("</")
                        .Append(this.TagName)
                        .Append('>');
                    break;

                case HtmlTagRenderMode.SelfClosing:
                    stringBuilder
                        .Append('<')
                        .Append(this.TagName);
                    this.AppendAttributes(stringBuilder);
                    stringBuilder.Append(" />");
                    break;

                default:
                    stringBuilder
                        .Append('<')
                        .Append(this.TagName);
                    this.AppendAttributes(stringBuilder);
                    stringBuilder.Append('>').Append(this.InnerHtml)
                        .Append("</")
                        .Append(TagName)
                        .Append('>');
                    break;
            }

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return this.ToString(HtmlTagRenderMode.Normal);
        }
    }
}
