using System;

namespace PulseCheck.IDomain
{
    public interface ITestFields
    {
        string AlternateCode { get; set; }
        string AlternateCodeSystemName { get; set; }
        string AlternateDescription { get; set; }
        string Code { get; set; }
        string CodeSystemName { get; set; }
        string Comment { get; set; }
        DateTime? Date { get; set; }
        string Flag { get; set; }
        int LineNum { get; set; }
        string Link { get; set; }
        string LOINC { get; set; }
        string Name { get; set; }
        string OrganizationCity { get; set; }
        string OrganizationName { get; set; }
        string OrganizationState { get; set; }
        string OrganizationStreet1 { get; set; }
        string OrganizationStreet2 { get; set; }
        string OrganizationZip { get; set; }
        string Range { get; set; }
        string Result { get; set; }
        string Status { get; set; }
        string Text { get; set; }
        string Units { get; set; }

        void set(string name, string val);
    }
}