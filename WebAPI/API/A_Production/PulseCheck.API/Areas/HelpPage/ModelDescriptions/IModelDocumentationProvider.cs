using System;
using System.Reflection;

namespace PulseCheck.API.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// Model documentation provider interface
    /// </summary>
    public interface IModelDocumentationProvider
    {
        /// <summary>
        /// Get documentation using MemberInfo object
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        string GetDocumentation(MemberInfo member);

        /// <summary>
        /// Get documentation using Type object
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetDocumentation(Type type);
    }
}