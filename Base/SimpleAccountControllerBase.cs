using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Formula.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Formula.SimpleAPI;
using Formula.SimpleAuthServer;

namespace Formula.SimpleAuthServerUI
{
    public abstract class SimpleAccountControllerBase : SimpleControllerBase
    {
        public IActionResult HandleReturnUrl(URLContextService context, Boolean failIfUnknown = true)
        {
            return HandleReturnUrl(context.TrustType, context.Url, failIfUnknown);
        }

        public IActionResult HandleReturnUrl(URLTrustType urlTrust, String returnUrl, Boolean failIfUnknown = true)
        {
            if (urlTrust == URLTrustType.Native)
            {
                return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });
            }
            else if (urlTrust == URLTrustType.Known)
            {
                return Redirect(returnUrl);
            }
            else // URLTrust.Unknown
            {
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else if (string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect("~/");
                }
                else
                {
                    if (failIfUnknown) 
                    {
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception("invalid return URL");
                    }
                    else
                    {
                        // since we don't have a valid context, then we just go back to the home page
                        return Redirect("~/");
                    }
                }
            }
        }
    }
}