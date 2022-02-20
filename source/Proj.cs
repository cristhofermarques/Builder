using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

public class Proj
{
    List<string> objs = new List<string>();

    public const string PJ = @"\";
    public string name = "app";

    public Dictionary<string, string> info = new Dictionary<string, string>
    {
        {"bin_name","app"},
        {"bin_type","exe"},
    };

    public string[] dbgArg;
    public string[] rlsArg;

    public string[] res;
    public string[] src;
    public string[] incDir;
    public string[] dep;

    public Proj( string projStr)
    {
        string projNameReturn = Builder.GetValues( projStr, "#proj")[0];
        if(projNameReturn != null && projNameReturn != string.Empty){ name = projNameReturn;}

        Dictionary< string, string> projInfoReturn = Builder.GetDictFromStrArray( Builder.GetValues( projStr, "#info"));
        if( projInfoReturn != null){ info = projInfoReturn;}

        dbgArg = Builder.GetValues(projStr, "#dbg_arg");
        rlsArg = Builder.GetValues(projStr, "#rls_arg");

        res = Builder.GetValues(projStr, "#res");

        incDir = Builder.GetValues(projStr, "#inc_dir");
        src = Builder.GetValues(projStr, "#src");

        dep = Builder.GetValues(projStr, "#dep");
    }

    public bool Build( string[] inUserArgs)
    {
        bool isTest = false, isRls = false;

        for( int i = 0; i < inUserArgs.Length; i++)
        {
            switch( inUserArgs[i])
            {
                case "-T":
                    isTest = true;
                break;

                case "-R":
                    isRls = true;
                break;
            }
        }

        bool buildObj = true;
        if( info != null && info.ContainsKey("bin_type") && info["bin_type"] == "exe"){ buildObj = false;}

        string buildReturn;
        if( (buildReturn = BuildResource( isRls, isTest)) != string.Empty){ Print( buildReturn); return false;}
        if( buildObj){ if( (buildReturn = BuildObjects( isRls, isTest)) != string.Empty){ Print( buildReturn); return false;}}
        if( (buildReturn = BuildBinary( isRls, isTest)) != string.Empty){ Print( buildReturn); return false;}

        if( !isTest)
            ExecCompiler( "cmd" , "/c cls");
        return true;
    }

    void Print( string msg)
    {
        Console.WriteLine( msg);
    }

    void PrintFail( bool result, string msg)
    {
        if(! result)
        {
            Console.WriteLine("[FAIL] : " + msg);
        }
    }

    int ExecCompiler( string compilerName, string args)
    {
        ProcessStartInfo info = new ProcessStartInfo( compilerName, args);
        info.UseShellExecute = false;

        Process process = Process.Start( info);
        process.WaitForExit();
        return process.ExitCode;
    }

    int ExecCompilerWithoutOutput( string compilerName, string args)
    {
        ProcessStartInfo info = new ProcessStartInfo( compilerName, args);
        info.RedirectStandardOutput = true;
        info.UseShellExecute = false;

        Process process = Process.Start( info);
        process.WaitForExit();
        return process.ExitCode;
    }

