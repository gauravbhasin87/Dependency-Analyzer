///////////////////////////////////////////////////////////////////////
// ManagedCodeAnalyser.cs - Application specific code file           //
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
 * This package contains the main method from where the Application starts. It communicates between various packages and passes data between them needed for 
 *processing.
 *
 * Public Interface:
 * ------------------
 *      
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
using RepositoryNS;
using DisplayPkg;


//Executor Package
namespace ManagedCodeAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            
            List<string> patterns;
            string path;
            bool recurse;
            bool relationship;
            List<string> files;
            bool opToFile;

            //parse commandline arguments 
            ManagedCodeAnalyzer.ParserCL.ParseCommandline(args, out path, out patterns, out recurse, out relationship, out opToFile);

            Console.WriteLine("Path: "+path + " Recurse: " + recurse + " Relationship: " + relationship + "Output to XML File: "+ opToFile );

            foreach (string str in patterns)
            {
                Console.WriteLine("Pattern: "+str);
            }


            //create FileMgr instance
            FileMgr fManager = new FileMgr();

            fManager.findFiles(path, recurse, patterns);
            files = fManager.getFiles();
            /*
            foreach (string file in files)
                Console.WriteLine("File :"+files);*/
            RepositoryNS.Repository<Elem>[] repoElemArray = Analyzer.doParse(files);
            Repository<ElemForRel> repoElemForRel = Analyzer.doAnalyse(files);

            //Diplay results
            DisplayRepository.displayTypeAnalysis(repoElemArray,true);
            DisplayRepository.displayRelationAnalysis(repoElemForRel,relationship);
            if(opToFile)
                DisplayRepository.xmlOutput(repoElemArray, repoElemForRel, recurse, relationship);
            

        }
    }
}
