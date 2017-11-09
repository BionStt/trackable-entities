﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackableEntities.EF.Tests.FamilyModels
{
    public class Contact : ITrackable
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(ContactDetail))]
        public int ContactDetailId { get; set; }

        public ContactDetail ContactDetail { get; set; }

        [NotMapped]
        public TrackingState TrackingState { get; set; }

        [NotMapped]
        public ICollection<string> ModifiedProperties { get; set; }
    }
}