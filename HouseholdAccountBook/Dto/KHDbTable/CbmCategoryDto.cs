﻿using HouseholdAccountBook.Dto.Abstract;

namespace HouseholdAccountBook.Dto.KHDbTable
{
    public class CbmCategoryDto : KHCbmDtoBase
    {
        public CbmCategoryDto() { }

        public int CATEGORY_ID { get; set; }
        public string CATEGORY_NAME { get; set; }
        public int REXP_DIV { get; set; }

    }
}
