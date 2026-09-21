using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.ViewModel
{
    public class CinemaViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
