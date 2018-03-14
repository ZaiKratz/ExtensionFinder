using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.InteropServices;


namespace ExtensionsFinder
{
    /*
     * ExtensionFinder class 
     
    */
    class ExtensionFinder
    {
        
        private string _ExtensionsDataBasePath = null;
        private dynamic JSonExtensions = null;

        private ExtensionFinder() { }
        public ExtensionFinder(string ExtensionsDataBaseFilePath)
        {
            ExtensionsDataBasePath = ExtensionsDataBaseFilePath;
            JSonExtensions = ParseJSonFile(ExtensionsDataBasePath);
        }

        //Read file bytes and return file bytes array
        private byte[] ReadBunchOfBytesInFile(string FilePath)
        {
            byte[] Array = new byte[256];
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(FilePath, FileMode.Open)))
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    reader.Read(Array, 0, 256);
                }

            }
            catch (ArgumentNullException Ex)
            {
                throw new ArgumentNullException(FilePath + " is empty. Original message: " + Ex.Message);
            }
            catch (FileNotFoundException Ex)
            {
                throw new FileNotFoundException("File " + FilePath + " not found. Original message: " + Ex.Message);
            }
            return Array;
        }

        //Parse extensions database into JSon object
        private dynamic ParseJSonFile(string FilePath)
        {
            string JSonFormatString = null;

            try
            {
                JSonFormatString = File.ReadAllText(FilePath);
            }
            catch (ArgumentNullException Ex)
            {
                throw new ArgumentNullException(FilePath + " is empty. Original message: " + Ex.Message);
            }
            catch (FileNotFoundException Ex)
            {
                throw new FileNotFoundException("File "+ FilePath + " not found. Original message: " + Ex.Message);
            }

            dynamic Array = JsonConvert.DeserializeObject(JSonFormatString);
            return Array;
        }

        public KeyValuePair<string, List<string>> FindExtensionForFile(string FilePath)
        {
            List<string> ExtensionsList = new List<string>();
            try
            {
                if (FilePath.Length != 0)
                {
                    byte[] BunchOfBytes = ReadBunchOfBytesInFile(FilePath);
                    string FileBytes = BitConverter.ToString(BunchOfBytes);
                    string Trimmed = FileBytes.Replace("-", " ");

                    foreach (var Item in JSonExtensions)
                    {
                        dynamic Signatures = Item.signature;
                        int Offset = Item.offset;
                        string Extension = Item.extension; 
                        foreach(var Sign in Signatures)
                        {
                            string Value = Sign.Value;
                            string BytesToCompare = null;
                            for (int Index = Offset; Index < Value.Length; Index++)
                                BytesToCompare += Trimmed[Index];

                            if ((Value == BytesToCompare) && (Value.Length == BytesToCompare.Length))
                                ExtensionsList.Add("." + Extension);
                        }
                    }
                }
            }
            catch (ArgumentNullException Ex)
            {
                throw new ArgumentNullException(FilePath + " is empty. Original message: " + Ex.Message);
            }
            catch (FileNotFoundException Ex)
            {
                throw new FileNotFoundException("File " + FilePath + " not found. Original message: " + Ex.Message);
            }

            KeyValuePair<string, List<string>> ExtensionsPair =
                        new KeyValuePair<string, List<string>>(new FileInfo(FilePath).Name, ExtensionsList);
            return ExtensionsPair;
        }

        /*properties*/
        public string ExtensionsDataBasePath
        {
            get { return _ExtensionsDataBasePath; }
            set { _ExtensionsDataBasePath = value; }
        }
    }
}
