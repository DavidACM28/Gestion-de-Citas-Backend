using System;
using System.Collections.Generic;
using System.Text;

namespace Gestion.Citas.Business.Constants
{
    public static class AppointmentStatus
    {
        public const string REQUESTED = "REQUESTED";
        public const string CONFIRMED = "CONFIRMED";
        public const string BEING_ATTENDED = "BEING_ATTENDED";
        public const string FINISHED = "FINISHED";
        public const string CANCELED = "CANCELED";
        public const string DID_NOT_ATTEND = "DID_NOT_ATTEND";
    }
}
