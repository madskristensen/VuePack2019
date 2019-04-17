using Microsoft.VisualStudio.Utilities;
using Microsoft.WebTools.Languages.Html.Editor.Completion;
using Microsoft.WebTools.Languages.Html.Editor.Completion.Def;
using System.Collections.Generic;

namespace VuePack
{
    [HtmlCompletionProvider(CompletionTypes.Children, "*")]
    [ContentType("htmlx")]
    internal class ElementCompletion : BaseCompletion
    {
        public override string CompletionType => CompletionTypes.Children;

        public override IList<HtmlCompletion> GetEntries(HtmlCompletionContext context)
        {
            var list = new List<HtmlCompletion>
            {
                CreateItem("partial", "<partial> tags serve as outlets for registered partials. Partial contents are also compiled by Vue when inserted. The <partial> element itself will be replaced. It requires a name attribute to be provided.", context.Session),
                CreateItem("component", "Alternative syntax for invoking components. Primarily used for dynamic components with the \"is\" attribute", context.Session),
                CreateItem("render", "Used to render templates", context.Session),
            };

            return list;
        }
    }
}
