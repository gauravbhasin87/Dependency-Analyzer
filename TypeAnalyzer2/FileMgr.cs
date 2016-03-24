///////////////////////////////////////////////////////////////////////
// FileMgr.cs.cs - Module for extracting files in the given directory//
//                  and its subdirectories(if option provided)       //
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
 *This package contains the application independent code for retrieving 
 *list of files present in the given subdirectories.
 *
 * Public Interface:
 * ------------------
 * 
 * public void findFiles(string path, bool recurse, List<string> patterns)
 *      - gives back the files as List<string>
 * 
 *  
 */
/* Required Files:
 *   -None
 *   
 * Build command:
 *   
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ManagedCodeAnalyzer
{
    public class FileMgr
    {
        private List<string> files = new List<string>();
        //private List<string> patterns = new List<string>();
        //private bool recurse = true;

        public void findFiles(string path, bool recurse, List<string> patterns)
        {
            try
            {
                //Console.WriteLine("Path in findfiles(): " + path+"\n");
                foreach (string pattern in patterns)
                {
                    string[] newFiles = Directory.GetFiles(path, pattern);
                    /*  for (int i = 0; i < newFiles.Length; ++i)
                          newFiles[i] = Path.GetFullPath(newFiles[i]); */

                    files.AddRange(newFiles);
                    /*  foreach (string file in files)
                          Console.WriteLine("\n  {0}", file+"\n");*/
                }
                if (recurse)
                {
                    string[] dirs = Directory.GetDirectories(path);
                    foreach (string dir in dirs)
                        findFiles(dir, recurse, patterns);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

       /* Not needed here anymore
        * public void addPattern(string pattern)
        {
            patterns.Add(pattern);
        }
        */
        public List<string> getFiles()
        {
            return files;
        }

#if(TEST_FILEMGR)
        static void Main(string[] args)
        {
            Console.Write("\n  Testing FileMgr Class");
            Console.Write("\n =======================\n");
            List<string> testList = new List<string>();
            testList.Add("*.cs");
            FileMgr fm = new FileMgr();
            
            fm.findFiles("..\\..\\..\\..\\..\\Project2HelpF14\\Parser\\Parser", true,testList );                   //Handle exception
            List<string> files = fm.getFiles();
            foreach (string file in files)
                Console.Write("\n  {0}", file);
            Console.Write("\n\n");
            
        }
#endif
    }
}
