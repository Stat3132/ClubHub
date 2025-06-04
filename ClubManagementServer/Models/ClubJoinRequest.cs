using System;

namespace ClubManagementServer.Models
{
    public class ClubJoinRequest
    {
        public required Guid JoinRequestID { get; set; }
        public required Guid ClubID { get; set; }
        public required string StudentName { get; set; }
        public required string StudentEmail { get; set; }
        public required string ReasonToJoin { get; set; }
        public required DateTime RequestDate { get; set; }
    }
}