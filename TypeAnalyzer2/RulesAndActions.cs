using CSsemi;
using RepositoryNS;
using ScopeStack;
///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.1                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Gaurav Bhasin, Syracuse University                   //
//              (315) 744-5233, gabhasin@syr.edu                     //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 * 
 * Rules for relationship analysis are:
 *  -DetectAggregation
 *  -DetectInheritence
 *  -DetectComposition
 *  -DetectUsing
 *  
 * Actions for Types analysis:
 *  -PushStack
 *  -PopStack
 *  
 * Actions for Relationship analysis
 *  -PushRelation
 *  
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 *   
 * Actions are used for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 *   
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RulesAndActions
{

    //////////////////////////////////////////////////////////
    //store identfied relation between types in repository

    public class PushRelation : AAction
    {
        Repository<ElemForRel> repoRel;
        string fileName;
        //constructor
        public PushRelation(Repository<ElemForRel> repo, string fileName)
        {
            repoRel = repo;
            this.fileName = fileName;

        }

        public override void doAction(CSemiExp semi)
        {
            ElemForRel e = new ElemForRel();
            int index = fileName.LastIndexOf("\\");
            string fName = fileName.Substring(index+1,fileName.Length-1-index);
            e.fileName = fName;
            e.Relation = semi[0];
            e.type1 = semi[1];
            e.type1Name = semi[2];
            e.text = semi[3];
            e.fileName2 = semi[4];
            e.type2 = semi[5];
            e.type2Name = semi[6];
            e.lineNumber = repoRel.semi.lineCount-1;
            //check for duplicacy
            int flag = 0;
            foreach(ElemForRel el in repoRel.locations) 
                if(e.fileName.Equals(el.fileName) && e.Relation.Equals(el.Relation) && e.type1.Equals(el.type1) && e.type1Name.Equals(el.type1Name) && e.type2.Equals(el.type2Name))
                        flag = 1;

            if (flag == 0)
                repoRel.locations.Add(e);


        }
    }


    /////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope

    public class PushStack : AAction
    {
        Repository<Elem> repo_;


        public PushStack(Repository<Elem> repo)
        {
            repo_ = repo;
            countScopes = false;

        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.begin = repo_.semi.lineCount - 1;
            elem.end = 0;
            repo_.stack.push(elem);
            if (elem.type == "function")
                countScopes = true;

            if (elem.type == "control" || elem.name == "anonymous" || elem.type == "bracelesScope")
            {
                if (countScopes)
                {
                    noOfScopes++;
                }
                if (AAction.displaySemi)
                {
                    Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
                    Console.Write("entering ");
                    string indent = new string(' ', 2 * repo_.stack.count);
                    Console.Write("{0}", indent);
                    this.display(semi); // defined in abstract action
                }

                return;

            }
            repo_.locations.Add(elem);

            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }
            if (AAction.displayStack)
                repo_.stack.display();
        }
    }
    /////////////////////////////////////////////////////////
    // pops scope info from stack when leaving scope

    public class PopStack : AAction
    {
        Repository<Elem> repo_;

        public PopStack(Repository<Elem> repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Elem elem;
            try
            {
                elem = repo_.stack.pop();
                for (int i = 0; i < repo_.locations.Count; ++i)
                {
                    Elem temp = repo_.locations[i];
                    {
                        if (elem.type == temp.type)
                        {
                            if (elem.name == temp.name)
                            {
                                if ((repo_.locations[i]).end == 0)
                                {
                                    (repo_.locations[i]).end = repo_.semi.lineCount;
                                    if (elem.type == "function")
                                    {
                                        (repo_.locations[i]).funcComplexity = noOfScopes;
                                        noOfScopes = 1;
                                        countScopes = false;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                Console.Write("popped empty stack on semiExp: ");
                semi.display();
                return;
            }
            CSsemi.CSemiExp local = new CSsemi.CSemiExp();
            local.Add(elem.type).Add(elem.name);
            // if(local[0] == "control" )
            //  return;

            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount);
                Console.Write("leaving  ");
                string indent = new string(' ', 2 * (repo_.stack.count + 1));
                Console.Write("{0}", indent);
                this.display(local); // defined in abstract action
            }
        }
    }
    ///////////////////////////////////////////////////////////
    // action to print function signatures - not used in demo

    public class PrintFunction : AAction
    {
        Repository<Elem> repo_;

        public PrintFunction(Repository<Elem> repo)
        {
            repo_ = repo;
        }
        public override void display(CSsemi.CSemiExp semi)
        {
            Console.Write("\n    line# {0}", repo_.semi.lineCount - 1);
            Console.Write("\n    ");
            for (int i = 0; i < semi.count; ++i)
                if (semi[i] != "\n" && !semi.isComment(semi[i]))
                    Console.Write("{0} ", semi[i]);
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            this.display(semi);
        }
    }
    /////////////////////////////////////////////////////////
    // concrete printing action, useful for debugging

    public class Print : AAction
    {
        Repository<Elem> repo_;

        public Print(Repository<Elem> repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
            this.display(semi);
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    //           Rules for Relation Ananlysis
    //
    /////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////
    //detect Inheritence between types

    public class DetectInheritance : ARule
    {
        private Repository<Elem>[] repFromParse;

        public DetectInheritance(Repository<Elem>[] repo)
        {
            repFromParse = repo;
        }

        public override bool test(CSemiExp semi)
        {
            int index = semi.Contains(":");
            if (index != -1)
            {
                foreach (Repository<Elem> repo in repFromParse)
                {
                    foreach (Elem e in repo.locations)
                    {
                        if (semi[index + 1] == e.name && (e.type=="class"|| e.type == "interface"))
                        {
                            CSsemi.CSemiExp local = new CSemiExp();
                            local.Add("Inheritence");
                            local.Add(semi[index - 2]);                             //need to do about implemts interface
                            local.Add(semi[index - 1]);
                            local.Add("inherits");
                            local.Add(e.type);
                            local.Add(semi[index + 1]);
                            string fileName = repo.AttachedFile;
                            int index1 = fileName.LastIndexOf("\\");
                            string fName = fileName.Substring(index1 + 1, fileName.Length - 1 - index1);
                            local.Add(fName);
                            local.Add(repo.AttachedFile);
                            local.displayNewLines = false;
                            doActions(local);
                            return true;
                        }
                    }
                }
            }
            return false;

        }
    }

    /////////////////////////////////////////////////////////
    //detect composition relationship

    public class DetectComposition : ARule             //done  //add filename here ....call from buildanalyser.parse with the file name in constructor

    {
        private Repository<Elem>[] repFromParse;
        int curReposIndex;
        List<string> filesInRule;

        public DetectComposition(Repository<Elem>[] repo, int i,List<string> files)
        {
            repFromParse = repo;
            curReposIndex = i;
            filesInRule = files;
        }

        public override bool test(CSemiExp semi)
        {
            int index = semi.Contains("struct");
            if(index != -1)
            {
                foreach(Elem e in repFromParse[curReposIndex].locations)
                {
                    //if struct is defined inside class in this file - files[index] 
                    if(semi.lineCount > e.begin && semi.lineCount<e.end && e.type == "class" )
                    {
                        CSemiExp local = new CSemiExp();
                        local.Add("Composition");
                        local.Add(semi[index]);
                        local.Add(semi[index + 1]);
                        local.Add("is part-of");
                        string fileName = repFromParse[curReposIndex].AttachedFile;
                        int index1 = fileName.LastIndexOf("\\");
                        string fName = fileName.Substring(index1 + 1, fileName.Length - 1 - index1);
                        local.Add(fName);
                        local.Add(e.type);
                        local.Add(e.name);
                        local.displayNewLines = false;
                        doActions(local);
                        return true;
                    }
                }
                    //struct is defined outside class and can be used anywhere else
                    //search every file for this struct declaration
                int j=0;
                foreach(object file in filesInRule)
                {
                    CSsemi.CSemiExp semiInRule = new CSsemi.CSemiExp();
                    semiInRule.displayNewLines = false;
                    if (!semiInRule.open(file as string))
                    {
                        Console.Write("\n  Can't open {0} in DetectComposition Rule\n\n", file);
                        return false;
                    }

                    while (semiInRule.getSemi())
                    {
                        if(semiInRule.Contains(semi[index+1]) != -1)
                        {
                            foreach(Elem e in repFromParse[j].locations)
                            {
                                if(semiInRule.lineCount > e.begin && semiInRule.lineCount<e.end && e.type == "class" )
                                {
                                    CSemiExp local = new CSemiExp();
                                    local.Add("Composition");
                                    local.Add("struct");
                                    local.Add(semi[index + 1]);
                                    local.Add("is part-of");
                                    string fileName = repFromParse[j].AttachedFile;
                                    int index1 = fileName.LastIndexOf("\\");
                                    string fName = fileName.Substring(index1 + 1, fileName.Length - 1 - index1);
                                    local.Add(fName);
                                    local.Add("e.type");
                                    local.Add(e.name);
                                    local.displayNewLines = false;
                                    doActions(local);

                                }
                            }
                        }
                    }
                    j++;

                }

                return true;  
            }
            return false;
        }  
    }
    
    /////////////////////////////////////////////////////////
    //detect aggregation

    public class DetectAggregation : ARule
    {
        private Repository<Elem>[] repFromParse;
        int repoIndex;
        string qname;

        public DetectAggregation(Repository<Elem>[] repArray, int i)
        {
            repFromParse = repArray;
            repoIndex = i;
        }

        public string namespacecheck(CSemiExp semi, int index ,bool reset)
        {
            if(reset)
                 qname ="";
            foreach(Repository<Elem> rep in repFromParse)
            {
                foreach(Elem e in rep.locations)
                {
                    if (e.name == semi[index] && e.type == "class")
                    {
                        qname += e.name;
                        return qname;
                    }
                    else if(e.name == semi[index] && e.type == "namespace")
                    {
                        qname = e.name + ".";
                        namespacecheck(semi, index + 2, false);
                        break;
                    }
                }
            }
            return qname;
        }
     
        public override bool test(CSemiExp semi)
        {

            int index = semi.Contains("new");
            if (index != -1)
            {
                //check for namespaces
                int i=0;
                while (semi[i] == "\n")
                    i++;
                string qualifiedName = namespacecheck(semi, i, true);
                if (qualifiedName != "")
                {
                    foreach (Elem e in repFromParse[repoIndex].locations)
                    {
                        //search for the containing class of this semi-expression 
                        if (semi.lineCount > e.begin && semi.lineCount < e.end && e.type == "class")
                        {
                            CSemiExp local = new CSemiExp();
                            local.Add("Aggregation");
                            local.Add(qualifiedName);                      //could contain >
                            local.Add(semi[index - 2]);
                            local.Add("is part-of");
                            string fileName = repFromParse[repoIndex].AttachedFile;
                            int index1 = fileName.LastIndexOf("\\");
                            string fName = fileName.Substring(index1 + 1, fileName.Length - 1 - index1);
                            local.Add(fName);
                            local.Add(e.type);
                            local.Add(e.name);
                            doActions(local);
                            return true;

                        }
                    }
                }
                
                return false;
            }
            return false;
        }
    }
    
    ////////////////////////////////////////////////////////
    //detect using Relationship

    public class DetectUsing : ARule
    {
        Repository<Elem>[] repFromParse;
        int currentIteration;

        public DetectUsing(Repository<Elem>[] repoArray, int i)
        {
            repFromParse = repoArray;
            currentIteration = i;
        }

        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }

        //function to split function arguments in seperate lists
        public List<string>[] splitFunctionArgs(CSemiExp semi, int index, int index2)
        {
            List<String>[] argsLists;
            int i = 0;
            int countcomma = 0;
            while (i < semi.count)
            {
                if (semi[i] == ",")
                    countcomma++;
                i++;
            }
            int commacount = countcomma;
            argsLists = new List<string>[countcomma + 1];
            int cindex = index + 1;
            countcomma = 0;
            for (i = index; i <= index2; i++)
            {
                if (semi[i] == ",")
                {
                    argsLists[countcomma] = new List<string>();

                    for (int j = cindex; j < i; j++)
                    {
                        argsLists[countcomma].Add(semi[j]);
                    }
                    cindex = i + 1;
                    countcomma++;
                }
            }

            if (countcomma == commacount)
            {
                argsLists[countcomma] = new List<string>();
                for (int j = cindex; j < index2; j++)
                {
                    argsLists[countcomma].Add(semi[j]);
                }
            }


            return argsLists;
        }

        string qname;

        public string namespacecheck(List<string> semi, int index, bool reset)
        {
            if (reset)
                qname = "";
            foreach (Repository<Elem> rep in repFromParse)
            {
                foreach (Elem e in rep.locations)
                {
                    if (e.name == semi[index] && e.type == "class")
                    {
                        qname += e.name;
                        return qname;
                    }
                    else if (e.name == semi[index] && e.type == "namespace")
                    {
                        qname = e.name + ".";
                        namespacecheck(semi, index + 2, false);
                        break;
                    }
                }
            }
            return qname;
        }

        public override bool test(CSsemi.CSemiExp semi)
        {
            if (semi[semi.count - 1] != "{")
                return false;
            string qualifiedName;
            int index = semi.FindFirst("(");
            int index2 = semi.FindFirst(")");
            if (index2 != index + 1)
            {
                if (index > 0 && !isSpecialToken(semi[index - 1]))
                {
                    //split function arguments
                    List<string>[] argumentLists = splitFunctionArgs(semi, index, index2);
                    //check for each repository in the list
                    foreach (List<string> argsList in argumentLists)
                    {
                        qualifiedName = namespacecheck(argsList, 0, true);
                        if (qualifiedName != "")
                        {
                            foreach (Elem el in repFromParse[currentIteration].locations)
                            {

                                if (semi.lineCount > el.begin && semi.lineCount < el.end && el.type == "class")
                                {
                                    CSemiExp local = new CSemiExp();
                                    local.Add("Using");
                                    local.Add(el.type);
                                    local.Add(el.name);
                                    local.Add("uses");
                                    string fileName = repFromParse[currentIteration].AttachedFile;
                                    int index1 = fileName.LastIndexOf("\\");
                                    string fName = fileName.Substring(index1 + 1, fileName.Length - 1 - index1);
                                    local.Add(fName);
                                    local.Add(qualifiedName);
                                    local.Add(argsList[argsList.Count-1]);
                                    doActions(local);
                                    return true;
                                }
                            }
                        }
                    }
                   
                }
                return false;
            }
            return false;
        }
    }



    /////////////////////////////////////////////////////////
    // rule to detect namespace declarations

    public class DetectNamespace : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("namespace");
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////
    // rule to dectect class definitions

    public class DetectClass : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int indexCL = semi.Contains("class");
            int indexIF = semi.Contains("interface");
            int indexST = semi.Contains("struct");
            int indexEN = semi.Contains("Enum");

            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add(semi[index]).Add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////
    // rule to dectect function definitions

    public class DetectFunction : ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(CSsemi.CSemiExp semi)
        {
            if (semi[semi.count - 1] != "{")
                return false;

            int index = semi.FindFirst("(");
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                local.Add("function").Add(semi[index - 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    /////////////////////////////////////////////////////////
    // detect entering anonymous scope
    // - expects namespace, class, and function scopes
    //   already handled, so put this rule after those
    public class DetectAnonymousScope : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("{");
            if (index != -1)
            {
                CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                // create local semiExp with tokens for type and name
                local.displayNewLines = false;
                local.Add("control").Add("anonymous");
                doActions(local);
                return true;
            }
            return false;
        }
    }

    ////////////////////////////////////////////////////////
    //detect braceless scopes

    public class DetectBracelessScopes : ARule                                                          //if else for switch case while do while                                          
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "else if", "else", "try", "do", "switch", "case" };

            foreach (string stoken in SpecialToken)
            {
                if (semi.Contains(stoken) != -1)
                {
                    CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                    // create local semiExp with tokens for type and name
                    local.displayNewLines = false;

                    local.Add("bracelesScope").Add(stoken);
                    doActions(local);
                    return true;
                }

            }
            return false;
        }
    }

    /////////////////////////////////////////////////////////
    // detect leaving scope

    public class DetectLeavingScope : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            int index = semi.Contains("}");
            if (index != -1)
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }

}
    

/******************This cose has been shifted to Analyser for modularity and remove dependency issue */
  /*public class BuildCodeAnalyzer
  {
    Repository repo ;

    public BuildCodeAnalyzer(Repository rep, CSsemi.CSemiExp semi)
    {
      repo = rep;  
      repo.semi = semi;
    }
    public virtual Parser build()
    {
      Parser parser = new Parser();

      // decide what to show
      AAction.displaySemi = true;
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
  }*/
 


            //int index = semi.Contains(".");
            //int index2 = semi.Contains("new");
            //string qname = semi[index2 + 1];
            //int j;
            //if (semi.Contains(".") != -1)
            //{
            //    if (index > index2)
            //    {
            //        for (j = index2 + 2; j < semi.count; j++)
            //        {
            //            if (semi[j] == ".")
            //            {
            //                qname = qname + "." + semi[j + 1];
            //                j = j + 2;
            //                continue;
            //            } 
            //        }
            //        return qname;
