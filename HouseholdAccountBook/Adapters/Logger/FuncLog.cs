using System;
using System.Runtime.CompilerServices;

namespace HouseholdAccountBook.Adapters.Logger
{
    public class FuncLog : IDisposable
    {
        private readonly string fileName;
        private readonly string methodName;
        private readonly int lineNumber;

        public FuncLog(object args = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            this.fileName = fileName;
            this.methodName = methodName;
            this.lineNumber = lineNumber;
            Log.FuncStart(args, fileName, methodName, lineNumber);
        }

        public void Dispose()
        {
            Log.FuncEnd(this.fileName, this.methodName, (ushort)(Math.Log10(this.lineNumber) + 1));
            GC.SuppressFinalize(this);
        }
    }
}
