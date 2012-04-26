using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace SaaS.Web.Models
{
    public class RegisterModel
    {
        [Required]
        [RegularExpression(ModelRegex.Email, ErrorMessage = "Must be a valid email address")]
        public string Email { get; set; }
        [Required]
        [StringLength(36, ErrorMessage = "The password needs at least 4 characters.", MinimumLength = 4)]
        public string Password { get; set; }
        [Required]
        [RegularExpression(ModelRegex.Name, ErrorMessage = "Name can contain only letters and a some special symbols")]
        [StringLength(36, MinimumLength = 3)]
        public string CompanyName { get; set; }

        [RegularExpression(ModelRegex.Name, ErrorMessage = "Name can contain only letters and a some special symbols")]
        [StringLength(36, MinimumLength = 3)]
        public string RealName { get; set; }

        [RegularExpression("^[a-zA-z'\\-\\s0-9]+$", ErrorMessage = "Must be text, numbers or '-'")]
        public string ContactPhone { get; set; }

        
    }

    public static class Extensions
    {
        public static MvcHtmlString ValidationBootstrapStateFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return new MvcHtmlString(htmlHelper.IsModelValid(expression) ? "" : "error");
        }



        public static bool IsModelValid<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var text = ExpressionHelper.GetExpressionText(expression);
            string modelName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(text);


            return htmlHelper.ViewContext.ViewData.ModelState.IsValidField(modelName);
        }
    }
}