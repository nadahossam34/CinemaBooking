using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.ViewModel
{
    public class NearestCinemaViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public double DistanceKm { get; set; }

        public double UserLatitude { get; set; }
        public double UserLongitude { get; set; }
    }
}
