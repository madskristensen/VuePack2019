using Microsoft.VisualStudio.Utilities;
using Microsoft.WebTools.Languages.Html.Editor.Completion;
using Microsoft.WebTools.Languages.Html.Editor.Completion.Def;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VuePack
{
    [HtmlCompletionProvider(CompletionTypes.Attributes, "*")]
    [ContentType("htmlx")]
    internal class AttributeDirectiveCompletion : BaseCompletion
    {
        public override string CompletionType => CompletionTypes.Attributes;

        public override IList<HtmlCompletion> GetEntries(HtmlCompletionContext context)
        {
            string text = context.Document.TextBuffer.CurrentSnapshot.GetText();
            List<string> names = DirectivesCache.GetValues(DirectiveType.Attribute);
            var list = new List<HtmlCompletion>();

            foreach (Match match in DirectivesCache.AttributeRegex.Matches(text))
            {
                string name = match.Groups["name"].Value;
                if (!names.Contains(name))
                {
                    names.Add(name);
                }
            }

            foreach (string name in names)
            {
                HtmlCompletion item = CreateItem(name, "Custom directive", context.Session);
                list.Add(item);
            }

            return list;
        }
    }
}