    string BuildResource( bool isRls, bool isTest)
    {
        if( res == null){ return string.Empty;}
        
        string outFolder = name + @"\bin";
        if(! Directory.Exists( outFolder)){ Directory.CreateDirectory( outFolder);}
        else
        {
            if( isRls)
            {
                outFolder += @"\rls";
                if(! Directory.Exists( outFolder)){ Directory.CreateDirectory( outFolder);}
            }
            else
            {
                outFolder += @"\dbg";
                if(! Directory.Exists( outFolder)){ Directory.CreateDirectory( outFolder);}
            }
        }

        for( int i = 0; i < res.Length; i++)
        {
            if( File.Exists( res[i]))
            {
                int lastPjLIdx = res[i].LastIndexOf(@"\");
                int lastPjRIdx = res[i].LastIndexOf(@"/");
                int lastPjIdx = lastPjLIdx > lastPjRIdx? lastPjLIdx: lastPjRIdx;
                int dotIdx = res[i].IndexOf('.') - lastPjIdx - 1;

                string resObj = outFolder + @"\" + res[i].Substring( lastPjIdx + 1, dotIdx) + ".res";
                string arg = res[i] + " -O coff -o " + resObj;

                if( !isTest)
                {
                    if( ExecCompiler("windres", arg) != 0)
                        return "[FAIL] : " + res[i];
                    
                    objs.Add( resObj);
                }
                else
                {
                    Print("windres " + arg);
                }
            }
            else{ return res[i] + " Not Exists";}
        }

        return string.Empty;
    }

    string BuildObjects( bool isRls, bool isTest)
    {
        if( src == null){ return "No Source To Build";}

        string compiler = "gcc";
        if( info != null)
        {
            if( info.ContainsKey("compiler"))
            {
                if( info["compiler"] == "g++"){ compiler = "g++";}
            }
        }
        
        string outFolder = name + @"\bin";
        if(! Directory.Exists( outFolder)){ Directory.CreateDirectory( outFolder);}
        else
        {
            if( isRls)
            {
                outFolder += @"\rls";
                if(! Directory.Exists( outFolder)){ Directory.CreateDirectory( outFolder);}
            }
            else
            {
                outFolder += @"\dbg";
                if(! Directory.Exists( outFolder)){ Directory.CreateDirectory( outFolder);}
            }
        }

        string flagStrArg = "";
        if( isRls && rlsArg != null)
        {
            for( int i = 0; i < rlsArg.Length; i++)
            {
                flagStrArg += ' ' + rlsArg[i].Replace( '|', ' ');
            }
        }
        else if( dbgArg != null)
        {
            for( int i = 0; i < dbgArg.Length; i++)
            {
                flagStrArg += ' ' + dbgArg[i].Replace( '|', ' ');
            }
        }

        string incStrArg = "";
        if( incDir != null)
        {
            for( int i = 0; i < incDir.Length; i++)
            {
                incStrArg += " -I " + incDir[i];
            }
        }

        for( int i = 0; i < src.Length; i++)
        {
            if( !File.Exists(src[i])){ return src[i] + " Not Exists";}
            {
                int lastPjLIdx = src[i].LastIndexOf(@"\");
                int lastPjRIdx = src[i].LastIndexOf(@"/");
                int lastPjIdx = lastPjLIdx > lastPjRIdx? lastPjLIdx: lastPjRIdx;
                int dotIdx = src[i].IndexOf('.') - lastPjIdx - 1;

                string srcObj = outFolder + @"\" + src[i].Substring( lastPjIdx + 1, dotIdx) + ".o";
                
                string arg = flagStrArg + ' ' + incStrArg + " -c " + src[i] + " -o " + srcObj;

                if( !isTest)
                {
                    if( ExecCompiler( compiler, arg) != 0)
                        return "[FAIL] : " + res[i];
                    
                    objs.Add( srcObj);
                }
                else
                {
                    objs.Add( srcObj);
                    Print( compiler + ' ' + arg);
                }
            }
        }

        return string.Empty;
    }

    string BuildBinary( bool isRls, bool isTest)
    {
        string outFolder = name + @"\bin";
        if(! Directory.Exists( outFolder)){ Directory.CreateDirectory( outFolder);}
        else
        {
            if( isRls)
            {
                outFolder += @"\rls";
                if(! Directory.Exists( outFolder)){ Directory.CreateDirectory( outFolder);}
            }
            else
            {
                outFolder += @"\dbg";
                if(! Directory.Exists( outFolder)){ Directory.CreateDirectory( outFolder);}
            }
        }

        string depStrArg = "";
        if( dep != null)
        {
            for( int i = 0; i < dep.Length; i++)
            {
                if( dep[i][0] == '-')
                {
                    depStrArg += " -l" + dep[i].Substring( 1);
                }
                else
                {
                    if( !File.Exists( dep[i])){ return dep[i] + " Nor Exists";}
                    else
                    {
                        if( ExecCompilerWithoutOutput("cmd", "/c copy " + dep[i] + ' ' + outFolder) != 0){ return dep[i] + "Copy Fail";}

                        int lastPjLIdx = dep[i].LastIndexOf(@"\");
                        int lastPjRIdx = dep[i].LastIndexOf(@"/");
                        int lastPjIdx = lastPjLIdx > lastPjRIdx? lastPjLIdx: lastPjRIdx;

                        string depNewCopy = outFolder + @"\" + dep[i].Substring( lastPjIdx + 1);
                        depStrArg += ' ' + depNewCopy;
                    }
                }
            }
        }

        string compiler = "gcc";
        if( info != null && info.ContainsKey("compiler") && info["compiler"] == "g++"){ compiler = "g++";}

        if( info != null && info.ContainsKey( "bin_type") && info["bin_type"] == "dll")
        {
            // Dll
            string flagStrArg = "";
            if( isRls && rlsArg != null)
            {
                for( int i = 0; i < rlsArg.Length; i++)
                {
                    flagStrArg += ' ' + rlsArg[i].Replace( '|', ' ');
                }
            }
            else if( dbgArg != null)
            {
                for( int i = 0; i < dbgArg.Length; i++)
                {
                    flagStrArg += ' ' + dbgArg[i].Replace( '|', ' ');
                }
            }

            string objStrArg = "";
            for(int i = 0; i < objs.Count; i++) 
            {
                objStrArg += ' ' + objs[i];
            }

            if( info.ContainsKey("bin_name")){ name = info["bin_name"];}

            string arg = ' ' + flagStrArg + " -shared -fpic " + objStrArg + depStrArg + " -o " + outFolder + @"\" + name + ".dll -Wl,--out-implib," + outFolder + @"\lib" + name + ".a";

            if( isTest){ Print( compiler + arg);}
            else
            {
                if( ExecCompiler( compiler, arg) != 0)
                    return "[FAIL] : " + name + ".dll";
            }
        }
        else
        {
            // Recompile Direct To exe
            string flagStrArg = "";
            if( isRls && rlsArg != null)
            {
                for( int i = 0; i < rlsArg.Length; i++)
                {
                    flagStrArg += ' ' + rlsArg[i].Replace( '|', ' ');
                }
            }
            else if( dbgArg != null)
            {
                for( int i = 0; i < dbgArg.Length; i++)
                {
                    flagStrArg += ' ' + dbgArg[i].Replace( '|', ' ');
                }
            }

            string incStrArg = "";
            if( incDir != null)
            {
                for( int i = 0; i < incDir.Length; i++)
                {
                    incStrArg += " -I " + incDir[i];
                }
            }

            string arg = flagStrArg + ' ' + incStrArg;

            for( int i = 0; i < src.Length; i++)
            {
                if( !File.Exists(src[i])){ return src[i] + " Not Exists";}
                {
                    arg += ' ' + src[i];
                }
            }

    
            for(int i = 0; i < objs.Count; i++) 
            {
                if( objs[i].Contains(".res"))
                {
                    arg += ' ' + objs[i];
                }
            }

            if( info.ContainsKey("bin_name")){ name = info["bin_name"];}

            arg += ' ' + depStrArg;
            arg += " -o " + outFolder + @"\" + name + ".exe";

            if( isTest){ Print( compiler + arg);}
            else
            {
                if( ExecCompiler( compiler, arg) != 0)
                        return "[FAIL] : " + name+ ".exe";
            }
        }
        return string.Empty;
    }
}