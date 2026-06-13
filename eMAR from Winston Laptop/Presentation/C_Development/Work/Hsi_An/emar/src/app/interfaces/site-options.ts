export interface SiteOptions {
  // Global Options:
  antimicrobial_display?: string;
  host_server_url?: string; // Base URL of PulseCheck ED Server

  // Site Specific Options:
  long_date_format?: string;
  short_date_format?: string;
  patient_image_path?: string;
  schedule_future_items?: number; // How many days in the future to generate future administration records
  custom_indicators_image_path?: string;
  medinpat?: string; // Y or N
  medoutpat?: string; // Y or N
  rxalert?: number; // 0 = Show All, 5 = Show Moderate, Server and Contradicted Only, 6 = Show Severe and Contradicted Only
  medpyxis?: string; // Y or N
  medexactmatch?: string; // Y or N
  drug_db_vendor?: string; // "M" = Multum, "F" = FDB US, "1" = FDB-CA, "2" = Medispan
  session_timeout?: number; // Timeout in Minutes 1 - 240
  session_timeout_url?: string; // URL to launch when timeout is triggered
  show_dose_form?: string; // Y or N. "Y" = Include Dose Form in display, "N" = Do no include dose form in display
  show_strength?: string; // Y or N. "Y" = Include the strength in display, "N" = Do no include the strength in display
  popup_on_give?: string; // Y or N. "Y" = 5 rights popup display on a give, "N" = 5 rights popup does not display on a give.
  default_printer_id?: string; // Device ID of default printer for a site
}
