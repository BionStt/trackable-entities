//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApiSample.Client.Entities.Temp
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using TrackableEntities;
    using TrackableEntities.Client;
    
    public partial class Location : ModelBase<Location>
    {
    
    	[DataMember]
    	public string City
    	{ 
    		get { return _City; }
    		set
    		{
    			if (Equals(value, _City)) return;
    			_City = value;
    			NotifyPropertyChanged(m => m.City);
    		}
    	}
    	private string _City;
    
    	[DataMember]
    	public string Country
    	{ 
    		get { return _Country; }
    		set
    		{
    			if (Equals(value, _Country)) return;
    			_Country = value;
    			NotifyPropertyChanged(m => m.Country);
    		}
    	}
    	private string _Country;
    }
}
