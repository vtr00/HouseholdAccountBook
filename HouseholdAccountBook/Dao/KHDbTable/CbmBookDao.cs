﻿using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Dto.KHDbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.KHDbTable
{
    public class CbmBookDao : KHReadDaoBase<CbmBookDto>
    {
        public CbmBookDao(OleDbHandler dbHandler) : base(dbHandler) { }

        public override async Task<IEnumerable<CbmBookDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbmBookDto>(@"SELECT * FROM CBM_BOOK ORDER BY BOOK_ID;");
        }
    }
}
