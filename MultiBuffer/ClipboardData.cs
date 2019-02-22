using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Clipboard = System.Windows.Clipboard;

namespace MultiBuffer
{
    public abstract class ClipboardData
    {
        public ClipboardDataType DataType { get; protected set; }

        public static ClipboardData ExtractClipboardData()
        {
            if (Clipboard.ContainsText())
            {
                return new ClipboardTextData(Clipboard.GetText());
            }

            if (Clipboard.ContainsAudio())
            {
                return new ClipboardAudioData(Clipboard.GetAudioStream());
            }

            if (Clipboard.ContainsImage())
            {
                return new ClipboardImageData(Clipboard.GetImage());
            }

            if (Clipboard.ContainsFileDropList())
            {
                var arr = new string[Clipboard.GetFileDropList().Count];
                Clipboard.GetFileDropList().CopyTo(arr, 0);
                return new ClipboardFileDropListData(arr);
            }

            throw new Exception("UnSupport Clipboard data type");
        }

        public enum ClipboardDataType
        {
            Text, Audio, Image, FileDropList
        }
    }

    public class ClipboardTextData : ClipboardData
    {
        public string TextData { get; }

        protected internal ClipboardTextData(string textData)
        {
            TextData = textData;
            DataType = ClipboardDataType.Text;
        }
    }

    public class ClipboardAudioData : ClipboardData
    {
        public Stream AudioStream { get; }

        protected internal ClipboardAudioData(Stream audioStream)
        {
            AudioStream = audioStream;
            DataType = ClipboardDataType.Audio;
        }
    }

    public class ClipboardImageData : ClipboardData
    {
        public BitmapSource ImageData { get; }

        protected internal ClipboardImageData(BitmapSource imageData)
        {
            ImageData = imageData;
            DataType = ClipboardDataType.Image;
        }
    }

    public class ClipboardFileDropListData : ClipboardData
    {
        public IEnumerable<string> FileDropList { get; }

        protected internal ClipboardFileDropListData(IEnumerable<string> fileDropList)
        {
            FileDropList = fileDropList;
            DataType = ClipboardDataType.Image;
        }
    }
}
