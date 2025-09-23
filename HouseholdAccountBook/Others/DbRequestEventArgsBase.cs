using HouseholdAccountBook.Models.DbHandler;
using System;

namespace HouseholdAccountBook.Others
{
    public abstract class DbRequestEventArgsBase : EventArgs
    {
        public DbHandlerFactory DbHandlerFactory { get; set; }

    }
}
