namespace Emar.Core.Options.Model
{
    public enum OptionNames
    {
        LONG_DATE_FORMAT,
        SHORT_DATE_FORMAT,
        SCHEDULE_FUTURE_ITEMS,
        PATIENT_IMAGE_PATH,
        CUSTOM_INDICATORS_IMAGE_PATH,
        RXALERT,
        MEDINPAT,
        MEDOUTPAT,
        MEDPYXIS,
        MEDEXACTMATCH,
        DRUG_DB_VENDOR,
        SESSION_TIMEOUT,
        SESSION_TIMEOUT_URL,
        SHOW_DOSE_FORM,
        SHOW_STRENGTH,
        POPUP_ON_GIVE,
        DEFAULT_PRINTER_ID,
        //Moving this from a global option to a site options.
        //We need it to be a site option so that each site can go to different places.
        //Emerus uses one load balancer for central time and another for mountain time.
        //The UI should be good since this will show up at the same place in the JSON
        //structure, just sorted with the site options not the global options.
        //Winston Murdock, 01/10/2023.
        HOST_SERVER_URL
    }
}