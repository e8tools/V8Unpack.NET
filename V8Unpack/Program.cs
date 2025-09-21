/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
void Usage()
{
    Console.WriteLine("V8Unpack command params");
    Console.WriteLine("Commands:");
    Console.WriteLine("\t-unpack SRC DSTDIR    - one-level unpack with inflate");
    Console.WriteLine("\t-parse  SRC DSTDIR    - recursive unpack");
}

void RecursiveParse(E8Tools.V8Unpack.Container Cf, string destDir, bool showProgress = false)
{
    var progress = 0;

    Directory.CreateDirectory(destDir);
    foreach (var file in Cf.Files())
    {
        var dstPath = Path.Combine(destDir, file.Name);
        var tmpPath = dstPath + ".tmp";

        if (showProgress) Progress(ref progress);

        using var fileStream = file.GetStream();
        using var output = new FileStream(tmpPath, FileMode.Create);
        fileStream.CopyTo(output);
        output.Close();

        using var input = new FileStream(tmpPath, FileMode.Open);
        if (E8Tools.V8Unpack.Container.IsContainer(input))
        {
            var innerCf = E8Tools.V8Unpack.Container.FromStream(input);
            RecursiveParse(innerCf, dstPath);

            input.Close();
            File.Delete(tmpPath);
        }
        else
        {
            input.Close();
            if (File.Exists(dstPath)) {
                File.Delete(dstPath);
            }
            File.Move(tmpPath, dstPath);
        }
    }
}

void Progress(ref int progress)
{
    string[] progressChars = ["|", "/", "-", "\\", "|", "/", "-", "\\"];
    Console.Write($"\b{progressChars[progress]}");
    progress = (progress + 1) % progressChars.Length;
}

void Parse()
{
    if (args.Length < 2)
    {
        Usage();
        return;
    }
    var filename = args[1];
    var destDir = args[2];

    var Cf = E8Tools.V8Unpack.Container.FromFile(filename);
    RecursiveParse(Cf, destDir, true);

    Console.WriteLine("\bDone.");
}

void Unpack()
{
    if (args.Length < 2)
    {
        Usage();
        return;
    }
    
    var filename = args[1];
    var destDir = args[2];
    
    Directory.CreateDirectory(destDir);

    var Cf = E8Tools.V8Unpack.Container.FromFile(filename);
    foreach (var file in Cf.Files())
    {
        var dstPath = Path.Combine(destDir, file.Name);

        using var fileStream = file.GetStream();
        using var output = new FileStream(dstPath, FileMode.Create);

        fileStream.CopyTo(output);

    }
    Console.WriteLine("Done.");
}

if (args ==  null || args.Length < 1)
{
    Usage();
}
else
{
    var command = args[0];
    if (string.Equals(command, "-parse", StringComparison.InvariantCultureIgnoreCase))
    {
        Parse();
    }
    else if (string.Equals(command, "-unpack", StringComparison.InvariantCultureIgnoreCase))
    {
        Unpack();
    }
    else
    {
        Usage();
    }
}