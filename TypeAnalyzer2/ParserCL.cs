///////////////////////////////////////////////////////////////////////
// ParserCL.cs - Parse command line arguments                        //
//                                                                   //
//                                                                   //                                                                  
// ver 1.0                                                           //
// Language:    C#, .Net Framework 4.5                               //
// Platform:    Sony Vaio, Win8.1, SP1                               //
// Application: Demonstration for CSE681, Project #2, Fall 2014      //
// Author:      Gaurav Bhasin, Syracuse University                   //
//              (315) 744-5233, gabhasin@syr.edu                     //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package takes the commandline arguments and split them into path, options and patterns
 * 
 * Public Interface:
 * ------------------
 *      public static void ParseCommandline(string[] args, out string path, out List<String> pat, out bool rec, out bool relation, out bool opToFile)//
 *                                              this method returns path, options and patterns using out parameters in method arguments.
 *  
 */
/* Required Files:
 *   -Repository.cs, Analyser.cs, DisplayRepository.cs
 *   
 *
 *   
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace ManagedCodeAnalyzer
{
    public class InvalidCmdLineException :Exception
    {
        public InvalidCmdLineException() : base("Invalid Command Line Argruments!!") { }
    }

    public class ParserCL
    {
        static private List<string> patterns;
        static string apath;
        static bool recurse;
        static bool relationship;
        static bool xmlOutput;
        static ParserCL()
        {      
            patterns = new List<string>();           
        }
        public static void ParseCommandline(string[] args, out string path, out List<String> pat, out bool rec, out bool relation, out bool opToFile)
        {
            path = "";
            pat = patterns;
            rec = false;
            relation = false;
            opToFile = false;
            if (args.Length == 0)
            {
                Console.Write("\n  Please enter path and patterns to analyze\n\n");               
                try
                {
                    throw new InvalidCmdLineException();
                }
                catch(InvalidCmdLineException ex)
                {
                    Console.WriteLine(ex.Message);
                    Environment.Exit(0);
                }
                //what to do if args are zero
            }

                Regex regex1 = new Regex(@"\A/S{1}\Z");
                Regex regex2 = new Regex(@"\A/R{1}\Z");
                Regex regex3 = new Regex(@"\A/X{1}\Z");

                
                for (int i = 0; i < args.Length; i++)
                {
                    Match match1 = regex1.Match(args[i]);
                    Match match2 = regex2.Match(args[i]);
                    Match match3 = regex3.Match(args[i]);
                    if(match1.Success)
                    {
                        recurse = true;
                        rec = recurse;
                       // Console.WriteLine(i+"  "+ args[i]+"    "+ recurse);
                    }
                    else if (match2.Success)
                    {
                        relationship = true;
                        relation = relationship;
                       // Console.WriteLine(i + "  " + args[i] + "    " + relation);
                    }
                    else if (match3.Success)
                    {
                        xmlOutput = true;
                        opToFile = xmlOutput;
                        // Console.WriteLine(i + "  " + args[i] + "    " + relation);
                    }
                    else if (args[i].Contains('\\'))                    // || ((args[i].Contains("/") && !((match1.Success) || (match2.Success))))
                    {
                        apath = Path.GetFullPath(args[i]);
                        path = apath;
                       // Console.WriteLine("path in parcercl " + path);
                    }
                    else if(args[i].Contains('.'))
                    {
                        patterns.Add(args[i]);
                    }
                    else
                    {
                        try 
                        {
                            throw new InvalidCmdLineException();
                        }
                        catch(InvalidCmdLineException ex)
                        {
                            Console.WriteLine(ex.Message);
                            Environment.Exit(0);
                        }
                    }
                   // Console.WriteLine("Pattern count :" + patterns.Count);
                    pat = patterns;          
            }
                    if (patterns.Count == 0)
                         patterns.Add("*.*");      
            }

        public static void ShowCommandLine(string[] args)
        {
            Console.Write("\n  Commandline args are:\n");
            foreach (string arg in args)
            {
                Console.Write("  {0}", arg);
            }
            Console.Write("\n\n  current directory: {0}", System.IO.Directory.GetCurrentDirectory());
            Console.Write("\n\n");
        }

    }

    
    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        //string[] arg = { "..\..", ".cs", "/S" };
    //        string path;
    //        List<String> pat;
    //        bool rec;
    //        bool relation;
    //        bool opToFile;
    //        ParserCL.ShowCommandLine(args);
    //        ParserCL.ParseCommandline(args, out path, out pat, out rec, out relation, out opToFile);
    //        Console.WriteLine("Path: "+path);
    //        foreach(string str in pat)
    //        {
    //            Console.WriteLine(str);
    //        }
    //    }
    //}
}
