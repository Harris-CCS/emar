using System;
using System.Collections.Generic;

namespace Emar.Core.InboundData.Model
{
    public class InboundUserDataDto
    {
        public string ExternalId { get; set; }
        public int ExternalUserNum { get; set; }
        public int InternalSiteId { get; set; }
        public string Type { get; set; }
        public bool IsActive { get; set; }
        public string InitialDisplay { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string NameSuffix { get; set; }
        public bool OrderingOnlyPhysician { get; set; }
        public bool NameDisplayInitials { get; set; }
        public string LoginName { get; set; }
        public string LoginPassword { get; set; }
        public byte[] Salt { get; set; }
        public DateTimeOffset? LastLoginTime { get; set; }
        public int FailedLoginAttempts { get; set; }

        public IEnumerable<InboundUserSettingsDto> UserSettings { get; set; }
    }
}
