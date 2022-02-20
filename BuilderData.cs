using System;
using System.IO;
using System.Collections.Generic;

public struct BuilderData
{
    public Dictionary< string, string> settings; 

    public BuilderData( string dataStr)
    {
        settings = new Dictionary<string, string>();
    }
}
