using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RazorEngine;
using RazorEngine.Templating;
using System.IO;
using PulseCheck.Domain;

namespace PulseCheck.API
{
    /// <summary>
    /// Configuration of Razor Engine
    /// </summary>
    public class RazorEngineConfig
    {
        /// <summary>
        /// Configure templates for Razor Engine
        /// </summary>
        public static void CompileTemplates()
        {
            var templates = new Dictionary<string, string>
            {
                { Domain.Constants.EmailTemplates.NEW_ACCOUNT, "NewAccountEmail" },
                { Domain.Constants.EmailTemplates.PASSWORD_RESET, "PasswordResetEmail" },
                { Domain.Constants.EmailTemplates.DEVICE_AUTHORIZATION, "DeviceAuthorizationEmail" },
            };

            foreach (var templateKey in templates.Keys)
            {
                var customPath = "~/Templates/Custom/" + templates[templateKey] + ".cshtml";
                var filePath = File.Exists(customPath) ? customPath : "~/Templates/" + templates[templateKey] + ".cshtml";
                var templateText = File.ReadAllText(HttpContext.Current.Server.MapPath(filePath));
                Engine.Razor.Compile(templateText, templateKey, typeof(MobileEmail));
            }
        } 
    }
}