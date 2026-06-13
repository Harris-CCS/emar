using System;
using System.Collections.Generic;
using System.Text;

namespace Emar.Core.Users.Model
{
    public class OrderingPhysicianDataDto
    {
        public IEnumerable<UserDto> AvailableOrderingPhysicians { get; set; }
        public int? PatientsErAttendingDoc { get; set; }
    }
}
