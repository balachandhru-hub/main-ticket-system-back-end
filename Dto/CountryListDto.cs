using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Dto
{
    public class CountryListDto
    {
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string MobileCountryCode { get; set; }
    }
}