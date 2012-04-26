using System.Collections.Generic;
using System.Web.Mvc;

namespace SaaS.Web.Models
{
    static class ModelRegex
    {
        public const string Email =
            @"^[a-zA-Z0-9](([_\.\-\'\+]?[a-zA-Z0-9]+)*)" +
                @"@(([a-zA-Z0-9]{1,64})(([\.\-][a-zA-Z0-9]{1,64})*)\.([a-zA-Z]{2,})" +
                    @"|(\d{1,3}(\.\d{1,3}){3}))$";

        public const string Name = "^([a-zA-Z0-9'\\-]+\\s+){0,4}[a-zA-Z0-9'\\-]+$";
    }



}