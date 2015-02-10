﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TrackableEntities.Client
{
    public abstract partial class EntityBase
    {
        /// <summary>
        /// Change-tracking state of an entity.
        /// </summary>
        [NotMapped]
        public TrackingState TrackingState { get; set; }

        /// <summary>
        /// Properties on an entity that have been modified.
        /// </summary>
        [NotMapped]
        public ICollection<string> ModifiedProperties { get; set; }

        /// <summary>
        /// Identifier used for correlation with MergeChanges.
        /// </summary>
        [NotMapped]
        public Guid EntityIdentifier { get; set; }

        /// <summary>
        /// Fire PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
