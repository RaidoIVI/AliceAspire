using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InstagramApiSharp.Classes.SessionHandlers
{
    public class FileSessionHandler : ISessionHandler
    {

        public IInstaApi InstaApi { get; set; }

        /// <summary>
        ///     Path to file
        /// </summary>
        /// <param name="useBinaryFormatter">Use BinaryFomatter, Not suggested!!!!
        /// <para> P.S: This is only for backward compatibility</para>
        /// </param>
        public string FilePath { get; set; }


        /// <summary>
        ///     Load and Set StateData to InstaApi
        /// </summary>
        public void Load(bool useBinaryFormatter = true)
        {

            if (File.Exists(FilePath))
            {
                if (useBinaryFormatter)
                {
                    using (var fs = File.OpenRead(FilePath))
                    {
                        InstaApi.LoadStateDataFromStream(fs);
                    }
                }
                else
                {
                    var state = File.ReadAllText(FilePath);
                    var decoded = CryptoHelper.Base64Decode(state);
                    InstaApi.LoadStateDataFromString(decoded);
                }
            }
        }

        /// <summary>
        ///     Save current StateData from InstaApi
        /// </summary>
        /// <param name="useBinaryFormatter">Use BinaryFomatter, Not suggested!!!!
        /// <para> P.S: This is only for backward compatibility</para>
        /// </param>
        public void Save(bool useBinaryFormatter = true)
        {
            if (useBinaryFormatter)
            {
                SaveMe(InstaApi.GetStateDataAsStream());
            }
            else
            {
                var b64 = CryptoHelper.Base64Encode(InstaApi.GetStateDataAsString());
                var data = Encoding.UTF8.GetBytes(b64);
                SaveMe(new MemoryStream(data));
            }
            void SaveMe(Stream stream)
            {
                using (var fileStream = File.Create(FilePath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
