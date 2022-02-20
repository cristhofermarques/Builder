using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        bool defBuildFile = true, fileInArgs = false;
        List<string> buildFiles = new List<string>();

        for(int i = 0; i < args.Length; i++)
        {
            if( args[i] == "-F"){ fileInArgs = true;}
            if( fileInArgs && File.Exists( args[i] + ".txt"))
            {
                buildFiles.Add( args[i] + ".txt");
                defBuildFile = false;
            }
            
        }

        for(int i = 0; i < buildFiles.Count; i++)
        {
            new Builder( buildFiles[i]).Build( args);
        }

        if( defBuildFile){ new Builder().Build( args);}
    }
}