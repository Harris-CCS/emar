using System;
using System.Collections.Generic;
using System.Linq;
using Emar.Core.Helpers;
using Emar.Core.Options.Model;
using Emar.Data;
using Microsoft.Extensions.Caching.Memory;

namespace Emar.Core.Options.Repository
{
    public class OptionRepository : IOptionRepository
    {
        private readonly EmarContext _context;
        private readonly MemoryCache _cache;
        
        public OptionRepository(EmarContext emarContext, EmarMemoryCache cache)
        {
            _context = emarContext ?? throw new ArgumentNullException(nameof(emarContext));
            _cache = cache.Cache;
        }

        public string GetOption(int siteId, OptionNames optionName, string defaultValue = null)
        {
            if(GetOptionDictionary(siteId).TryGetValue(optionName.ToString(), out string optionValue))
                return optionValue.Trim() ?? defaultValue;

            return defaultValue;
        }

        public bool GetOptionBool(int siteId, OptionNames optionName, bool? defaultValue = null)
        {
            //***************************************************************
            //Function Name:        GetOptionBool
            //Author:               Winston Murdock
            //Date:                 10/01/2020
            //Purpose:              To return a boolean for any options that are Y/N, Yes/No, etc...
            //************************************** *************************

            //Get the actual option value from the DB.
            var ret = GetOption(siteId, optionName);

            // If we didn't get a value from the options table, 
            if (ret == null)
            {
                // if a default was provided, use it
                if (defaultValue.HasValue)
                    return defaultValue.Value;

                // Else return false
                return false;
            }

            //If first character is "Y", "T", or "1" then return true, else false
            return ret.StartsWith("Y", true, null)
                   || ret.StartsWith("T", true, null)
                   || ret.StartsWith('1');
        } //end GetOptionBool
        
        private Dictionary<string, string> GetOptionDictionary(int siteId)
        {
            return _cache.GetOrCreate(siteId + CacheKeys.SiteOptions, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var ret =
                    _context.SiteOptions
                        .Where(s => s.SiteId == siteId)
                        .Select(s => new {Key = s.Option.Name, Value = s.OptionValue})
                        .ToDictionary(e => e.Key, e => e.Value);
                entry.Size = ret.Count;
                return ret;
            });
        }

        public Dictionary<string, string> GetSiteOptions(int siteId, string optionsList)
        {
            //Get the specified options for the current site.
            //Winston Murdock, 10/29/2020.  EMAR-441.
            //Also get all of the "global" options.
            //Winston Murdock, 11/20/2020.  EMAR-508
            //When "all" is specified, get all of the site-specific options
            //and all of the global options.
            //Thanks to Mario Roy for the suggestion.
            //Winston Murdock, 12/01/2020.  EMAR-441

            //Return variable.
            //Field 1 is the name.
            //Field 2 is the value.
            var optionList = new Dictionary<string, string>();

            //Declare the string array of option names here.
            //That way both the if and else below have access to it.
            string[] optionNames;

            //See if the parameter is "all".
            if (optionsList.ToLower() == "all")
            {
                //They want all site options.
                //Get the names of all of the site options into an array.
                //https://stackoverflow.com/a/972323
                var optionNamesFromEnum = OptionNames.GetValues(typeof(OptionNames));

                //The line above returns an array of objects.
                //Convert it to an array of strings.
                //https://stackoverflow.com/a/1970750
                optionNames = optionNamesFromEnum.OfType<object>().Select(o => o.ToString()).ToArray();
            }
            else
            {
                //They specified the options that they want.
                //Split the options list into an array.
                optionNames = optionsList.Split(",", StringSplitOptions.RemoveEmptyEntries);
            } //end if

            //Regardless of which branch we took above, we've got the option names
            //that we want in the optionNames array.
            
            //Sort the array of options.
            Array.Sort(optionNames);

            //For each option, get the value and add it to the return variable.
            foreach (var s in optionNames)
            {
                //Store the current option name in a string.
                //Also convert it to lower case and trim any spaces.
                var sTemp = s.ToLower().Trim();

                //See if there's an option with the name specified.
                //Using ToUpper since all of our options are in upper case in the DB.
                //And this will allow the option names to be case insensitive.
                //If there is one, then get the value for that option and add it to the return variable.
                //If there isn't one, then skip this one and move to the next iteration.
                if (Enum.TryParse(typeof(OptionNames), sTemp, true, out object temp))
                {
                    //The "temp" variable is the actual OptionName enum value.
                    //Convert the option name to lower case in the return value.
                    //Also trim any leading and trailing spaces from the string.
                    optionList.Add(sTemp, GetOption(siteId, (OptionNames)temp));
                } //end if.
            } //end loop.

            //Go get the global options and add them to the dictionary.
            //Sort the global options by name.
            //Try to get the global options from the cache.
            //If they're in the cache already, then this will pull from there.
            //If they aren't in the cache, then this will grab them from the DB.
            var globalOptions = _cache.GetOrCreate("All" + CacheKeys.GlobalOptions, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var ret = _context.GlobalOptions.OrderBy(i => i.Name).ToList();
                entry.Size = ret.Count;
                return ret;
            });

            //Now add everything from the global options to optionList.
            foreach (var go in globalOptions)
            {
                //Add each option to the return variable.
                //Lower case the name.
                optionList.Add(go.Name.ToLower(), go.Value);
            } //end foreach loop

            //Return the dictionary.
            return optionList;
        } //end GetSiteOptions
    }
}
