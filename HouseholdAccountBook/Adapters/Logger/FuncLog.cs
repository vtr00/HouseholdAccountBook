using System;
using System.Runtime.CompilerServices;

namespace HouseholdAccountBook.Adapters.Logger
{
    public class FuncLog : IDisposable
    {
        private readonly string fileName;
        private readonly string methodName;

        public FuncLog(object args = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            this.fileName = fileName;
            this.methodName = methodName;
            Log.FuncStart(args, fileName, methodName, lineNumber);
        }

        public void Dispose()
        {
            Log.FuncEnd(this.fileName, this.methodName);
            GC.SuppressFinalize(this);
        }
    }
}
