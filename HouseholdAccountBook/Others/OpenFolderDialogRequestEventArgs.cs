using System;

namespace HouseholdAccountBook.Others
{
    public class OpenFolderDialogRequestEventArgs : EventArgs
    {
        public string InitialDirectory { get; set; }

        public string FolderName { get; set; }

        public string Title { get; set; }

        public bool? Result { get; set; } = false;
    }
}
