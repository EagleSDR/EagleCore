using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EagleWeb.Launcher.Misc
{
    class DataFile<T>
    {
        public DataFile(string filename, T defaultValue)
        {
            this.filename = filename;
            if (File.Exists(filename))
                data = JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
            else
                data = defaultValue;
        }

        private string filename;
        private T data;

        public T Data => data;

        public void Save()
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(data));
        }
    }
}
