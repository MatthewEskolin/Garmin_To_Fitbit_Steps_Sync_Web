// Add more target elements here on a new line if you want to target more than just div.  Example: [HtmlTargetElement("a")] to hide/show links
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Garmin_To_Fitbit_Steps_Sync_Web.Utilities
{


    [HtmlTargetElement("*", Attributes = "is-visible")]
    public class VisibilityTagHelper : TagHelper
    {
        // default to true otherwise all existing target elements will not be shown, because bool's default to false
        public bool IsVisible { get; set; } = true;

        // You only need one of these Process methods, but just showing the sync and async versions
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!IsVisible)
                output.SuppressOutput();

            base.Process(context, output);
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!IsVisible)
                output.SuppressOutput();

            return base.ProcessAsync(context, output);
        }
    }



}