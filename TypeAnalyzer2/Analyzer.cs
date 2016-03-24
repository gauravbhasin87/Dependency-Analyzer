///////////////////////////////////////////////////////////////////////
// Analyzer.cs - Module for parsing each files and give back the     //
//             the analysis information back to Application Executor //
//             via pasing Repository information.                    // 
// Analzer takes each file in the file list provided by FileMgr and  // 
//passes it to Parse for parsing requires for Type and Relationship  //
//analysis.                                                          //
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
 *This Package builds Parser and Analyzer with necessary rules and
 *actions to parse Type and function information and also analyse 
 *the given files set to find relationship between types in them.
 *
 * Public Interface:
 * ------------------
 *  public BuildParser(Repository<Elem> rep, CSsemi.CSemiExp semi) //retrieves Repository and semi-expression for bulding Parser
 *  public virtual Parser build()  //constructs parser object                                                                                        
 *  public BuildAnalyzer(Repository<ElemForRel> repo, CSsemi.CSemiExp semi, Repository<Elem>[] repoElem, string fileName, int i,List<string> files )
 *                                                                     // takes in the list of repositories created by parsing files(in doParse),list of files
 *                                                                     //current files and repsitory in execution to build analyzer
 *  public static Repository<Elem>[] doParse(List<string> files)//  parse each file and check rules on each semi-expression 
 *  public static Repository<ElemForRel> doAnalyse(List<string> files)// analyse files by testing rules defined for finding relationship between types, on each 
 *                                                                                                                                                  semi-expression
 *  
 * 
 *  
 */
