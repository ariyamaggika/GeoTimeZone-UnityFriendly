using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace GeoTimeZone
{
    public class TimezoneFileReader
    {
		MemoryStream ms;
        private const int LineLength = 8;
        private const int LineEndLength = 1;


		private static readonly object Locker = new object();

		public TimezoneFileReader(byte[] data) {
			Debug.LogFormat ("DataLen: {0}", data.Length);
			ms = new MemoryStream(data);
		}

        private long GetCount()
        {
            return ms.Length/(LineLength + LineEndLength);
        }

        public long Count
        {
			get { return GetCount(); }
        }

        public  string GetLine(long line)
        {
            var index = (LineLength + LineEndLength) * (line - 1);

            var buffer = new byte[LineLength];

            lock (Locker)
            {
				ms.Position = index;
				ms.Read(buffer, 0, LineLength);   
            }

            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }
    }
}