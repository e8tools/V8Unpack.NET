/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
static void EnumerateFiles(E8Tools.V8Unpack.Container c, string align = "")
{

    byte[] buffer = new byte[10240];
    MemoryStream memoryStream = new(buffer);

    foreach (var item in c.Files())
    {
        Array.Fill<byte>(buffer, 0);

        using (var fileStream = item.GetStream())
        {
            var result = fileStream.Read(buffer, 0, buffer.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);

        }
        
        Console.WriteLine($"{align}{item.Name}");

        var isContainer = E8Tools.V8Unpack.Container.IsContainer(memoryStream);
        if (isContainer)
        {
            using (var fileStream = item.GetStream())
            using (var copy = new MemoryStream())
            {
                fileStream.CopyTo(copy);
                fileStream.Close();
                var inner = E8Tools.V8Unpack.Container.FromStream(copy);
                EnumerateFiles(inner, $"{align}\t");
            }
        }
    }
}

var c = E8Tools.V8Unpack.Container.FromFile(args[0]);
EnumerateFiles(c);
