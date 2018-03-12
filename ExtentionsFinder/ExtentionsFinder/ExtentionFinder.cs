using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

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
        private string ReadBunchOfBytesInFile(string FilePath)
        {
            byte[] Array = new byte[100];
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(FilePath, FileMode.Open)))
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    reader.Read(Array, 0, 100);
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
            return BitConverter.ToString(Array);
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

        public Dictionary<string, string> FindExtensionForFile(string FilePath)
        {
            Dictionary<string, string> ExtensionsDictionary = new Dictionary<string, string>();
            try
            {
                if (FilePath.Length != 0)
                {
                    string FileBytes = ReadBunchOfBytesInFile(FilePath);
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
                                ExtensionsDictionary.Add(new FileInfo(FilePath).Name, "." +Extension);
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

            return ExtensionsDictionary;
        }

        /*properties*/
        public string ExtensionsDataBasePath
        {
            get { return _ExtensionsDataBasePath; }
            set { _ExtensionsDataBasePath = value; }
        }
    }
}
