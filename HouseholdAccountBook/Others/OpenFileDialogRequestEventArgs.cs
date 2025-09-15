using System;

namespace HouseholdAccountBook.Others
{
    public class OpenFileDialogRequestEventArgs : EventArgs
    {
        public bool CheckFileExists { get; set; } = true;

        public string InitialDirectory { get; set; }

        public string FileName { get; set; }

        public string Title { get; set; }

        public string Filter { get; set; }

        public bool Multiselect { get; set; } = false;

        public string[] FileNames { get; set; }

        public bool? Result { get; set; } = false;
    }
}
