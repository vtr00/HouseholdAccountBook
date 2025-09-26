using HouseholdAccountBook.Models.DbHandler;
using System;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    public abstract class DbRequestEventArgsBase : EventArgs
    {
        public DbHandlerFactory DbHandlerFactory { get; set; }

    }
}
