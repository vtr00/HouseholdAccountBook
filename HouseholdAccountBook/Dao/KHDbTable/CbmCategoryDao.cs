﻿using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Dto.KHDbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.KHDbTable
{
    public class CbmCategoryDao : KHReadDaoBase<CbmCategoryDto>
    {
        public CbmCategoryDao(OleDbHandler dbHandler) : base(dbHandler) { }

        public override async Task<IEnumerable<CbmCategoryDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbmCategoryDto>(@"SELECT * FROM CBM_CATEGORY ORDER BY CATEGORY_ID;");
        }
    }
}
