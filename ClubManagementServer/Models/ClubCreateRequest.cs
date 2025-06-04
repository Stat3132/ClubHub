using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubManagementServer.Models
{
    public class ClubCreateRequest
    {
        public required Guid CreateRequestID { get; set; }
        public required string ClubName { get; set; }
        public required string ClubDeclaration { get; set; }
        public required string StudentName { get; set; }
        public required string StudentEmail { get; set; }
        public required string ReasonToCreate { get; set; }
        public required DateTime RequestDate { get; set; }

        [NotMapped]
        public required string AdvisorEmail { get; set; }
    }
}