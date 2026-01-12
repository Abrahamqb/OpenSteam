using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpenSteam.Service
{
    public class Attach
    {
        public void PatchSteam(string path, bool Delet)
        {
            if (Delet)
            {
                if (File.Exists(Path.Combine(path, "xinput1_4.dll")))
                {
                    File.Delete(Path.Combine(path, "xinput1_4.dll"));
                }
                if (File.Exists(Path.Combine(path, "hid.dll")))
                {
                    File.Delete(Path.Combine(path, "hid.dll"));
                }
                NotificationWindow win = new NotificationWindow("¡Unpatched Steam!", 2);
                win.Show();
            }
            else
            {
                if (Directory.Exists(path))
                {
                    byte[] File1 = Properties.Resources.xinput1_4;
                    File.WriteAllBytes(Path.Combine(path, "xinput1_4.dll"), File1);
                    byte[] File2 = Properties.Resources.hid;
                    File.WriteAllBytes(Path.Combine(path, "hid.dll"), File2);
                    NotificationWindow win = new NotificationWindow("¡Steam Patched!", 2);
                    win.Show();
                }
            }
            
        }
    }
}
