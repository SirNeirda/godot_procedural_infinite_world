using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Concurrent;
using System.Globalization;
using Godot;
using System.IO.Compression;
//using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Bouncerock
{
    public static class FileWriter
    {
        private static readonly string PrivateKey = "1234567890123456";

        public static T LoadEncryptedXML<T>(string path) where T : class
        {
            T result;

            if (!File.Exists(path))
            {
                GD.Print("File " + path + " does not exist!");
                return null;
            }

            string data;
            using (var reader = new StreamReader(path))
            {
                data = DecryptDataToString(reader.ReadToEnd());
                GD.Print(data);
            }

            var stream = new MemoryStream();
            using (var sw = new StreamWriter(stream) { AutoFlush = true })
            {
                sw.WriteLine(data);
                stream.Position = 0;
                result = new XmlSerializer(typeof(T)).Deserialize(stream) as T;
            }

            return result;
        }

        public static void SaveEncryptedXML<T>(string path, object value) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, value);
                stream.Flush();
                stream.Position = 0;
                string sr = new StreamReader(stream).ReadToEnd();
                var fileStream = new FileStream(path, FileMode.Create);
                var streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine(EncryptDataToString(sr));
                streamWriter.Close();
                fileStream.Close();
            }
        }

       


        public static void WriteXML(object item, string path)
        {
            XmlSerializer serializer = new XmlSerializer(item.GetType());
            StreamWriter writer = new StreamWriter(path);
            serializer.Serialize(writer.BaseStream, item);
            writer.Close();
        }

        private static string RemoveInvalidXmlChars(string text)
        {
            if (text == null)
                return text;
            if (text.Length == 0)
                return text;

            if (text.Contains("\n"))
            {
                text = text.Replace("\n", "[N]");
            }
            if (text.Contains("\b"))
            {
                text = text.Replace("\b", "[B]");
            }
            return text;
        }


        /// -------------------------------------------------------------
        /// ISL FILES
        /// ISL files are custom save files used in the project.
        /// -------------------------------------------------------------

        public static bool StringToISL(string json, string fullpath, string version)
        {
            try
            {
                byte[] header = WriteISLHeader();

                byte[] content = (Encoding.UTF8.GetBytes(json));

                byte[] resultArray = new byte[header.Length + content.Length];
                /*GD.Print("Here's how it goes : " +
                   " Result array total length:" + resultArray.Length + "\n" +
                   " header total length:" + header.Length + "\n" +
                    " content total length:" + content.Length + "\n"
                    );*/
                Buffer.BlockCopy(header, 0, resultArray, 0, header.Length);

                Buffer.BlockCopy(content, 0, resultArray, header.Length, content.Length);
                /*Stream stream = new MemoryStream();
                stream.Write(header,0,header.Length);
                stream.Write(content, header.Length, content.Length);*/
                File.WriteAllBytes(fullpath, resultArray);
                //GD.Print("Wrote all bytes");
                return true;
            }
            catch (Exception e)
            {
                GD.Print(e.Message);
                return false;
            }
        }

        public static bool BinaryToISL(byte[] content, string fullpath, string version = "00", bool compress = true, bool encrypt = false)
        {
            try
            {
                byte[] header = WriteISLHeader();

                byte[] resultArray = new byte[header.Length + content.Length];
                /*GD.Print("Here's how it goes : " +
                   " Result array total length:" + resultArray.Length + "\n" +
                   " header total length:" + header.Length + "\n" +
                    " content total length:" + content.Length + "\n"
                    );*/
                Buffer.BlockCopy(header, 0, resultArray, 0, header.Length);

                Buffer.BlockCopy(content, 0, resultArray, header.Length, content.Length);
                /*Stream stream = new MemoryStream();
                stream.Write(header,0,header.Length);
                stream.Write(content, header.Length, content.Length);*/
                //resultArray = Compress(resultArray);
                CreateMetaFile(fullpath, content);

                File.WriteAllBytes(fullpath + ".isl", resultArray);
                GD.Print("Wrote " + content.Length + " bytes for " + fullpath);
                return true;
            }
            catch (Exception e)
            {
                GD.Print(e.Message);
                return false;
            }
        }

         public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        public static void CreateMetaFile(string fullpath, byte[] content)
        {
            
            string hash = GetHashMD5FromBytes(content);
            string metaextention =".meta";
            StreamWriter writer = new StreamWriter(fullpath+metaextention);
            writer.WriteLine(hash);
            writer.Close();
        }

        // Compute a file's hash.
        public static string GetHashMD5FromFile(string fullpath)
        {
            MD5 Md5 = MD5.Create();
            FileStream stream = File.OpenRead(fullpath);
            if (stream != null)
            {
                byte[] result = Md5.ComputeHash(stream);
                //return result.ToString();
                int i;
                StringBuilder sOutput = new StringBuilder(result.Length);
                for (i = 0; i < result.Length; i++)
                {
                    sOutput.Append(result[i].ToString("x2"));
                }
                stream.Close();
                return sOutput.ToString();
            }
            return null;
        }

        public static string GetHashMD5FromBytes(byte[] buffer)
        {
            MD5 Md5 = MD5.Create();
            byte[] result = Md5.ComputeHash(buffer);
            StringBuilder sOutput = new StringBuilder(result.Length);
                for (int i = 0; i < result.Length; i++)
                {
                    sOutput.Append(result[i].ToString("x2"));
                }
                return sOutput.ToString();
        }

        //Bytes header.
        //12 bytes
        //Bytes 1-3 = ISL
        //Bytes 4
        private static byte[] WriteISLHeader(string customdate = "")
        {

            //First 3 bytes are magic number to signify we are an ISL file
            //Debug.Log(arrayNow.Length + "//" + arrayNow);
            byte[] header = new byte[12];
            int index = 0;
            byte[] format = Encoding.UTF8.GetBytes("ISL");
            Buffer.BlockCopy(format, 0, header, index, format.Length);
            index = index + format.Length;

            //Now we add the date created to the header
            long now = DateTime.Now.ToBinary();
            byte[] arrayNow = BitConverter.GetBytes(now);
            Buffer.BlockCopy(arrayNow, 0, header, index, arrayNow.Length);
            index = index + arrayNow.Length;
            GD.Print("We wrote" + index + "bytes with " + (header.Length - index) + " left.");
            return header;
        }
        private static void ReadISLHeader(byte[] header)
        {
            if (header.Length != 12) { GD.Print("header isn't expected size"); }
            string format = Encoding.UTF8.GetString(header, 0, 3);
            
            byte[] date = new byte[8];
            Buffer.BlockCopy(header, 3, date, 0, 8);
            long datetime = BitConverter.ToInt64(date,0);
            DateTime now = DateTime.FromBinary(datetime);
            
        }

        public static void Test(string path)
        {
            FileStream stream = File.OpenRead(path);
            byte[] bytessss = new byte[stream.Length];
            stream.Read(bytessss, 0, bytessss.Length); 
            string json = Encoding.UTF8.GetString(Decrypt(bytessss));
            GD.Print(json);
        }


       /* public static TReturn ReadISL<TReturn>(string filepath)
        {
            
            
            FileStream stream = File.OpenRead(filepath);
            byte[] total = new byte[stream.Length];

            byte[] header = new byte[12];
            byte[] content = new byte[stream.Length - header.Length];

            stream.Read(header, 0, 12);
            ReadISLHeader(header);

            stream.Read(content, 0, content.Length);

            string json = Encoding.UTF8.GetString(Decrypt(content));


            TReturn result = JsonConvert.DeserializeObject<TReturn>(json);
            return result;
        }*/

 

        public static byte[] SerializeToBinary<T>(T obj) where T : class
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                using (MemoryStream memoryStream = new MemoryStream())
                using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream))
                {
                    serializer.WriteObject(xmlWriter, obj);
                    xmlWriter.Flush();
                    return memoryStream.ToArray();
                }
            }

        public static T DeserializeFromBinary<T>(byte[] data) where T : class
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                using (MemoryStream memoryStream = new MemoryStream(data))
                using (XmlReader xmlReader = XmlReader.Create(memoryStream))
                {
                    return serializer.ReadObject(xmlReader) as T;
                }
            }

        public static byte[] ReadISLToByte(string filepath)
        {
            

            FileStream stream = File.OpenRead(filepath);
            byte[] total = new byte[stream.Length];

            byte[] header = new byte[12];
            byte[] content = new byte[stream.Length - header.Length];

            stream.Read(header, 0, 12);
            ReadISLHeader(header);

            stream.Read(content, 0, content.Length);

            return content;
        }

        /// -------------------------------------------------------------
        /// NEW Encryption Decryption
        /// 
        /// -------------------------------------------------------------

        public static byte[] Encrypt(byte[] input)
        {
            PasswordDeriveBytes pdb =
              new PasswordDeriveBytes(PrivateKey, // Change this
              new byte[] { 0x43, 0x87, 0x23, 0x72 }); // Change this
            MemoryStream ms = new MemoryStream();
            Aes aes = Aes.Create();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            aes.Padding = PaddingMode.PKCS7;
            CryptoStream cs = new CryptoStream(ms,
              aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();
            cs.Close();
            return ms.ToArray();
        }
        public static byte[] Decrypt(byte[] input)
        {
            //Debug.Log("getting " + input.Length + "as an input");
            PasswordDeriveBytes pdb =
              new PasswordDeriveBytes(PrivateKey, // Change this
              new byte[] { 0x43, 0x87, 0x23, 0x72 }); // Change this
            MemoryStream ms = new MemoryStream();
            Aes aes = Aes.Create();
            aes.Key = pdb.GetBytes(aes.KeySize / 8);
            aes.IV = pdb.GetBytes(aes.BlockSize / 8);
            aes.Padding = PaddingMode.PKCS7;
            CryptoStream cs = new CryptoStream(ms,
              aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();
            cs.Close();
            return ms.ToArray();
        }

        /// -------------------------------------------------------------
        /// Encryption Decryption
        /// 
        /// -------------------------------------------------------------

        private static byte[] EncryptStringToBytes(string toEncrypt)
        {
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
           Aes aes = Aes.Create();
            ICryptoTransform cTransform = aes.CreateEncryptor();
            return cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            
        }

        private static string DecryptBytesToString(byte[] toDecrypt)
        {
            Aes aes = Aes.Create();
            ICryptoTransform cTransform = aes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toDecrypt, 0, toDecrypt.Length);
            return Encoding.UTF8.GetString(resultArray);
        }




        private static string EncryptDataToString(string toEncrypt)
        {
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
           Aes aes = Aes.Create();
            ICryptoTransform cTransform = aes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        private static string DecryptDataToString(string toDecrypt)
        {
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
            Aes aes = Aes.Create();
            ICryptoTransform cTransform = aes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Encoding.UTF8.GetString(resultArray);

        }

        private static Aes CreateAESManaged()
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(PrivateKey);
            var result = Aes.Create();

            var newKeysArray = new byte[16];
            Array.Copy(keyArray, 0, newKeysArray, 0, 16);

            result.Key = newKeysArray;
            result.Mode = CipherMode.ECB;
            result.Padding = PaddingMode.PKCS7;
            return result;
        }

        /// <summary>
        ///     Deserialize XML string, optionally only an inner fragment of the XML, as specified by the innerStartTag parameter.
        /// </summary>
        public static T DeserializeXml<T>(this string @this, string innerStartTag = null)
        {
            using (var stringReader = new StringReader(@this))
            {
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    if (innerStartTag != null)
                    {
                        xmlReader.ReadToDescendant(innerStartTag);
                        var xmlSerializer = CachingXmlSerializerFactory.Create(typeof(T), new XmlRootAttribute(innerStartTag));
                        return (T)xmlSerializer.Deserialize(xmlReader.ReadSubtree());
                    }
                    return (T)CachingXmlSerializerFactory.Create(typeof(T), new XmlRootAttribute("AutochartistAPI")).Deserialize(xmlReader);
                }
            }
        }

        public static byte[] MultiArrayToBytes(float[,] array)
            {
                    var buffer = new byte[array.GetLength(0) * array.GetLength(1)];
                    Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
                    return buffer;
            }

        /// <summary>
        ///     A caching factory to avoid memory leaks in the XmlSerializer class.
        /// See http://dotnetcodebox.blogspot.dk/2013/01/xmlserializer-class-may-result-in.html
        /// </summary>
        public static class CachingXmlSerializerFactory
        {
            private static readonly ConcurrentDictionary<string, XmlSerializer> Cache = new ConcurrentDictionary<string, XmlSerializer>();
            public static XmlSerializer Create(Type type, XmlRootAttribute root)
            {
                if (type == null)
                {
                    throw new ArgumentNullException(nameof(type));
                }
                if (root == null)
                {
                    throw new ArgumentNullException(nameof(root));
                }
                var key = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", type, root.ElementName);
                return Cache.GetOrAdd(key, _ => new XmlSerializer(type, root));
            }
            public static XmlSerializer Create<T>(XmlRootAttribute root)
            {
                return Create(typeof(T), root);
            }
            public static XmlSerializer Create<T>()
            {
                return Create(typeof(T));
            }
            public static XmlSerializer Create<T>(string defaultNamespace)
            {
                return Create(typeof(T), defaultNamespace);
            }
            public static XmlSerializer Create(Type type)
            {
                return new XmlSerializer(type);
            }
            public static XmlSerializer Create(Type type, string defaultNamespace)
            {
                return new XmlSerializer(type, defaultNamespace);
            }

            
        }
    }


}