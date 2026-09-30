using System;

namespace Dto
{
    public class UserModelNameDto
    {
        public Guid Id { get; set; }
        public string ModelName { get; set; }
    }

    public class CurrencyNameDto
    {

        public string CurrencyName { get; set; }
        public int? SortNumber { get; set; }
    }
    public class UserAppNameDto
    {
        public Guid Id { get; set; }

        public String Key{get;set;}
        public String Name{get;set;}
        public String Description{get;set;}
        public String Link{get;set;}
    }
}
