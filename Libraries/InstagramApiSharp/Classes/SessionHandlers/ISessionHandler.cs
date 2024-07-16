using InstagramApiSharp.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace InstagramApiSharp.Classes.SessionHandlers
{
    public interface ISessionHandler
    {
        IInstaApi InstaApi { get; set; }

        /// <summary>
        ///     Path to file
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        ///     Load and Set StateData to InstaApi
        /// </summary>
        /// <param name="useBinaryFormatter">Use BinaryFomatter, Not suggested!!!!
        /// <para> P.S: This is only for backward compatibility</para>
        /// </param>
        void Load(bool useBinaryFormatter = true);

        /// <summary>
        ///     Save current StateData from InstaApi
        /// </summary>
        /// <param name="useBinaryFormatter">Use BinaryFomatter, Not suggested!!!!
        /// <para> P.S: This is only for backward compatibility</para>
        void Save(bool useBinaryFormatter = true);
    }
}