/* Required Files:
 *   -Parser.cs, RulesAndActions.cs, Repository.cs, Semi.cs
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
using RepositoryNS;
using RulesAndActions;
using ParserPkg;




namespace ManagedCodeAnalyzer
{
   public class Analyzer
    {
        private static List<string> fileSet;
        static Repository<Elem>[] repoArray;
        static Repository<ElemForRel> repoForRel;

        public static Repository<Elem>[] RepoArray { get { return repoArray; } set { repoArray = value; } }
       
        static public void getFiles(List<string> files)
        {
            fileSet = files;
        }


       //Parse 1 - extracts types and their functions and saves it in the repository
       public static Repository<Elem>[] doParse(List<string> files)
        {
           // Console.Write("\n  Demonstrating Parser");
           // Console.Write("\n ======================\n");

            repoArray = new Repository<Elem>[files.Count];
            int i = 0;
            
            foreach (object file in files)
            {
               // Console.Write("\n  Processing file {0}\n", file as string);
                repoArray[i] = new Repository<Elem>(file as string);

                //implement to extract file name from path    --required
                CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                semi.displayNewLines = false;
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    return null;
                }
               // Console.Write("\n  Type and Function Analysis");
               //Console.Write("\n ----------------------------\n");
                BuildParser builder = new BuildParser(repoArray[i], semi);
                Parser parser = builder.build();
                try
                {
                    while (semi.getSemi())
                        parser.parse(semi);
                  //  Console.Write("\n\n  locations table contains:\n\n\n");
                }
                catch (Exception ex)    
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }            
                semi.close();
                
                i++;
            }
            return repoArray;
        }

       //Parse 2 -  anaylse the given list of files and retrieves the relationships between types and saves relations in repository
       public static Repository<ElemForRel> doAnalyse(List<string> files)
       {
           //Console.Write("\n  Demonstrating Analyser");
           //Console.Write("\n ======================\n");

           repoForRel = new Repository<ElemForRel>();
           int i = 0;

           foreach (object file in files)
           {
               //Console.Write("\n  Processing file {0}\n", file as string);

               //implement to extract file name from path    --required

               CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
               semi.displayNewLines = false;
               if (!semi.open(file as string))
               {
                   Console.Write("\n  Can't open {0}\n\n", file);
                   return null;
               }

              // Console.Write("\n  Relation Analysis");
              // Console.Write("\n ----------------------------\n");
               BuildAnalyzer builder2 = new BuildAnalyzer(repoForRel, semi, repoArray, file as string,i,files );
               Parser parser2 = builder2.build2();

               try
               {
                   while (semi.getSemi())
                       parser2.parse(semi);
 //                  Console.Write("\n\n  locations table contains:\n\n\n");
               }
               catch (Exception ex)
               {
                   Console.Write("\n\n  {0}\n", ex.Message);
               }
               //  Repository rep = Repository.getInstance();
               semi.close();
               //  repoArray[1].attachedFile = null;
               i++;
           }

           return repoForRel;
          
       }


        //static void Main(string[] args)
        //{
        //    //string path = "../../";
        //    List<string> patterns = new List<string>();
        //   // patterns.Add("*.cs");
        //   // string[] files = Analyzer.getFiles(path, patterns);
        //    //doAnalysis(files);
        //}
    }
   
    // build parser with rules and actions defined in RulesAndAction.cs
   public class BuildParser
   {
       Repository<Elem> repo;

       public BuildParser(Repository<Elem> rep, CSsemi.CSemiExp semi)
       {
           repo = rep;
           repo.semi = semi;
       }
       public virtual Parser build()
       {
           Parser parser = new Parser();

           // decide what to show
           AAction.displaySemi = false;
           AAction.displayStack = false;  // this is default so redundant

           // action used for namespaces, classes, and functions
           PushStack push = new PushStack(repo);

           // capture namespace info
           DetectNamespace detectNS = new DetectNamespace();
           detectNS.add(push);
           parser.add(detectNS);

           // capture class info\
           DetectClass detectCl = new DetectClass();
           detectCl.add(push);
           parser.add(detectCl);

           // capture function info
           DetectFunction detectFN = new DetectFunction();
           detectFN.add(push);
           parser.add(detectFN);

           // handle entering anonymous scopes, e.g., if, while, etc.
           DetectAnonymousScope anon = new DetectAnonymousScope();
           anon.add(push);
           parser.add(anon);

           // handle leaving scopes
           DetectLeavingScope leave = new DetectLeavingScope();
           PopStack pop = new PopStack(repo);
           leave.add(pop);
           parser.add(leave);

           //capture braceless scopes
           DetectBracelessScopes bScope = new DetectBracelessScopes();
           bScope.add(push);
           bScope.add(pop);
           parser.add(bScope);



           // parser configured
           return parser;
       }
   }

   // build parser with rules and actions defined in RulesAndAction.cs
   public class BuildAnalyzer
    {
        Repository<ElemForRel> repoForRel;
        Repository<Elem>[] repositoryArray;
        string fileName;
        int currentIteration;
        List<string> fileset;

        public BuildAnalyzer(Repository<ElemForRel> repo, CSsemi.CSemiExp semi, Repository<Elem>[] repoElem, string fileName, int i,List<string> files )
        {
            repoForRel = repo;
            repoForRel.semi = semi;
            repositoryArray = repoElem;
            this.fileName = fileName;
            this.currentIteration = i;
            fileset = files;
        }

        public virtual Parser build2()
        {
            Parser parser2 = new Parser();

            AAction.displaySemi = false;
            AAction.displayStack = false;  // this is default so redundant
            
            //Action for relationship analysis
            PushRelation pRel = new PushRelation(repoForRel, fileName);
            
            //handling inheritance relationship
            DetectInheritance detectInh = new DetectInheritance(repositoryArray);
            detectInh.add(pRel);
            parser2.add(detectInh);

            //handling Composition Inheritance of struct
            DetectComposition detectCmp = new DetectComposition(repositoryArray, currentIteration, fileset);
            detectCmp.add(pRel);
            parser2.add(detectCmp);

            //handling Aggregation
            DetectAggregation detectAggr = new DetectAggregation(repositoryArray, currentIteration);
            detectAggr.add(pRel);
            parser2.add(detectAggr);

            //handling using
            DetectUsing detectUse = new DetectUsing(repositoryArray, currentIteration);
            detectUse.add(pRel);
            parser2.add(detectUse);

            return parser2;
        }

    }
    

}


//might need 

//Console.Write("*************************************************");
//Console.Write("**************RepoArray Test*********************");
//Console.Write("*************************************************");
//int j = 0;
/*foreach (Repository rep in repoArray)
{
    Console.WriteLine("*************************************************");
    Console.WriteLine(j+". File name : " + rep.attachedFile);
    foreach (Elem e in rep.locations)
        Console.Write("\n  {0,15}, {1,25}, {2,5}, {3,5}, {4,5}", e.type, e.name, e.begin, e.end, e.funcComplexity);
    j++;
}*/




