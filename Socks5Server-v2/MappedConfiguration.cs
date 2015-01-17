using System;
using System.IO;
using System.Xml.Serialization;

namespace Socks5S
{
    public class MappedConfiguration<T>
    {

        public T Data { get; private set; }

        public void Deserialize(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamReader reader = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
            {
                this.Data = (T)serializer.Deserialize(reader);
                reader.Close();
            }
        }

    }
}
