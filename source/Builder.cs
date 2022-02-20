using System;
using System.IO;
using System.Collections.Generic;

public partial class Builder
{
    bool fileReadOk = false, invalid = false;
    string buildFileName = "";
    public List<Proj> projs;

    public Builder( string buildFilePath = "build.txt")
    {

        buildFileName = buildFilePath;
        string buildStr;
        if(! (fileReadOk = TryGetBuildFile( out buildStr, buildFilePath))){ return;}
        
        if( ! buildStr.Contains("#proj")){ Console.WriteLine("No '#proj' Tag Defined in " + buildFileName); invalid = true; return;}

        List<string> projsStr = GetProjStr( buildStr);
        if( projsStr != null && projsStr.Count > 0)
            GetProjs( projsStr.ToArray());
    }

    public void Build(string[] inUserArgs)
    {
        if( invalid){ return;}
        if( projs == null || projs.Count < 0){ return;}
        if(! fileReadOk){ Console.WriteLine( buildFileName + " Not Finded"); return;}

        if( inUserArgs == null){ inUserArgs = new string[]{""};}

        List<Proj> listToBuild = new List<Proj>();

        for( int i = 0; i < projs.Count; i++)
        {
            for( int a = 0; a < inUserArgs.Length; a++)
            {
                if( inUserArgs[a][0] == '-' && projs[i].name == inUserArgs[a].Substring(1))
                {
                    listToBuild.Add(projs[i]);
                }
            }
        }

        if( listToBuild.Count > 0)
        {    
            for( int i = 0; i < listToBuild.Count; i++)
            {
                listToBuild[i].Build( inUserArgs);
            }
        }
        else
        {
            for( int i = 0; i < projs.Count; i++)
            {
                projs[i].Build( inUserArgs);
            }
        }
    }
    public static string[] GetValues(string str, string fieldName)
    {
        if (str.Contains( fieldName))
        {
            List<string> values = new List<string>();
            string[] splitedStr = str.Split(new char[]{' ','\n'});

            bool isInRightField = false;
            for(int i = 0; i < splitedStr.Length; i++)
            {
                splitedStr[i] = splitedStr[i].Trim();
                if( splitedStr[i] != string.Empty)
                {
                    if( splitedStr[i][0] == '#')
                    {
                        isInRightField = splitedStr[i] == fieldName;
                    }
                    if( isInRightField && splitedStr[i] != fieldName)
                    {
                        if(! values.Contains( splitedStr[i]))
                            values.Add( splitedStr[i]);
                    }    
                }
            }

           return values.ToArray();
        }
        return null;
    }
    private bool TryGetBuildFile( out string dataStr, string dataName = "build.txt")
    {
        dataStr = string.Empty;
        if (! File.Exists(dataName)) { Console.WriteLine(dataName + " Not Finded"); return false;}

        StreamReader file = new StreamReader(dataName);
        if (file == null) { return false; }

        dataStr = file.ReadToEnd();
        file.Close();
        return true;
    }
    private List<string> GetProjStr( string buildFileStr)
    {
        if(! buildFileStr.Contains("#proj"))
            return null;

        List<int> projFlagIdxList = new List<int>();
        int lastIdx = 0;
        while ((lastIdx = buildFileStr.IndexOf("#proj", lastIdx)) >= 0)
        {
            lastIdx++;
            projFlagIdxList.Add(lastIdx - 1);
        }

        projFlagIdxList.Add( buildFileStr.Length);

        List<string> projsStrList = new List<string>();
        for (int i = 0; i < projFlagIdxList.Count - 1; i++)
        {
            int startIdx = projFlagIdxList[i];
            int strLen = projFlagIdxList[i + 1] - projFlagIdxList[i];
            string projStr = buildFileStr.Substring(startIdx, strLen);
            projsStrList.Add( projStr);
        }
        return projsStrList;
    }
    public static Dictionary<string, string> GetDictFromStrArray( string[] str)
    {
        if( str == null){ return null;}

        Dictionary<string, string> dict = new Dictionary<string, string>();

        foreach (string pair in str)
        {
            if (pair.Contains(":"))
            {
                string[] splitedpair = pair.Split(':');
                if(! dict.ContainsKey( splitedpair[0].Trim()))
                    dict.Add(splitedpair[0].Trim(), splitedpair[1].Trim());
            }
        }

        return dict;
    }
    private string GetBuildFileHeader( string buildFileStr)
    {   
        if(! buildFileStr.Contains("#proj")){ return string.Empty;}

        return buildFileStr.Substring(0, buildFileStr.IndexOf("#proj"));

    }
    private void GetProjs( string[] projsStr)
    {
        projs = new List<Proj>();
        for(int i = 0; i < projsStr.Length; i++)
        {
            var proj = new Proj( projsStr[i]);
            if(! projs.Contains( proj))
                projs.Add( proj);
        }
    }
}   